module Program

open System
open System.Net.WebSockets
open System.Threading
open System.Threading.Tasks
open Saturn
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Database

type ChatsRequestDto = {
    AccountId: int
}

let getChats (next: HttpFunc) (ctx: HttpContext) =
    task {
        let! request = ctx.BindJsonAsync<ChatsRequestDto>()
        let chats = query {
            for c in Database.ctx.Chats do
            where (c.Sender.Id = request.AccountId || c.Receiver.Id = request.AccountId)
            select c
        }
        return! json (chats |> Seq.toArray) next ctx
    }

// type MessagesRequestDto = {
//     ChatId: int
// }

// let getMessages (next: HttpFunc) (ctx: HttpContext) =
//         task {
//             let! request = ctx.BindJsonAsync<MessagesRequestDto>()
//             let messages = query {
//                 for m in Database.ctx.Messages do
//                 where (m.)
//             }
//         }

type WebSocketRequestDto = {
    SenderId: int
    ReceiverId: int
}

let saveMessage (author: Database.Account) (text: string) =
    let message: Database.Message = {
        Id = 0
        Text = text
        Date = DateTime.Now
        Author = author
    }

    Database.ctx.Messages.Add(message) |> ignore
    Database.ctx.SaveChanges() |> ignore


let webSocketHandler (context: HttpContext) (webSocket: WebSocket) (sender: Database.Account) =
    task {
        let buffer = Array.zeroCreate<byte> 1024
        let segment = ArraySegment<byte>(buffer)
        let cancellationToken = context.RequestAborted

        while webSocket.State = WebSocketState.Open do
            let! result = webSocket.ReceiveAsync(segment, cancellationToken)
            if result.MessageType = WebSocketMessageType.Text then
                let message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count)
                printfn "Received: %s" message

                saveMessage sender message

                let response = message
                let responseBytes = System.Text.Encoding.UTF8.GetBytes(response)
                let responseSegment = ArraySegment<byte>(responseBytes)
                do! webSocket.SendAsync(responseSegment, WebSocketMessageType.Text, true, cancellationToken)
            elif result.MessageType = WebSocketMessageType.Close then
                do! webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken)
    }

let getWebSocket (next: HttpFunc) (ctx: HttpContext) =
    task {
        let! request = ctx.BindJsonAsync<WebSocketRequestDto>()

        let mutable checkAccountsExists = true

        let accountQuery (id: int) =
            try
                let senderQuery = (query {
                    for a in Database.ctx.Accounts do
                    where (a.Id = id)
                    select a
                } |> Seq.toArray)

                Some (senderQuery |> Seq.item 0)
                
            with
                | _ ->
                    checkAccountsExists <- false
                    None

        let sender = accountQuery request.SenderId
        accountQuery request.ReceiverId |> ignore

        if ctx.WebSockets.IsWebSocketRequest && checkAccountsExists then
            let! webSocket = ctx.WebSockets.AcceptWebSocketAsync()
            match sender with
                | Some res -> do! webSocketHandler ctx webSocket res
                | None -> ()
            return Some ctx
        else
            return None
    }

let chatRouter =
    choose [
        POST >=> route "/get-chats" >=> getChats
        POST >=> route "/ws" >=> getWebSocket
    ]

let app = application {
    use_router chatRouter
    app_config (fun app ->
        app.UseWebSockets()
    )
}

run app
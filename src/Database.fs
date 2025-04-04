module Database

open Microsoft.Extensions.Configuration
open Microsoft.EntityFrameworkCore
open System
open System.IO

module Database =
    [<CLIMutable>]
    type Currency = {
        Id: int
        Name: string
    }

    [<CLIMutable>]
    type Money = {
        Id: int
        Amount: decimal
        Currency: Currency
    }

    [<CLIMutable>]
    type CardType = {
        Id: int
        Name: string
    }

    [<CLIMutable>]
    type Card = {
        Id: int
        Number: string
        Balance: Money
        CardType: CardType
    }

    [<CLIMutable>]
    type Wallet = {
        Id: int
        Name: string
        Currency: Currency
        Cards: Card list
    }

    [<CLIMutable>]
    type Account = {
        Id: int
        FirstName: string
        LastName: string
        RegDate: DateTime
        Wallets: Wallet list
    }

    [<CLIMutable>]
    type Message = {
        Id: int
        Text: string
        Date: DateTime
        Author: Account
    }

    [<CLIMutable>]
    type Chat = {
        Id: int
        Sender: Account
        Receiver: Account
        Messages: Message list
    }

    let getConnectionString() =
        let configuration =
            ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
                .Build()

        configuration.GetConnectionString("DefaultConnection")

    type DatabaseContext() =
        inherit DbContext()

        member val Currencies = base.Set<Currency>() with get

        member val Monies = base.Set<Money>() with get

        member val CardTypes = base.Set<CardType>() with get

        member val Cards = base.Set<Card>() with get

        member val Wallets = base.Set<Wallet>() with get

        member val Accounts = base.Set<Account>() with get

        member val Messages = base.Set<Message>() with get

        member val Chats = base.Set<Chat>() with get

        override _.OnConfiguring (optionsBuilder: DbContextOptionsBuilder): unit = 
            // base.OnConfiguring(optionsBuilder: DbContextOptionsBuilder)
            optionsBuilder.UseSqlServer(getConnectionString()) |> ignore
    
    let ctx = new DatabaseContext()
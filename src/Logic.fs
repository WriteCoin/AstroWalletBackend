module Logic

open System

type Currency = Currency of string

type Money = {
    Amount: decimal
    Currency: Currency
}

type CardType = CardType of string

type Card = private {
    Number: string
    Balance: Money
    CardType: CardType
}

module Card =
    let isValid (number: string) (balance: Money) (cardType: CardType) =
        number.Length = 16 && balance.Amount > 0M

    let create (number: string) (balance: Money) (cardType: CardType) =
        if isValid number balance cardType then
            Ok {
                Number = number
                Balance = balance
                CardType = cardType
            }
        else
            Error "Не удалось создать карту"

type Wallet = {
    Name: string
    Currency: Currency
    Cards: Card list
}

type Account = {
    FirstName: string
    LastName: string
    Wallets: Wallet list
}

type Message = {
    text: string
    date: DateTime
}

type Chat = private {
    Sender: Account
    Receiver: Account
    Messages: Message list
}

module Chat =
    let isValid (sender: Account) (receiver: Account) =
        sender.FirstName <> receiver.FirstName && sender.LastName <> receiver.LastName

    let create (sender: Account) (receiver: Account) (messages: Message list) =
        if isValid sender receiver then
            Ok {
                Sender = sender
                Receiver = receiver
                Messages = messages
            }
        else
            Error "Не удалось создать чат"

type Messenger = Chat list
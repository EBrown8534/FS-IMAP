open System
open System.Net.Sockets
open System.Text
open EBrown.Imap.Core

[<EntryPoint>]
let main argv = 
    use server = Server().Start()
    Console.ReadLine() |> ignore
    printfn "Closing..."
    0

open System
open System.Net.Sockets
open System.Text
open EBrown.Imap.Core
open EBrown.Imap.Server.Storage

[<EntryPoint>]
let main argv = 
    use server = Server(getStorageEvents()).Start()
    Console.ReadLine() |> ignore
    printfn "Closing..."
    0

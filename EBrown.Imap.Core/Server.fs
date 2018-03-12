namespace EBrown.Imap.Core
open EBrown.Tcp.Socket
open EBrown.Imap.Core.Commands
open System.Net.Sockets
open System.Text
open System.Threading

type User = { Id : string }
type StorageEvents =
    { Authenticate : string * string -> User option
      GetUidValidity : User -> int
      GetNextUniqueId : User -> int }

type Server (storageEvents) =
    let objectToStr o = o.ToString()
    let mutable sockets : (Socket * User option) list = []
    let dispatch (socket : Socket) (user : User option) (command : ClientCommand) =
        async {
            let sendResult (x : string) = async { return! x |> Encoding.ASCII.GetBytes |> socket.AsyncSend }
            let output = 
                match command.Command with
                | ClientCommandName.Logout -> { ServerCommand.Tag = command.Tag; Command = ServerCommandName.Logout Ok }
                | ClientCommandName.Capability -> { ServerCommand.Tag = command.Tag; Command = ServerCommandName.Capability (Ok, [|"IMAP4rev1"; "AUTH=PLAIN"|]) }
                | ClientCommandName.Noop -> { ServerCommand.Tag = command.Tag; Command = ServerCommandName.Noop Ok }
                |> generateServerCommand
            do output |> Array.map sendResult |> Array.map (Async.RunSynchronously) |> ignore }
    let rec runClient (socket : Socket) =
        let printfn = printfn "Socket %s: %s" (socket.RemoteEndPoint.ToString())
        let user = Some { Id = "ebrown@example.com" }
        async {
            let! buffer = () |> socket.AsyncReceiveAll
            let str = buffer |> Encoding.ASCII.GetString
            sprintf "Received (%i bytes): %s" buffer.Length str |> printfn
            match str |> parseClientCommand with
            | Some c ->
                match c.Command with
                | ClientCommandName.Logout _ ->
                    do! c |> dispatch socket user |> Async.Ignore
                    return ()
                | _ ->
                    do! c |> dispatch socket user |> Async.Ignore
                    return! socket |> runClient
            | None ->
                sprintf "Unrecognized command: %s" str |> printfn
                return! socket |> runClient }
    member this.OnConnect (socket : Socket) =
        async {
            try
                lock sockets (fun () -> sockets <- (socket, None)::sockets)
                sockets |> List.length |> printfn "Sockets open: %i"
                return! socket |> runClient
            finally
                lock sockets (fun () -> sockets <- sockets |> List.filter (fst >> (<>) socket))
                SocketShutdown.Both |> socket.Shutdown |> socket.Close
                sockets |> List.length |> printfn "Sockets open: %i" }
    member this.OnClose () =
        lock sockets (fun () ->
            let byeLine = generateLine Untagged [||] "BYE" [|"IMAP4rev1 Server logging out" |> Some|]
            sockets
            |> List.toArray
            |> Array.filter (fun (sock, _) -> sock.Connected)
            |> Array.map (fun (sock, user) -> [| byeLine |> Encoding.ASCII.GetBytes |] |> Array.concat |> sock.AsyncSend)
            |> Async.Parallel
            |> Async.RunSynchronously
            |> ignore)
    member this.Start () = EBrown.Tcp.Server.Start { Connect = this.OnConnect; Close = this.OnClose } 143

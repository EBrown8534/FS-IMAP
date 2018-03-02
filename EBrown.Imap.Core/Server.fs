namespace EBrown.Imap.Core
open EBrown.Tcp.Socket
open System.Net.Sockets
open System.Text
open System.Threading

type User = { Id : string }

type Server () =
    let mutable sockets : (Socket * User option) list = []
    let rec runClient (socket : Socket) =
        let printfn = printfn "Socket %s: %s" (socket.RemoteEndPoint.ToString())
        async {
            let! buffer = () |> socket.AsyncReceiveAll
            let str = buffer |> Encoding.ASCII.GetString
            sprintf "Received (%i bytes): %s" buffer.Length str |> printfn
            if str.Length = 3 && str = "BYE" then printfn "Disconnected"
            else
                let! bytesSent = [| "Hello, other world!"B |] |> Array.concat |> socket.AsyncSend
                bytesSent |> sprintf "Sent response (%i bytes)" |> printfn
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
            sockets
            |> List.toArray
            |> Array.filter (fun (sock, _) -> sock.Connected)
            |> Array.map (fun (sock, user) -> [| "BYE"B |] |> Array.concat |> sock.AsyncSend)
            |> Async.Parallel
            |> Async.RunSynchronously
            |> ignore)
    member this.Start () = EBrown.Tcp.Server.Start { Connect = this.OnConnect; Close = this.OnClose } 143

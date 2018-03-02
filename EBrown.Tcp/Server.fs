namespace EBrown.Tcp
open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading
open EBrown.Tcp
open EBrown.Tcp.Socket

/// Defines our TCP/IP Server
type Server () =
    static member StartI events port (ipAddress : IPAddress) =
        let cts = new CancellationTokenSource ()
        let server = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        (ipAddress, port) |> IPEndPoint |> server.Bind
        SocketOptionName.MaxConnections |> int |> server.Listen
        (() |> ipAddress.ToString, port) ||> printfn "Started listening on %s:%d"
    
        let rec waitForConnection () = 
            async {
                printfn "Waiting for connection..."
                let! socket = () |> server.AsyncAccept
                () |> socket.RemoteEndPoint.ToString |> printfn "Socket connected: %s"
                try Async.Start(socket |> events.Connect, cancellationToken = cts.Token)
                with e -> e.ToString() |> printfn "An error occurred: %s"
                return! () |> waitForConnection }
        Async.Start(() |> waitForConnection, cancellationToken = cts.Token)

        { new IDisposable with
            member this.Dispose () =
                () |> events.Close
                () |> cts.Cancel
                () |> server.Close
                () |> cts.Dispose
                () |> server.Dispose }
    static member Start events port = (events, port, IPAddress.Any) |||> Server.StartI 

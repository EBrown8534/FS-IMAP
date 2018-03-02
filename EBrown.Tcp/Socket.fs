module EBrown.Tcp.Socket
open System.Net.Sockets

/// Add definitions to the `System.Net.Sockets.Socket`
type Socket with
    member socket.AsyncAccept () = Async.FromBeginEnd(socket.BeginAccept, socket.EndAccept)
    member socket.AsyncReceive (buffer : byte array, ?offset, ?count) =
        Async.FromBeginEnd(
            buffer, 
            defaultArg offset 0, 
            defaultArg count buffer.Length, 
            (fun (buffer, offset, size, callback, state) -> socket.BeginReceive(buffer, offset, size, SocketFlags.None, callback, state)), 
            socket.EndReceive)
    member socket.AsyncSend (buffer : byte array, ?offset, ?count) =
        Async.FromBeginEnd(
            buffer,
            defaultArg offset 0,
            defaultArg count buffer.Length,
            (fun (buffer, offset, size, callback, state) -> socket.BeginSend(buffer, offset, size, SocketFlags.None, callback, state)),
            socket.EndSend)
    member socket.AsyncReceiveAll () =
        let rec receive buffer =
            async {
                let tempBuffer = 1024 |> Array.zeroCreate
                let! bytesReceived = tempBuffer |> socket.AsyncReceive
                let buffer = [|buffer; tempBuffer.[0..bytesReceived - 1]|] |> Array.concat
                if bytesReceived < tempBuffer.Length then return buffer else return! buffer |> receive }
        [||] |> receive

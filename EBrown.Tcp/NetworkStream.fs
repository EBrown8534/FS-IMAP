module EBrown.Tcp.NetworkStream
open System.Net.Sockets

/// Add definitions to the `System.Net.Sockets.NetworkStream`
type NetworkStream with
    member stream.AsyncRead (buffer : byte array, ?offset, ?count) =
        Async.FromBeginEnd(
            buffer, 
            defaultArg offset 0, 
            defaultArg count buffer.Length, 
            stream.BeginRead, 
            stream.EndRead)
    member stream.AsyncReadAll () =
        let rec receive buffer =
            async {
                let tempBuffer = 1024 |> Array.zeroCreate
                let! bytesReceived = tempBuffer |> stream.AsyncRead
                let buffer = [|buffer; tempBuffer.[0..bytesReceived - 1]|] |> Array.concat
                if bytesReceived < tempBuffer.Length then return buffer else return! buffer |> receive }
        [||] |> receive
    member stream.ReadAll () =
        let rec receive buffer =
            let tempBuffer = 1024 |> Array.zeroCreate
            let bytes = (tempBuffer, 0, tempBuffer.Length) |> stream.Read
            let buffer = [|buffer; tempBuffer.[0..bytes - 1]|] |> Array.concat
            if bytes < tempBuffer.Length then buffer else buffer |> receive
        [||] |> receive
    member stream.Write (buffer : byte array) = stream.Write(buffer, 0, buffer.Length)

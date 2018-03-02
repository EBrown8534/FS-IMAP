open System.Net.Sockets
open System
open System.Text
open EBrown.Tcp.NetworkStream
open System.Threading

[<EntryPoint>]
let main argv = 
    printfn "Press [Enter] / [Return] to quit, any other character to send data."
    let cts = new CancellationTokenSource ()
    use client = new TcpClient("127.0.0.1", 143)
    printfn "Connected to %s" (client.Client.RemoteEndPoint.ToString())
    use stream = client.GetStream()

    let sendData = 
        async {
            while (Console.ReadKey().Key <> ConsoleKey.Enter) do
                if not cts.IsCancellationRequested then
                    printfn ""
                    [| "Hello world!"B |] |> Array.concat |> stream.Write
            [| "BYE"B |] |> Array.concat |> stream.Write
            printfn "Disconnected" }
    let receiveData = 
        async {
            let rec loop () =
                async {
                    let! bytes = () |> stream.AsyncReadAll
                    let str = bytes |> Encoding.ASCII.GetString
                    printfn "Received %i bytes: %s" bytes.Length str
                    if str.Length = 3 && str = "BYE" then printfn "Disconnected, press any key to exit."; cts.Cancel() else return! () |> loop }
            return! () |> loop }
    Async.Start(receiveData, cancellationToken = cts.Token)
    try Async.RunSynchronously(sendData, cancellationToken = cts.Token)
    with | :? OperationCanceledException -> () | e -> printfn "%s" (e.ToString()); printfn "Press enter to exit..."; Console.ReadLine() |> ignore
    0

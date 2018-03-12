open System.Net.Sockets
open System
open System.Text
open EBrown.Tcp.NetworkStream
open System.Threading
open System.Threading.Tasks

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
                    [| "abcd1234 CAPABILITY"B |] |> Array.concat |> stream.Write
            [| "client LOGOUT"B |] |> Array.concat |> stream.Write
            printfn "Disconnected" }
    let receiveData = 
        async {
            let rec loop () =
                async {
                    let! bytes = () |> stream.AsyncReadAll
                    let str = bytes |> Encoding.ASCII.GetString
                    printfn "Received %i bytes: %s" bytes.Length str
                    if str.StartsWith("* BYE") then
                        printfn "Disconnected, press any key to exit."
                        cts.Cancel()
                    else return! () |> loop }
            return! () |> loop }
    let t = Async.StartAsTask(receiveData, cancellationToken = cts.Token)
    try Async.RunSynchronously(sendData, cancellationToken = cts.Token)
    with
    | :? OperationCanceledException -> ()
    | e -> printfn "%s" (e.ToString()); printfn "Press enter to exit..."; Console.ReadLine() |> ignore
    try t.Wait()
    with
    | :? AggregateException as ex when ex.InnerExceptions |> Seq.exists (fun (ex : Exception) -> match ex with | :? TaskCanceledException -> true | _ -> false) -> ()
    | e -> printfn "%s" (e.ToString()); printfn "Press enter to exit..."; Console.ReadLine() |> ignore
    0

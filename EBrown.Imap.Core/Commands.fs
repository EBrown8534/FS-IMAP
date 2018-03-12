module EBrown.Imap.Core.Commands

type Tag = | Untagged | Tagged of string
let joinStrings (sep : string) (sarr : string array) = System.String.Join(sep, sarr)
let getTag = function | Untagged -> "*" | Tagged s -> s
let generateLine tag largs name rargs = [|[|tag |> getTag |> Some|]; largs; [|name |> Some|]; rargs|] |> Array.concat |> Array.choose id |> joinStrings " "

type ClientCommandName = | Capability | Noop | Logout
type ClientCommand = { Tag : Tag; Command : ClientCommandName }
let generateClientCommandName =
    function
    | Capability -> ("CAPABILITY", [||])
    | Noop -> ("NOOP", [||])
    | Logout -> ("LOGOUT", [||])
let generateClientCommand (command : ClientCommand) = command.Command |> generateClientCommandName ||> generateLine command.Tag [||]
let parseClientCommand (command : string) =
    let parseTag = function | "*" -> Untagged | s -> Tagged s
    let parseCommandName =
        function
        | [|"CAPABILITY"|] -> Capability |> Some
        | [|"NOOP"|] -> Noop |> Some
        | [|"LOGOUT"|] -> Logout |> Some
        | _ -> None
    let parts = command.Split(' ')
    parts.[1..] |> parseCommandName |> Option.map (fun c -> { ClientCommand.Tag = parts.[0] |> parseTag; Command = c })

type OkBadResult = | Ok | Bad
let getOkBad = function | Ok -> ("OK", "completed") | Bad -> ("BAD", "")
type ServerCommandName = | Capability of OkBadResult * string array | Noop of OkBadResult | Logout of OkBadResult
type ServerCommand = { Tag : Tag; Command : ServerCommandName }
let generateServerCommandName (command : ServerCommand) =
    match command.Command with 
    | Capability (res, options) ->
        let lRes, rRes = res |> getOkBad
        [|(Untagged, [||], "CAPABILITY", options); (command.Tag, [|lRes|], "CAPABILITY", [|rRes|])|]
    | Noop res ->
        let lRes, rRes = res |> getOkBad
        [|command.Tag, [|lRes|], "NOOP", [|rRes|]|]
    | Logout res ->
        let lRes, rRes = res |> getOkBad
        [|(Untagged, [||], "BYE", [|"IMAP4rev1 Server logging out"|]); command.Tag, [|lRes|], "LOGOUT", [|rRes|]|]
let generateServerCommand (command : ServerCommand) = command |> generateServerCommandName |> Array.map (fun (t, l, n, r) -> generateLine t (l |> Array.map Some) n (r |> Array.map Some))

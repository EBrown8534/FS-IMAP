module EBrown.Imap.Server.Storage
open EBrown.Imap.Core

type Configuration =
    { Password : string
      UidValididty : int
      NextUniqueId : int }
let getConfig (f : string) =
    let lines = System.IO.Path.Combine(@"DemoFiles", f, @"Configuration.txt") |> System.IO.File.ReadAllLines
    { Password = lines.[0]; UidValididty = lines.[1] |> int; NextUniqueId = lines.[2] |> int }
let getStorageEvents () = 
    { Authenticate = (fun (u, p) -> if (u |> getConfig).Password = p then Some { Id = u } else None)
      GetUidValidity = (fun u -> (u.Id |> getConfig).UidValididty)
      GetNextUniqueId = (fun u -> (u.Id |> getConfig).NextUniqueId) }

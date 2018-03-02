namespace EBrown.Tcp
open System.Net.Sockets
type ServerEvents =
    { Connect : Socket -> Async<unit>
      Close : unit -> unit }

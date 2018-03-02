F# IMAP
===

This repository houses an F# IMAP server (or what will become an F# IMAP server), and a demo client to connect to the server.

EBrown.Tcp
---

This is a set of general extensions (quite small) to the .NET TCP/IP stack, as well as a general TCP/IP Server.

EBrown.Imap.Core
---

This is the IMAP TCP/IP Server (extension of the EBrown.Tcp.Server), as well as the basic IMAP TCP/IP stack.

EBrown.Imap.Server
---

This is the actual IMAP server execution, and will also contain the file reading/storage mechanisms for interacting with IMAP emails.

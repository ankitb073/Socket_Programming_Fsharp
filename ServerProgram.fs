module ServerProgram
open System
open System.Net
open System.Net.Sockets
open System.IO
open System.Threading.Tasks

let calculate operation numbers =
    try
        match operation, numbers with
        | "add", _ -> Some (numbers |> List.sum |> string)
        | "subtract", h :: t -> Some ((h - List.sum t) |> string)
        | "multiply", _ -> Some (numbers |> List.fold (*) 1 |> string)
        | _, _ -> None
    with _ -> None

let handleClient (client: TcpClient) (exitFlag: bool ref) =
    async {
        use stream = client.GetStream()
        let reader = new StreamReader(stream)
        let writer = new StreamWriter(stream)
        writer.AutoFlush <- true
        try
            try
                writer.WriteLine("Hello!")
                while true do
                    let! line = reader.ReadLineAsync() |> Async.AwaitTask
                    let parts = line.Split(' ')
                    let command = parts.[0]
                    let operands = parts.[1..]
                    
                    let response =
                        match command, operands with
                        | "bye", _ -> client.Close(); "-5"
                        | "terminate", _ -> exitFlag.Value <- true; "-5"
                        | op, _ when (op <> "add" && op <> "subtract" && op <> "multiply") -> "-1"
                        | _, arr when arr.Length < 2 -> "-2"
                        | _, arr when arr.Length > 4 -> "-3"
                        | _, arr when arr |> Array.exists (fun a -> not (System.Int32.TryParse(a, ref 0))) -> "-4"
                        | op, arr -> 
                            let nums = arr |> Array.map Int32.Parse |> Array.toList
                            match calculate op nums with
                            | Some res -> res
                            | None -> "-1"
                    writer.WriteLine(response)
            with ex -> 
                Console.WriteLine("Error: " + ex.Message)
                writer.WriteLine("-5") // Error, force client to exit
        finally
            client.Close()
    }

let runServer (port: int) =
    let listener = new TcpListener(IPAddress.Any, port)
    listener.Start()
    let exitFlag = ref false
    while not exitFlag.Value do
        let client = listener.AcceptTcpClient()
        Async.Start (handleClient client exitFlag)

// Start the server on port 8888
runServer 8888

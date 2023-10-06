module ClientProgram
open System
open System.IO
open System.Net.Sockets
open System.Threading.Tasks

let clientProgram (ip: string) (port: int) =
    async {
        use client = new TcpClient(ip, port)
        use stream = client.GetStream()
        let reader = new StreamReader(stream)
        let writer = new StreamWriter(stream)
        writer.AutoFlush <- true

        // Receive and print Hello message
        let! helloMessage = reader.ReadLineAsync() |> Async.AwaitTask
        Console.WriteLine(helloMessage)

        let running = ref true

        while running.Value do
            // Get command from user
            Console.Write("Sending command: ")
            let command = Console.ReadLine()

            // Send command to server
            do! writer.WriteLineAsync(command) |> Async.AwaitTask

            // Get and print response from server
            let! response = reader.ReadLineAsync() |> Async.AwaitTask

            match response with
            | "-5" -> 
                Console.WriteLine("exit")
                running.Value <- false
            | "-1" -> Console.WriteLine("Server response: incorrect operation command.")
            | "-2" -> Console.WriteLine("Server response: number of inputs is less than two.")
            | "-3" -> Console.WriteLine("Server response: number of inputs is more than four.")
            | "-4" -> Console.WriteLine("Server response: one or more of the inputs contain(s) non-number(s).")
            | res -> Console.WriteLine("Server response: " + res)
    }
    |> Async.RunSynchronously

// Example: Connecting to server at IP 127.0.0.1 and port 8888
clientProgram "127.0.0.1" 8888

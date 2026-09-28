using PingPong.AkkaGame;

Console.WriteLine("=== Ping-Pong (Akka.NET actor system) ===");

var finalRally = await PingPongMatch.RunAsync(5, Console.WriteLine);

Console.WriteLine($"=== Match finished after {finalRally} rallies ===");

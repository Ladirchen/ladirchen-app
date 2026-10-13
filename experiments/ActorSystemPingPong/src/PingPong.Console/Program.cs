using PingPong.Game;

const int Rallies = 5;

Console.WriteLine($"Starting a {Rallies}-rally ping-pong match between two actors...");
Console.WriteLine();

var finalRally = await PingPongMatch.RunAsync(Rallies, Console.WriteLine);

Console.WriteLine();
Console.WriteLine($"Done. Ball crossed the net {finalRally} times.");

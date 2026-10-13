namespace PingPong.AkkaGame;

/// <summary>Sent by the ping actor to volley the ball; carries the current rally count.</summary>
public sealed record Ping(int Rally);

/// <summary>Sent by the pong actor back to whoever served, echoing the rally count.</summary>
public sealed record Pong(int Rally);

/// <summary>Kicks a match off.</summary>
public sealed record Start;

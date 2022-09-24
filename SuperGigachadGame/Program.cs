using SuperGigachadGame;
using SuperGigachadGame.Server;

var port = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PORT")) ? Convert.ToInt32(Environment.GetEnvironmentVariable("PORT")) : 7777;
var isServerOnly = args.Length > 0 && args[0] == "--server" || Environment.GetEnvironmentVariable("SERVER_ONLY") == "true";


if (!isServerOnly)
{
    var game = new Game();

    while (game.Window.IsOpen)
    {
    
        game.Window.DispatchEvents();
        game.Update();
        game.Window.Clear();
        game.Draw();
        game.Window.Display();
    }   
}
else
{
    var server = new GameServer("0.0.0.0", port, false);
    server.Start();
    Console.WriteLine("Started... Port: " + port);
    while (true)
    {
        server.Update();
    }
}

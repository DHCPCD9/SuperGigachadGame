using System.Globalization;
using System.Net;
using SuperGigachadGame;
using SuperGigachadGame.Server;

var port = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PORT")) ? Convert.ToInt32(Environment.GetEnvironmentVariable("PORT")) : 7777;
var isServerOnly = args.Length > 0 && args[0] == "--server" || Environment.GetEnvironmentVariable("SERVER_ONLY") == "true";




if (!isServerOnly)
{
    int clientPort = 0;
    
    Console.Write("Port: ");
    if (!Int32.TryParse(Console.ReadLine(), out clientPort))
    {
        Console.WriteLine("Invalid port");
        return;
    }

    Console.Write("IP: ");
    if (!IPAddress.TryParse(Console.ReadLine(), out var address))
    {
        Console.WriteLine("Invalid address");
        return;
    }
    
    var game = new Game(address, port);
    
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

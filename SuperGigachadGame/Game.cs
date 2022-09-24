using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using SuperGigachadGame.GameObjects;
using SuperGigachadGame.GameObjects.Drawable;
using SuperGigachadGame.Player;
using SuperGigachadGame.Server;

namespace SuperGigachadGame;

public class Game
{
    
    public RenderWindow Window { get; set; }
    public bool Vsync { get; set; }
    public bool ShowDebugInfo { get; set; }
    private Clock PerformanceClock { get; set; } = new Clock();
    public Font DefaultFont { get; set; }
    public LocalPlayer LocalPlayer { get; set; }
    public int Speed { get; set; } = 5;
    public GameServer Server { get; set; }
    public List<RemotePlayer> RemotePlayers { get; } = new();
    public GameClient Client { get; set; }
    public bool IsReady { get; set; }
    public int Score { get; set; }
    public Text ChatBox { get; set; }
    public Text ChatField { get; set; }

    private String _text
    {
        get => ChatField.DisplayedString;
        set => ChatField.DisplayedString = value;
    }

    public bool IsInChat { get; set; }
    public List<DrawableFruit> Fruits { get; } = new();
    public List<Drawable> Drawables { get; } = new();
    public Game()
    {
        Client = new GameClient(this, "0.0.0.0", 7777);
        Client.Connect();
        


        Console.WriteLine("Loading font roboto.ttf");
        DefaultFont = new Font("assets/fonts/roboto.ttf");
        Console.WriteLine("Creating window");
        Window = new RenderWindow(new VideoMode(800, 600), "SuperGigachadGame");
        
        ChatBox = new Text("", DefaultFont, 20);
        ChatBox.DisplayedString += "Welcome to game. Press enter to chat.\n";

        ChatBox.Position = new Vector2f(0, Window.Size.Y - ChatBox.GetLocalBounds().Height - 200);
        
        
        ChatField = new Text("", DefaultFont, 20);
        ChatField.Position = new Vector2f(0, Window.Size.Y - 200);

        Window.Closed += (sender, args) => Window.Close();

        Window.TextEntered += (sender, args) =>
        {
            if (!IsInChat) return;
            if (args.Unicode == "\b")
            {
                if (_text.Length > 0)
                {
                    _text = _text.Remove(_text.Length - 1);
                }
            }
            else
            {
                _text += args.Unicode;
            }
            
        };
        
        Window.KeyPressed += (sender, args) =>
        {

            if (IsInChat && args.Code == Keyboard.Key.Enter)
            {
                IsInChat = false;
              
                var stream = new MemoryStream();
                var writter = new BinaryWriter(stream);
                
                writter.Write((int)PacketType.Chat);
                writter.Write(ChatField.DisplayedString);
                
                Client.Send(stream.ToArray());
                _text = "";
                return;
            }
            
            if (!IsInChat && args.Code == Keyboard.Key.Enter)
            {
                IsInChat = true;
                _text = "";
                return;
            }
            
            
            
            if (IsInChat) return;
            if (!IsReady && args.Code != Keyboard.Key.Escape) return;
            var internalSpeed = Speed;
            if (args.Shift)
            {
                internalSpeed *= 2;
            }
            
           
            if (args.Code == Keyboard.Key.Escape)
            {
                Window.Close();
            }

            if (args.Code == Keyboard.Key.D)
            {
                LocalPlayer.Move(1 * internalSpeed, 0);
            }
            
            if (args.Code == Keyboard.Key.A)
            {
                LocalPlayer.Move(1 * -internalSpeed, 0);
            }
        };
        
        Window.Resized += (sender, args) =>
        {
            Window.SetView(new View(new FloatRect(0, 0, args.Width, args.Height)));
        };
    }
    

    public void Update()
    {
        PerformanceClock.Restart();
    }

    public void Draw()
    {

        var debugString = new StringBuilder();
        debugString.AppendLine("Last frame took " + PerformanceClock.ElapsedTime.AsMicroseconds() / 1000F + "ms");
        debugString.AppendLine("Running on " + RuntimeInformation.FrameworkDescription + " " + RuntimeInformation.OSDescription);

        if (IsReady)
        {
            debugString.AppendLine("Local player position: " + LocalPlayer.Position.X + ", " + LocalPlayer.Position.Y);
            debugString.AppendLine("Points: " + Score);
            debugString.AppendLine("Ping: " + Client.Ping + "ms");
        }
        
        var debugText = new Text(debugString.ToString(), DefaultFont);
        debugText.CharacterSize = 12;
        
        Window.Draw(debugText);
        
        if (IsReady)
        {
            foreach (var player in RemotePlayers)
            {
                Window.Draw(player);
            }

            foreach (var fruits in Fruits)
            {
                Window.Draw(fruits);
            }
            
            Window.Draw(LocalPlayer);
        }
        Window.Draw(ChatBox);

        if (IsInChat)
        {
            Window.Draw(ChatField);
        }
        
        
    }
}
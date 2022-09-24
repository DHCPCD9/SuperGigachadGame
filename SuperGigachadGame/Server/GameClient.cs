using System.Numerics;
using System.Security.Principal;
using SFML.System;
using SuperGigachadGame.GameObjects;
using SuperGigachadGame.GameObjects.Drawable;
using SuperGigachadGame.Player;
using WatsonTcp;

namespace SuperGigachadGame.Server;

public class GameClient
{
    public WatsonTcpClient Client { get; set; }
    public String Hostname { get; set; }
    public int Port { get; set; }
    public String? Username { get; set; }
    private int _ping;
    private DateTimeOffset _lastPing;

    public int Ping
    {
        get
        {
            if (_lastPing.AddSeconds(1) < DateTimeOffset.Now)
            {
                _ping = GetPing();
            }
            
            return _ping;
        }
    }

    private int GetPing()
    {
        _lastPing = DateTimeOffset.Now;
        var buffer = new MemoryStream();
        
        using (var writer = new BinaryWriter(buffer))
        {
            writer.Write((int)PacketType.Ping);
            writer.Write(DateTimeOffset.Now.ToUnixTimeMilliseconds());
            writer.Flush();
        }
        
        Client.Send(buffer.ToArray());
        return 0;
    }


    private Game _game;
    
    public GameClient(Game game, String hostname, int port)
    {
        _game = game;
        Hostname = hostname;
        Port = port;
        Client = new WatsonTcpClient("127.0.0.1", 7777);
        Client.Events.ServerConnected += Events_Connected;
        Client.Events.ServerDisconnected += Events_Disconnected;
        Client.Events.MessageReceived += Events_DataReceived;
    }

    public void Connect()
    {
        Client.Connect();
    }

    private void Events_Disconnected(object? sender, DisconnectionEventArgs e)
    {
        Console.WriteLine("Disconnected: " + e.Reason);
    }

    private void Events_DataReceived(object? sender, MessageReceivedEventArgs e)
    {
        var reader = new BinaryReader(new MemoryStream(e.Data));
        
        var packetId = reader.ReadInt32();
        

        if (packetId == (int)PacketType.PlayerData)
        {
            var uId = reader.ReadInt32();
            var username = reader.ReadString();
            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            var color = reader.ReadString();
            
            Console.WriteLine("Got username: " + username + " ID: " + uId);
            Console.WriteLine("Color: " + color);
            
            Console.WriteLine("Creating local player");
            
            _game.LocalPlayer = new LocalPlayer(_game, color);
            _game.LocalPlayer.ID = uId;
            _game.IsReady = true;
        }

        if (packetId == (int)PacketType.PlayerJoined)
        {
            var uId = reader.ReadInt32();
            var username = reader.ReadString();
            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            var color = reader.ReadString();
            var score = reader.ReadInt32();

            Console.WriteLine("Got username: " + username + " ID: " + uId);
            
            _game.RemotePlayers.Add(new RemotePlayer(_game, uId, username, color, x, y, score));
        }

        if (packetId == (int)PacketType.PlayerPosition)
        {
            var uId = reader.ReadInt32();
            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            
            var player = _game.RemotePlayers.FirstOrDefault(p => p.ID == uId);
            
            if (player != null)
            {
                player.Position = new Vector2i(x, y);
            }
        }
        
        if (packetId == (int)PacketType.PlayerLeft)
        {
            var uId = reader.ReadInt32();
            
            var player = _game.RemotePlayers.FirstOrDefault(p => p.ID == uId);
            
            if (player != null)
            {
                _game.RemotePlayers.Remove(player);
            }
        }

        if (packetId == (int)PacketType.Ping)
        {
            var time = reader.ReadInt64();
            _ping = (int)(DateTimeOffset.Now.ToUnixTimeMilliseconds() - time);
            _lastPing = DateTimeOffset.Now;
        }

        if (packetId == (int)PacketType.FruitSpawn)
        {
       
            
            var type = (FruitType)reader.ReadInt32();
            if (type == FruitType.Apple)
            {
                var apple = new DrawableApple();
                apple.Id = new Guid(reader.ReadBytes(16));
                var x = reader.ReadInt32();
                var y = reader.ReadInt32();
                apple.Position = new Vector2i(x, y);

                _game.Fruits.Add(apple);
            }
        }

        if (packetId == (int)PacketType.FruitMove)
        {
            var type = (FruitType)reader.ReadInt32();
            var id = new Guid(reader.ReadBytes(16));
            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            
            var fruit = _game.Fruits.FirstOrDefault(f => f.Id == id);
            
            if (fruit != null)
            {
                fruit.Position = new Vector2i(x, y);
            }
        }

        if (packetId == (int)PacketType.ScoreUpdate)
        {
            
            var uId = reader.ReadInt32();
            var score = reader.ReadInt32();

            if (uId == _game.LocalPlayer.ID)
            {
                _game.Score = score;
            }
        }

        if (packetId == (int)PacketType.Chat)
        {
            var chatUser = ChatUser.FromReader(reader);
            var message = reader.ReadString();
            
            var formattedMessage = $"{chatUser.Name}: {message}";
            _game.ChatBox.DisplayedString +=  formattedMessage + "\n";
        }
    }

    private void Events_Connected(object? sender, ConnectionEventArgs e)
    {
        Console.WriteLine("Connected!");
        var buffer = new MemoryStream();
        var writter = new BinaryWriter(buffer);
        
        writter.Write((int)PacketType.RequrestData);
        
        Client.Send(buffer.ToArray());
    }
    

    public void Send(byte[] buffer)
    {
        Client.Send(buffer);
    }
}
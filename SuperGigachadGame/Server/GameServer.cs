using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualBasic;
using SFML.System;
using SuperGigachadGame.GameObjects;
using WatsonTcp;

namespace SuperGigachadGame.Server;

public class GameServer
{
    public String Host { get; set; }
    public int Port { get; set; }
    public bool IsInternal { get; set; }
    public WatsonTcpServer Listener { get; set; }
    public List<Client> Clients { get; set; } = new();
    public bool IsStarted { get; set; } = false;
    public int TickRate => 128;
    private Clock TickRateClock { get; } = new();
    public List<BaseFruit> Fruits { get; } = new();
    public int MaxFruits => 16;
    public int MaxFieldX => 600;
    public int MaxFieldY => 800;
    public int Speed => 1;


    public GameServer(String host, int port, bool isInternal)
    {
        Host = host;
        Port = port;
        IsInternal = isInternal;
    }

    public void Start()
    {
        Listener = new WatsonTcpServer("0.0.0.0", Port);
        Listener.Events.ClientConnected += Events_ClientConnected;
        Listener.Events.StreamReceived += Events_StreamReceived;
        Listener.Events.ClientDisconnected += Events_ClientDisconnected;
        Listener.Start();
    }

    private void Events_ClientDisconnected(object? sender, DisconnectionEventArgs e)
    {
        var client = Clients.FirstOrDefault(x => x.Tcp == e.IpPort);
        
        if (client != null)
        {
            Clients.Remove(client);
        }
        //Disconnect packet
        var disconnectPacketStream = new MemoryStream();
        var disconnectPacketWriter = new BinaryWriter(disconnectPacketStream);
        
        disconnectPacketWriter.Write((int)PacketType.PlayerLeft);
        disconnectPacketWriter.Write(client.ID);

        Broadcast(disconnectPacketStream.ToArray());
        
        Console.WriteLine($"Client {e.IpPort} disconnected");
    }

    public void Broadcast(byte[] toArray)
    {
        foreach (var client in Clients.ToList())
        {
            Listener.Send(client.Tcp, toArray);
        }
    }

    private void Events_StreamReceived(object? sender, StreamReceivedEventArgs e)
    {
        var reader = new BinaryReader(new MemoryStream(e.Data));
        var packetId = reader.ReadInt32();

        if (packetId == (int)PacketType.RequrestData)
        {
            var rawPacket = new MemoryStream();
            var binaryWritter = new BinaryWriter(rawPacket);

            var client = new Client
            {
                ID = Clients.Count + 1,
                X = 0,
                Y = 0,
                Username = "User " + Clients.Count + 1,
                Tcp = e.IpPort
            };
        
            client.SetRandomColor();

            var rawPacketToSent = new MemoryStream();
            var binaryWriterToSent = new BinaryWriter(rawPacketToSent);
            binaryWriterToSent.Write((int)PacketType.PlayerJoined);
            binaryWriterToSent.Write(client.ToBuffer());
            Broadcast(rawPacketToSent.ToArray());
        
            foreach (var clientToSent in Clients)
            {
                if (clientToSent.Tcp == client.Tcp) continue;
            
                var rawPacketToSent2 = new MemoryStream();
                var binaryWriterToSent2 = new BinaryWriter(rawPacketToSent2);
                binaryWriterToSent2.Write((int)PacketType.PlayerJoined);
                binaryWriterToSent2.Write(clientToSent.ToBuffer());
            
                Listener.Send(client.Tcp, rawPacketToSent2.ToArray());
            }
        
            //Sending fruits stuff
        
            foreach (var fruit in Fruits)
            {
                Listener.Send(client.Tcp, fruit.ToFruitSpawnPacket());
            }
        
            Clients.Add(client);
            binaryWritter.Write((int)PacketType.PlayerData);
            binaryWritter.Write(client.ToBuffer());
            binaryWritter.Close();
            rawPacket.Close();
            Console.WriteLine("Client connected! (" + e.IpPort + ")" + " ID: " + client.ID);
            Listener.Send(e.IpPort, rawPacket.ToArray());
        }

        if (packetId == (int)PacketType.PlayerPosition)
        {
            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            var client = Clients.FirstOrDefault(c => c.Tcp == e.IpPort);

            if (client == null)
            {
                Listener.DisconnectClient(e.IpPort, MessageStatus.AuthRequired);
                return;
            }
            
            client.X = x;
            client.Y = y;
            
            //Sending movement clients to all clients
            foreach (var c in Clients)
            {
                if (c.Tcp == client.Tcp) continue;
                var stream = new MemoryStream();
                var writer = new BinaryWriter(stream);
                writer.Write((int)PacketType.PlayerPosition);
                writer.Write(client.ID);
                writer.Write(x);
                writer.Write(y);
                writer.Flush();
                Listener.Send(c.Tcp, stream.ToArray());
            }
        }

        if (packetId == (int)PacketType.Ping)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            
            writer.Write((int)PacketType.Ping);
            writer.Write(DateTimeOffset.Now.ToUnixTimeMilliseconds());
        }

        if (packetId == (int)PacketType.Chat)
        {
            var message = reader.ReadString();

            if (message.Trim().StartsWith("/"))
            {
                var command = message.Trim().Split(" ")[0].Substring(1);
                Console.WriteLine($"User {e.IpPort} executed command {command}");
                
                if (command == "help")
                {
                    var stream = new MemoryStream();
                    var writer = new BinaryWriter(stream);
                    
                    writer.Write((int)PacketType.Chat);
                    writer.Write(ChatUser.SYSTEM.ToBuffer());
                    writer.Write("Available commands: /help, /ping, /players");
                    writer.Flush();
                    Listener.Send(e.IpPort, stream.ToArray());
                    return;
                }
            }
            
            var client = Clients.FirstOrDefault(c => c.Tcp == e.IpPort);
            
            if (client == null)
            {
                Listener.DisconnectClient(e.IpPort, MessageStatus.AuthRequired);
                return;
            }
            
            var stream2 = new MemoryStream();
            var writer2 = new BinaryWriter(stream2);
            
            writer2.Write((int)PacketType.Chat);
            writer2.Write(ChatUser.FromClient(client).ToBuffer());
            writer2.Write(message);
            writer2.Flush();
            Broadcast(stream2.ToArray());
            

        }
    }

    private void Events_ClientConnected(object? sender, ConnectionEventArgs e)
    {
        
       
    }

    public void Update()
    {

        if (TickRateClock.ElapsedTime.AsMilliseconds() >= 1000 / TickRate)
        {
            TickRateClock.Restart();
            if (Fruits.Count < MaxFruits)
            {
                var apple = new Apple
                {
                    X = new Random().Next(MaxFieldX),
                    Y = new Random().Next(300, MaxFieldY)
                };
                Fruits.Add(apple);
                
            
                Broadcast(apple.ToFruitSpawnPacket());
            }

            foreach (var fruit in Fruits)
            {
                fruit.Y -= Speed;
            
                if (fruit.Y < 0)
                {
                    fruit.Y = new Random().Next(300, MaxFieldY);
                    fruit.X = new Random().Next(MaxFieldX);
                    break;
                }
                
                foreach (var client in Clients)
                {
                    if (fruit.IsColiding(new Vector2i(client.X, client.Y)))
                    {
                        fruit.Y = new Random().Next(300, MaxFieldY);
                        fruit.X = new Random().Next(MaxFieldX);
                        
                        var stream = new MemoryStream();
                        var writer = new BinaryWriter(stream);
                        
                        writer.Write((int)PacketType.ScoreUpdate);
                        writer.Write(client.ID);
                        writer.Write(client.Score++);
                        writer.Flush();
                        
                        Broadcast(stream.ToArray());
                    }
                }
            
                Broadcast(fruit.ToFruitMovePacket());
            }  
        }
        Thread.Sleep(TickRate / 1000);
    }

   
}
using SFML.Graphics;
using SFML.System;
using SuperGigachadGame.Server;

namespace SuperGigachadGame.GameObjects;

public abstract class BaseFruit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public virtual FruitType Type { get; set; }
    /// <summary>
    /// The score value of the fruit
    /// </summary>
    public virtual int Points { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    
    
    public byte[] ToBuffer()
    {
        var stream = new MemoryStream();
        
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write((int)Type);
            writer.Write(Id.ToByteArray());
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Points);
            writer.Flush();
        }
        
        return stream.ToArray();
    }
    
    public byte[] ToFruitSpawnPacket()
    {
        var stream = new MemoryStream();
        
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write((int)PacketType.FruitSpawn);
            writer.Write((int)Type);
            writer.Write(Id.ToByteArray());
            writer.Write(X);
            writer.Write(Y);
            writer.Flush();
            writer.Close();
        }
        
        stream.Close();

        return stream.ToArray();
    }
    
    public byte[] ToFruitMovePacket()
    {
        var stream = new MemoryStream();
        
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write((int)PacketType.FruitMove);
            writer.Write((int)Type);
            writer.Write(Id.ToByteArray());
            writer.Write(X);
            writer.Write(Y);
            writer.Close();
        }
        
        stream.Close();
        
        return stream.ToArray();
    }

    public void ReadFromBytes(byte[] buffer)
    {

        var rawStream = new MemoryStream(buffer);
        using (var reader = new BinaryReader(rawStream))
        {
            Type = (FruitType)reader.ReadInt32();
            Id = new Guid(reader.ReadBytes(16));
            X = reader.ReadInt32();
            Y = reader.ReadInt32();
            Points = reader.ReadInt32();
            reader.Close();
        }
        
        rawStream.Close();
    }
    
    public bool IsColiding(Vector2i position)
    {
        var distance = Math.Sqrt(Math.Pow(position.X - X, 2) + Math.Pow(position.Y - Y, 2));
        
        return distance < 10;
    }
    
}
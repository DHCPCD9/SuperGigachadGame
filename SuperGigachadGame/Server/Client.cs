using System.Net.Sockets;

namespace SuperGigachadGame.Server;

public class Client
{
    public string Tcp { get; set; }
    public string Username { get; set; }
    public int ID { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public String Color { get; set; }
    public int Score { get; set; } = 0;

    public void SetRandomColor()
    {
        var random = new Random();

        Color = String.Format("#{0:X6}", random.Next(0x1000000));
    }

    public byte[] ToBuffer()
    {
        var rawBuffer = new MemoryStream();
        
        var writer = new BinaryWriter(rawBuffer);
        
        writer.Write(ID);
        writer.Write(Username);
        writer.Write(X);
        writer.Write(Y);
        writer.Write(Color);
        writer.Write(Score);
        writer.Flush();

        return rawBuffer.ToArray();
    }
}
namespace SuperGigachadGame.Server;

public class ChatUser
{
    public static ChatUser SYSTEM = new ChatUser("Server", -1, true);

    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsSystem { get; set; }
    
    public ChatUser(string name, int id, bool isSystem)
    {
        Id = id;
        Name = name;
        IsSystem = isSystem;
    }
    
    public static ChatUser FromClient(Client client)
    {
        return new ChatUser(client.Username, client.ID, client.ID < 0);
    }
    
    public static ChatUser FromBuffer(byte[] buffer)
    {
        using (var stream = new MemoryStream(buffer))
        using (var reader = new BinaryReader(stream))
        {
            var id = reader.ReadInt32();
            var name = reader.ReadString();
            var isSystem = reader.ReadBoolean();
            return new ChatUser(name, id, isSystem);
        }
    }
    
    public static ChatUser FromStream(Stream stream)
    {
        using (var reader = new BinaryReader(stream))
        {
            var id = reader.ReadInt32();
            var name = reader.ReadString();
            var isSystem = reader.ReadBoolean();
            return new ChatUser(name, id, isSystem);
        }
    }
    
    public static ChatUser FromReader(BinaryReader reader)
    {
   
        var id = reader.ReadInt32();
        var name = reader.ReadString();
        var isSystem = reader.ReadBoolean();
        return new ChatUser(name, id, isSystem);
    }
    
    public byte[] ToBuffer()
    {
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(Id);
            writer.Write(Name);
            writer.Write(IsSystem);
            return stream.ToArray();
        }
    }
}
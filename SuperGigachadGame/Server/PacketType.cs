namespace SuperGigachadGame.Server;

public enum PacketType : int
{
    PlayerData = 0,
    Connected = 1,
    PlayerPosition = 2,
    PlayerJoined = 3,
    PlayerLeft = 4,
    Ping = 5,
    FruitSpawn = 6,
    FruitMove = 7,
    ScoreUpdate = 8,
    Chat = 9
}
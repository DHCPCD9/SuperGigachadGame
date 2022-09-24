using SFML.Graphics;
using SFML.System;

namespace SuperGigachadGame.Player;

public class RemotePlayer : BasePlayer
{
    
    public Vector2i Position { get; set; }
    public Vector2i Velocity { get; set; }
    public Game Game { get; set; }
    public string PlayerColor { get; set; }
    public String Username { get; set; }
    public int ID { get; set; }
    public int Score { get; set; } = 0;

    public RemotePlayer(Game game, int uid, string username, string color, int x, int y, int score)
    {
        Position = new Vector2i(x, y);
        Velocity = new Vector2i(0, 0);
        Game = game;
        PlayerColor = color;
        Username = username;
        ID = uid;
        Score = score;
    }
    
    public override void Update()
    {
        
    }

    public override void Draw(RenderTarget target, RenderStates states)
    {
        var shape = new RectangleShape(new Vector2f(32, 32));
        var color = System.Drawing.ColorTranslator.FromHtml(PlayerColor);
        shape.FillColor = new Color((byte) color.R, (byte) color.G, (byte) color.B);
        shape.Position = new Vector2f(Position.X, target.GetView().Size.Y - Position.Y - shape.Size.Y);
        target.Draw(shape);
    }

    public override void Move(int x, int y)
    {
        Position += new Vector2i(x, y);

        if (Position.X > Game.Window.Size.X - 32)
        {
            Position = new Vector2i(0, Position.Y);
        }
        
        if (Position.X < 0)
        {
            Position = new Vector2i((int)Game.Window.Size.X - 32, Position.Y);
        }
    }
}
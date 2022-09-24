using SFML.Graphics;
using SFML.System;
using SFMLDrawable = SFML.Graphics.Drawable;

namespace SuperGigachadGame.GameObjects.Drawable;

[DrawableFruit(type: FruitType.Apple, score: 10)]
public class DrawableApple : DrawableFruit
{
    public override void Draw(RenderTarget target, RenderStates states)
    {

        var shape = new RectangleShape(new Vector2f(32, 32));
        shape.FillColor = Color.Red;
        shape.Position = new Vector2f(Position.X, target.GetView().Size.Y - Position.Y - shape.Size.Y);

        
        target.Draw(shape);
    }

    public override FruitType Type { get; set; }
    public override int Points { get; set; }
}
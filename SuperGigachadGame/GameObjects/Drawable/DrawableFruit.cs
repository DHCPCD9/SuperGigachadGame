using System.Reflection;
using SFML.Graphics;
using SFML.System;

namespace SuperGigachadGame.GameObjects.Drawable;

public abstract class DrawableFruit : BaseFruit, SFML.Graphics.Drawable
{
    public abstract void Draw(RenderTarget target, RenderStates states);
    
    public Vector2i Position { get; set; }
    
    public DrawableFruit()
    {
        var attribute = GetType().GetCustomAttribute<DrawableFruitAttribute>();
        
        if (attribute == null)
            throw new Exception("Fruit must have DrawableFruitAttribute");
        
        Type = attribute.Type;
        Points = attribute.Score;
    }
}
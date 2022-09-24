using SFML.Graphics;

namespace SuperGigachadGame.Player;

public abstract class BasePlayer : Drawable
{
    public abstract void Update();
    
    public abstract void Draw(RenderTarget target, RenderStates states);
    
    public abstract void Move(int x, int y);
}
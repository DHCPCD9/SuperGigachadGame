namespace SuperGigachadGame.GameObjects.Drawable;

public class DrawableFruitAttribute : Attribute
{
    public FruitType Type;
    public int Score;
    
    public DrawableFruitAttribute(FruitType type, int score)
    {
        Type = type;
        Score = score;
    }
}
using SFML.Graphics;

namespace SuperGigachadGame.GameObjects;

public class Apple : BaseFruit
{
    public override FruitType Type { get; set; } = FruitType.Apple;
    public override int Points { get; set; } = 10;
}
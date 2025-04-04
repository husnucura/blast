using System.Collections;
using UnityEngine;

public class Rocket : SpecialItem
{

    public enum RocketDirection { Horizontal, Vertical }
    public RocketDirection Direction { get; private set; }


    public Rocket(GameObject gameObject, RocketDirection direction, int x, int y)
    : base(gameObject, direction == RocketDirection.Horizontal ? ItemType.HorizontalRocket : ItemType.VerticalRocket, x, y)
    {
        Direction = direction;
    }
    public override bool IsRocket() => true;



    public override void Blast()
    {
        
    }

    public override bool IsFallable()
    {
        return true;
    }

}

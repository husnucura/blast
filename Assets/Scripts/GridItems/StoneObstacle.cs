using System.Collections;
using UnityEngine;

public class StoneObstacle : Obstacle
{
    public StoneObstacle(GameObject gameObject, int x, int y) : base(gameObject, ItemType.Stone, x, y)
    {
    }
    public override bool DealDamage(int damage, DamageSource damageSource)
    {
        if (damageSource == DamageSource.Rocket)
            health -= damage;
        return health <= 0;
    }


}

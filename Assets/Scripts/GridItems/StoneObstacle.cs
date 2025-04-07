using System.Collections;
using UnityEngine;

public class StoneObstacle : Obstacle
{
    public StoneObstacle(GameObject gameObject, int x, int y) : base(gameObject, ItemType.Stone, x, y)
    {
    }
    public override bool DealDamage(int damage, DamageSource damageSource,GridState gridState)
    {
        if (damageSource == DamageSource.Rocket|| damageSource == DamageSource.RocketCombo)
            health -= damage;
        if(health <= 0)
            gridState.stoneTarget--;
        return health <= 0;
    }


}

using System.Collections;
using UnityEngine;

public abstract class Obstacle : GridItem
{
    protected Obstacle(GameObject gameObject,ItemType itemType,int x,int y):base(gameObject,itemType,x,y)
    {
    }
    protected int health =1;

    public override bool IsObstacle() => true;
    public virtual bool DealDamage(int  damage,DamageSource damageSource,GridState gridState)
    {
        health -=damage;
        return health <=0;
    }
    public virtual IEnumerator DealDamageAnimation(){
        yield return null;
    }

}

public enum DamageSource
{
    Blast,
    Rocket
}
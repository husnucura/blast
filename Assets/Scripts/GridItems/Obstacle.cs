using UnityEngine;

public abstract class Obstacle : GridItem
{
    protected Obstacle(GameObject gameObject,ItemType itemType,int x,int y):base(gameObject,itemType,x,y)
    {
    }
    protected int health =1;

    public override bool IsObstacle() => true;
    public virtual bool DealDamage(int  damage,DamageSource damageSource)
    {
        health -=damage;
        return health <=0;
    }

}

public enum DamageSource
{
    Blast,
    Rocket
}
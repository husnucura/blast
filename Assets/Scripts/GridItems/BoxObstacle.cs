using UnityEngine;

public class BoxObstacle : Obstacle
{

    public BoxObstacle(GameObject gameObject, int x, int y) : base(gameObject, ItemType.Box, x, y)
    {
    }


    public override bool DealDamage(int damage, DamageSource damageSource, GridState gridState)
    {
        bool result = base.DealDamage(damage,damageSource,gridState);
        if(result){
            gridState.boxTarget--;
        }
        return result;
    }
    public override void Blast()
    {
        // Implement blasting behavior, such as breaking the box
        Debug.Log("Box blasted!");
    }
}

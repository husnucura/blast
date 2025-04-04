using System.Collections;
using UnityEngine;

public class VaseObstacle : Obstacle
{
    public VaseObstacle(GameObject gameObject,int x, int y) : base(gameObject,ItemType.Vase,x,y) 
    {
        health = 2;
     // Initialize any specific properties or behaviors for the vase obstacle
    }

    public override bool IsFallable() 
    {
        return true;
    }
}

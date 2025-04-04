using UnityEngine;

public class SpecialItem : GridItem{
    public SpecialItem(GameObject gameObject,ItemType itemType,int x,int y):base(gameObject,itemType,x,y){

    }
    public override bool IsSpecialItem() => true;
    public virtual bool IsRocket() => false;
  
}


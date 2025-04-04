using UnityEngine;
using System;

public class GridInputHandler : MonoBehaviour
{
    [Header("Debug")]
    private GridPositionCalculator gridCalculator;

    private void Awake()
    {
        gridCalculator = GridPositionCalculator.Instance;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleGridInput();
        }
    }

    private void HandleGridInput()
    {
        if (gridCalculator == null){
            Debug.Log("Ensure gridcalculator instantiated before");
            return;
        } 
            

        Vector3 mouseWorldPos = GetMouseWorldPosition(Camera.main,Input.mousePosition);
        if (mouseWorldPos == Vector3.negativeInfinity) return;

        Vector2Int gridPos = gridCalculator.GetGridPosition(mouseWorldPos);
        
        GridEvents.TriggerGridCellClicked(gridPos);
    }

    public Vector3 GetMouseWorldPosition(Camera camera,Vector3 position)
    {
        return camera.ScreenToWorldPoint(position);
        
    }

}

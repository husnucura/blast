using UnityEngine;
public class GameManager : MonoBehaviour {
    private void Start() {
        GridManager.Instance.LoadLevel(LevelManager.LoadCurrentLevelNumber());
    }
}
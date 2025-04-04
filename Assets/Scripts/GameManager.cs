using UnityEngine;
public class GameManager : MonoBehaviour {
    private void Start() {
        GridManager.Instance.LoadLevel(1);
    }
}
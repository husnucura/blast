using UnityEngine;
using TMPro;

public class ScoreGroupVisualController : MonoBehaviour
{
    public static ScoreGroupVisualController Instance { get; private set; }
    public enum ScoreItem
    {
        Box,
        Stone,
        Vase
    }

    [System.Serializable]
    public class ScoreVisual
    {
        public ScoreItem item;
        public Transform root;
        public TextMeshProUGUI scoreText;
        public GameObject completedImage;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public ScoreVisual[] visuals;
    public TextMeshProUGUI RemainingMove;

    public void SetScore(ScoreItem item, int score)
    {
        foreach (var visual in visuals)
        {
            if (visual.item == item)
            {
                if (score <= 0)
                {
                    visual.scoreText.gameObject.SetActive(false);
                    visual.completedImage.SetActive(true);
                }
                else
                {
                    visual.scoreText.gameObject.SetActive(true);
                    visual.completedImage.SetActive(false);
                    visual.scoreText.text = score.ToString();
                }
                break;
            }
        }
    }
    public void SetRemainingmoves(int moves){
        RemainingMove.text = moves.ToString();
    }

    private void Reset()
    {
        var children = GetComponentsInChildren<Transform>();
        visuals = new ScoreVisual[System.Enum.GetValues(typeof(ScoreItem)).Length];
        foreach (ScoreItem item in System.Enum.GetValues(typeof(ScoreItem)))
        {
            foreach (var t in children)
            {
                if (t.name == item.ToString())  // Name matches the enum value
                {
                    var sv = new ScoreVisual();
                    sv.item = item;
                    sv.root = t;
                    sv.scoreText = t.GetComponentInChildren<TextMeshProUGUI>();
                    sv.completedImage = t.Find("Completed")?.gameObject;
                    visuals[(int)item] = sv;
                    break;
                }
            }
        }
    }
}

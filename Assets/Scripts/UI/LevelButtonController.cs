using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButtonController : MonoBehaviour
{
    public Button levelButton;
    public TextMeshProUGUI levelText;

    private int currentLevel;

    private void Start()
    {
        currentLevel = LevelManager.LoadCurrentLevelNumber();
        UpdateButtonText();
        levelButton.onClick.AddListener(OnLevelButtonClick);
    }

    private void UpdateButtonText()
    {
        if (currentLevel > 10)
        {
            levelText.text = "Finished";
        }
        else
        {
            levelText.text = $"Level {currentLevel}";
        }
    }

    private void OnLevelButtonClick()
    {
        if (currentLevel > 10)
        {
            Debug.Log("All levels completed.");
            return;
        }

         SceneManager.LoadScene(1);

    }
}

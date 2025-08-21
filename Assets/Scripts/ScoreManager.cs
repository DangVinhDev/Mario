using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public TextMeshProUGUI scoreText;
    private int score;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        UpdateLabel();
    }

    public void Add(int amount)
    {
        score += amount;
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (scoreText)
            scoreText.text = "Score: " + score.ToString();
    }
}

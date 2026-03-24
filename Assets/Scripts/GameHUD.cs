using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameHUD : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI effectText; 

    private void Start()
    {
        player.OnScoreChanged += UpdateScore;
        player.OnHealthChanged += UpdateHealth;
        player.OnBonusApplied += ShowEffect;

        healthBar.maxValue = player.MaxHealth;
        healthBar.value = player.MaxHealth;
        scoreText.text = "Î÷êè: 0";
        healthText.text = $"HP: {player.MaxHealth}/{player.MaxHealth}";
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreText.text = $"Ðåêîðä: {bestScore}";
        effectText.text = "";
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnScoreChanged -= UpdateScore;
            player.OnHealthChanged -= UpdateHealth;
            player.OnBonusApplied -= ShowEffect;
        }
    }

    private void UpdateScore(int newScore)
    {
        scoreText.text = $"Î÷êè: {newScore}";
    }

    private void UpdateHealth(int current, int max)
    {
        healthBar.value = current;
        healthText.text = $"HP: {current}/{max}";
    }

    private void ShowEffect(BonusType type, float duration)
    {
        string effectName = type switch
        {
            BonusType.SpeedBoost => "ÓÑÊÎÐÅÍÈÅ",
            BonusType.Invincibility => "ÍÅÓßÇÂÈÌÎÑÒÜ",
            BonusType.Heal => "ËÅ×ÅÍÈÅ",
            _ => ""
        };
        if (type == BonusType.Heal)
        {
            StartCoroutine(ShowTemporaryText(effectName, 1f));
        }
        else
        {
            StartCoroutine(ShowTemporaryText(effectName, duration));
        }
    }

    private System.Collections.IEnumerator ShowTemporaryText(string text, float
   duration)
    {
        effectText.text = text;
        yield return new WaitForSeconds(duration);
        effectText.text = "";
    }
}
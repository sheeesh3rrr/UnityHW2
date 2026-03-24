using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI developerText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    private void Start()
    {
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreText.text = $"Рекорд: {bestScore}";
        playButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }
    private void StartGame()
    {
        SceneManager.LoadScene(1);
    }
    private void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
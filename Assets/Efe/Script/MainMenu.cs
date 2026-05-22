using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ana menü: Start → oyun sahnesi, Quit → çıkış.
/// Buton On Click: StartGame() / QuitGame()
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Sahne")]
    [SerializeField] private string gameSceneName = "MainScene";
    [SerializeField] private int fallbackSceneBuildIndex = 1;

    [Header("Opsiyonel")]
    [SerializeField] private GameObject creditsPanel;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(gameSceneName) && Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        Debug.LogWarning($"[MainMenu] '{gameSceneName}' yüklenemedi. Build Settings'e ekle veya sahne adını kontrol et.", this);
        TryLoadFallback();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ToggleCredits()
    {
        if (creditsPanel == null) return;
        creditsPanel.SetActive(!creditsPanel.activeSelf);
    }

    private void TryLoadFallback()
    {
        if (fallbackSceneBuildIndex >= 0 && fallbackSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(fallbackSceneBuildIndex);
    }
}

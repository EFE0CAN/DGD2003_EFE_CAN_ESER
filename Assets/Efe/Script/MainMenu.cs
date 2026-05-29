using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Ana menü: 2 panel (Ana Menü / Ayarlar).
/// START → oyun sahnesi, SETTINGS → ayarlar paneli, BACK → ana menü, QUIT → çıkış.
/// Buton On Click: StartGame() / ShowSettings() / ShowMainMenu() / QuitGame()
/// Slider On Value Changed: OnVolumeChanged(float)
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Ses")]
    [SerializeField] private Slider volumeSlider;

    [Header("Sahne")]
    [SerializeField] private string gameSceneName = "MainScene";
    [SerializeField] private int fallbackSceneBuildIndex = 1;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        ShowMainMenu();

        if (volumeSlider != null)
        {
            float savedVolume = SaveSystem.GetVolume();
            volumeSlider.SetValueWithoutNotify(savedVolume);
            OnVolumeChanged(savedVolume);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    /// <summary>Panel 1: START, SETTINGS, QUIT</summary>
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>Panel 2: Volume + BACK</summary>
    public void ShowSettings()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
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

    public void OnVolumeChanged(float value) => SaveSystem.SetVolume(value);

    private void TryLoadFallback()
    {
        if (fallbackSceneBuildIndex >= 0 && fallbackSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(fallbackSceneBuildIndex);
    }
}

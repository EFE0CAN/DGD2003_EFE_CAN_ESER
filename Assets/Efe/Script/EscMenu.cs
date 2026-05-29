using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Oyun içi ESC menüsü — 2 panel (FirstMenu / SettingsPanel).
/// ESC: aç/kapat | Resume | Settings | BACK | Quit | Volume slider
/// </summary>
public class EscMenu : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Ses")]
    [SerializeField] private Slider volumeSlider;

    [Header("Sahne")]
    [SerializeField] private string mainMenuSceneName = "firstMenu";

    private void Start()
    {
        if (menuRoot != null)
            menuRoot.SetActive(false);

        IsOpen = false;
        ShowMainMenu();

        if (volumeSlider != null)
        {
            float savedVolume = SaveSystem.GetVolume();
            volumeSlider.SetValueWithoutNotify(savedVolume);
            OnVolumeChanged(savedVolume);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleMenu();
    }

    public void ToggleMenu()
    {
        if (!IsOpen)
        {
            OpenMenu();
            return;
        }

        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            ShowMainMenu();
            return;
        }

        Resume();
    }

    public void OpenMenu()
    {
        IsOpen = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (menuRoot != null)
            menuRoot.SetActive(true);

        ShowMainMenu();
    }

    public void Resume()
    {
        IsOpen = false;
        Time.timeScale = 1f;

        if (menuRoot != null)
            menuRoot.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        IsOpen = false;

        if (!string.IsNullOrEmpty(mainMenuSceneName) && Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogWarning($"[EscMenu] '{mainMenuSceneName}' yüklenemedi.", this);
    }

    public void OnVolumeChanged(float value) => SaveSystem.SetVolume(value);
}

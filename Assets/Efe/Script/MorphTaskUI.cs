using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sahnedeki MorphInteractable sayısını UI'da gösterir.
/// Kalan 0 olunca panel açılır ve belirli süre sonra başka sahneye geçilir.
/// </summary>
public class MorphTaskUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text countText;
    [SerializeField] private GameObject completionPanel;
    [Tooltip("Örn: Kalan: {0}")]
    [SerializeField] private string countFormat = "Kalan: {0}";

    [Header("Sahne geçişi")]
    [SerializeField] private string nextSceneName = "firstMenu";
    [SerializeField] private int nextSceneBuildIndex = 0;
    [SerializeField] private float delayBeforeLoad = 2f;

    private int _remaining;
    private bool _completionStarted;

    private void OnEnable()
    {
        MorphInteractable.MorphCompleted += OnMorphCompleted;
    }

    private void OnDisable()
    {
        MorphInteractable.MorphCompleted -= OnMorphCompleted;
    }

    private void Start()
    {
        if (completionPanel != null)
            completionPanel.SetActive(false);

        _remaining = CountRemainingMorphTasks();
        RefreshUI();
    }

    private static int CountRemainingMorphTasks()
    {
        MorphInteractable[] all = FindObjectsByType<MorphInteractable>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        int count = 0;
        foreach (MorphInteractable item in all)
        {
            if (item != null && !item.IsMorphed)
                count++;
        }

        return count;
    }

    private void OnMorphCompleted(MorphInteractable _)
    {
        if (_completionStarted) return;

        _remaining = CountRemainingMorphTasks();
        RefreshUI();

        if (_remaining > 0) return;

        _completionStarted = true;

        GameTimer timer = FindFirstObjectByType<GameTimer>();
        float score = timer != null ? timer.TimeLeft : 0f;
        int totalMorphs = FindObjectsByType<MorphInteractable>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None).Length;
        SaveSystem.RecordWin(score, totalMorphs);

        StartCoroutine(ShowPanelAndLoadScene());
    }

    private void RefreshUI()
    {
        if (countText == null) return;
        countText.text = string.Format(countFormat, Mathf.Max(0, _remaining));
    }

    private IEnumerator ShowPanelAndLoadScene()
    {
        if (completionPanel != null)
            completionPanel.SetActive(true);

        yield return new WaitForSeconds(delayBeforeLoad);

        if (!string.IsNullOrEmpty(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        if (nextSceneBuildIndex >= 0 && nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneBuildIndex);
    }
}

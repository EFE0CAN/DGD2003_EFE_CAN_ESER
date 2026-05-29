using System.IO;
using UnityEngine;

/// <summary>
/// PlayerPrefs: ayarlar (ses) | JSON: skor ve ilerleme (savegame.json).
/// </summary>
public static class SaveSystem
{
    private const string PrefVolume = "MasterVolume";
    private const string PrefHasJson = "HasJsonSave";
    private const string FileName = "savegame.json";

    public static GameSaveData Data { get; private set; } = new GameSaveData();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoLoad() => Load();

    public static void Load()
    {
        float volume = PlayerPrefs.GetFloat(PrefVolume, 1f);
        AudioListener.volume = Mathf.Clamp01(volume);

        Data = new GameSaveData();

        if (PlayerPrefs.GetInt(PrefHasJson, 0) != 1)
            return;

        string path = GetSavePath();
        if (!File.Exists(path))
            return;

        try
        {
            string json = File.ReadAllText(path);
            GameSaveData loaded = JsonUtility.FromJson<GameSaveData>(json);
            if (loaded != null)
                Data = loaded;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveSystem] JSON okunamadı: {e.Message}");
        }
    }

    public static void Save()
    {
        PlayerPrefs.SetFloat(PrefVolume, AudioListener.volume);
        PlayerPrefs.SetInt(PrefHasJson, 1);
        PlayerPrefs.Save();

        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(GetSavePath(), json);
    }

    public static float GetVolume() => PlayerPrefs.GetFloat(PrefVolume, 1f);

    public static void SetVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PrefVolume, AudioListener.volume);
        PlayerPrefs.Save();
    }

    public static void RecordWin(float score, int morphTasksDone)
    {
        int rounded = Mathf.RoundToInt(score);
        Data.lastRunScore = score;
        Data.morphTasksCompleted += morphTasksDone;

        if (rounded > Data.highScore)
            Data.highScore = rounded;

        Data.gamesWon++;
        Save();
    }

    public static void RecordLoss()
    {
        Data.gamesLost++;
        Save();
    }

    public static string GetSavePath() =>
        Path.Combine(Application.persistentDataPath, FileName);
}

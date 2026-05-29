using System;

/// <summary>
/// JSON dosyasına kaydedilen oyun verisi (skor, ilerleme).
/// </summary>
[Serializable]
public class GameSaveData
{
    public int highScore;
    public int gamesWon;
    public int gamesLost;
    public int morphTasksCompleted;
    public float lastRunScore;
}

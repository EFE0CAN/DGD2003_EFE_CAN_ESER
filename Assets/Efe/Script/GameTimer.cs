using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// TextMeshPro ile geri sayım. Canvas'taki TextMeshPro - Text (UI) bileşenini sürükle.
/// </summary>
public class GameTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    [Header("Süre")]
    [SerializeField] private float totalSeconds = 90f;
    [SerializeField] private bool startOnPlay = true;
    [SerializeField] private bool countDown = true;

    [Header("Uyarı (son saniyeler)")]
    [SerializeField] private float warningThreshold = 30f;
    [SerializeField] private float blinkSpeed = 4f;
    [SerializeField] private Color warningColorA = Color.red;
    [SerializeField] private Color warningColorB = Color.white;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Olaylar")]
    public UnityEvent onTimeUp;

    public static event Action OnTimeUp;

    private float _timeLeft;
    private bool _isRunning;

    public float TimeLeft => _timeLeft;
    public bool IsRunning => _isRunning;

    private void Awake()
    {
        if (timerText == null)
            timerText = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        ResetTimer();

        if (startOnPlay)
            _isRunning = true;
    }

    private void Update()
    {
        if (!_isRunning) return;

        if (countDown)
        {
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                _isRunning = false;
                FinishTimer();
            }
        }
        else
        {
            _timeLeft += Time.deltaTime;
            if (totalSeconds > 0f && _timeLeft >= totalSeconds)
            {
                _timeLeft = totalSeconds;
                _isRunning = false;
                FinishTimer();
            }
        }

        RefreshUI();
    }

    public void ResetTimer()
    {
        _timeLeft = countDown ? totalSeconds : 0f;
        RefreshUI();
    }

    public void StartTimer()
    {
        _isRunning = true;
    }

    public void PauseTimer()
    {
        _isRunning = false;
    }

    public void ResumeTimer()
    {
        _isRunning = true;
    }

    public void AddTime(float seconds)
    {
        _timeLeft += seconds;
        if (countDown && _timeLeft < 0f) _timeLeft = 0f;
        RefreshUI();
    }

    private void FinishTimer()
    {
        onTimeUp?.Invoke();
        OnTimeUp?.Invoke();
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(_timeLeft / 60f);
        int seconds = Mathf.FloorToInt(_timeLeft % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        if (countDown && _timeLeft <= warningThreshold && _timeLeft > 0f && _isRunning)
        {
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            timerText.color = Color.Lerp(warningColorA, warningColorB, t);
        }
        else
        {
            timerText.color = normalColor;
        }
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// TextMeshPro dijital geri sayım: son 10 sn'de kırmızı-beyaz yanıp sönme ve büyüme-küçülme.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class GameTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    [Header("Süre")]
    [SerializeField] private float totalSeconds = 90f;
    [SerializeField] private bool startOnPlay = true;
    [SerializeField] private bool countDown = true;

    [Header("Dijital saat görünümü")]
    [SerializeField] private float characterSpacing = 12f;

    [Header("Uyarı — son saniyeler")]
    [SerializeField] private float warningThreshold = 10f;
    [Tooltip("Bu sürenin altında kırmızı-beyaz yanıp sönme ve büyüme-küçülme")]
    [SerializeField] private float warningScaleMin = 0.85f;
    [SerializeField] private float warningScaleMax = 1.3f;
    [SerializeField] private float warningScaleSpeed = 5f;
    [Tooltip("Her saniye değişiminde ekstra vurgu (uyarı modunda)")]
    [SerializeField] private float punchPeakScale = 1.4f;
    [SerializeField] private float punchDuration = 0.2f;
    [SerializeField] private float blinkSpeed = 5f;
    [SerializeField] private bool blinkColon = true;
    [SerializeField] private float colonBlinkRate = 2f;
    [SerializeField] private Color warningColorA = Color.red;
    [SerializeField] private Color warningColorB = Color.white;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Olaylar")]
    public UnityEvent onTimeUp;

    public static event Action OnTimeUp;

    private float _timeLeft;
    private bool _isRunning;
    private float _punchTimer;
    private int _lastWholeSecond = -1;
    private Vector3 _baseTextScale = Vector3.one;

    public float TimeLeft => _timeLeft;
    public bool IsRunning => _isRunning;

    private bool InWarning => countDown && _isRunning && _timeLeft <= warningThreshold && _timeLeft > 0f;

    private void Awake()
    {
        if (timerText == null)
            timerText = GetComponentInChildren<TMP_Text>();

        CacheBaseScale();
    }

    private void Start()
    {
        ApplyDigitalStyle();
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

        TickSecondPulse();
        RefreshUI();
    }

    private void CacheBaseScale()
    {
        if (timerText == null) return;

        _baseTextScale = timerText.rectTransform.localScale;
        if (_baseTextScale.sqrMagnitude < 0.0001f)
            _baseTextScale = Vector3.one;
    }

    private void ApplyDigitalStyle()
    {
        if (timerText == null) return;

        timerText.fontStyle = FontStyles.Bold;
        timerText.characterSpacing = characterSpacing;
        timerText.paragraphSpacing = 0f;
        timerText.enableWordWrapping = false;
        timerText.alignment = TextAlignmentOptions.Center;
    }

    private void TickSecondPulse()
    {
        if (!InWarning) return;

        int wholeSecond = Mathf.FloorToInt(Mathf.Max(0f, _timeLeft));
        if (wholeSecond == _lastWholeSecond) return;

        _lastWholeSecond = wholeSecond;
        _punchTimer = punchDuration;
    }

    public void ResetTimer()
    {
        _timeLeft = countDown ? totalSeconds : 0f;
        _lastWholeSecond = -1;
        _punchTimer = 0f;
        RefreshUI();
    }

    public void StartTimer() => _isRunning = true;
    public void PauseTimer() => _isRunning = false;
    public void ResumeTimer() => _isRunning = true;

    public void AddTime(float seconds)
    {
        _timeLeft += seconds;
        if (countDown && _timeLeft < 0f) _timeLeft = 0f;
        _lastWholeSecond = -1;
        RefreshUI();
    }

    private void FinishTimer()
    {
        _punchTimer = 0f;
        onTimeUp?.Invoke();
        OnTimeUp?.Invoke();
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(_timeLeft / 60f);
        int seconds = Mathf.FloorToInt(_timeLeft % 60f);

        string separator = GetColonSeparator();
        timerText.text = $"{minutes:00}{separator}{seconds:00}";

        ApplyColor();
        ApplyScaleAnimation();
    }

    private string GetColonSeparator()
    {
        if (!blinkColon || !InWarning)
            return ":";

        return Mathf.FloorToInt(Time.time * colonBlinkRate) % 2 == 0 ? ":" : " ";
    }

    private void ApplyColor()
    {
        if (InWarning)
        {
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            timerText.color = Color.Lerp(warningColorA, warningColorB, t);
            return;
        }

        timerText.color = normalColor;
    }

    private void ApplyScaleAnimation()
    {
        if (timerText == null) return;

        float scale = 1f;

        if (InWarning)
        {
            float wave = (Mathf.Sin(Time.time * warningScaleSpeed) + 1f) * 0.5f;
            scale = Mathf.Lerp(warningScaleMin, warningScaleMax, wave);

            if (_punchTimer > 0f)
            {
                _punchTimer -= Time.deltaTime;
                float punchT = 1f - Mathf.Clamp01(_punchTimer / punchDuration);
                float punchBoost = Mathf.Lerp(punchPeakScale, 1f, punchT * punchT);
                scale *= punchBoost;
            }
        }

        timerText.rectTransform.localScale = _baseTextScale * scale;
    }

    private void OnDisable()
    {
        if (timerText != null)
            timerText.rectTransform.localScale = _baseTextScale;
    }
}

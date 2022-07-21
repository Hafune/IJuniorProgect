using System;
using TMPro;
using UnityEngine;

public class FinishLabelsInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timeLabel;
    [SerializeField] private TextMeshProUGUI _scoreLabel;

    private int _totalCoins;
    private int _totalCollectedCoins;
    private string _scorePreset;
    private string _timePreset;

    public void IncrementTotalCoinsCount() => _totalCoins++;
    public void IncrementTotalCollectedCoinsCount() => _totalCollectedCoins++;

    public void UpdateLabels()
    {
        int minutes = Math.Min(59, (int) (Time.timeSinceLevelLoad / 60));
        int seconds = Math.Min(59, (int) Time.timeSinceLevelLoad - minutes * 60);

        _timeLabel.text = string.Format(_timePreset, minutes, seconds);
        _scoreLabel.text = string.Format(_scorePreset, _totalCollectedCoins, _totalCoins);
    }

    private void Awake()
    {
        _timePreset = _timeLabel.text;
        _scorePreset = _scoreLabel.text;
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CoinStorage : MonoBehaviour
{
    [SerializeField] private UnityEvent _onCoinAdded;
    [SerializeField] private int _coins;
    [SerializeField] private TextMeshProUGUI _label;

    private string _textPreset;

    public int Coins => _coins;

    public void AddCoin()
    {
        _coins++;
        UpdateText();
        _onCoinAdded.Invoke();
    }

    private void UpdateText() => _label.text = string.Format(_textPreset, _coins);

    private void Start()
    {
        _textPreset = _label.text;
        UpdateText();
    }
}
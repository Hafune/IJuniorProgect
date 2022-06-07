using TMPro;
using UnityEngine;

public class CoinStorage : MonoBehaviour
{
    [SerializeField] private int _coins;
    [SerializeField] private TextMeshProUGUI _label;

    public void AddCoin()
    {
        _coins++;
        _label.text = _coins.ToString();
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinCollector : MonoBehaviour
{
    [SerializeField] private int _coins;
    [SerializeField] private TextMeshProUGUI _label;

    public void CollectCoin(GameObject coin)
    {
        Debug.Log(coin.transform.position);
        _coins++;
        _label.text = _coins.ToString();
    }
}
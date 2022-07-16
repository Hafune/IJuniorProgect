using UnityEngine;

public class AdditionalLayer : MonoBehaviour
{
    [SerializeField, Layer] private int _layer = 0;

    public int Layer => _layer;
}
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Spring2D : MonoBehaviour
{
    private static readonly int OnLaunch = Animator.StringToHash("OnLaunch");

    [SerializeField] [Min(0)] private float _magnitude;

    private Animator _animator;

    public float Magnitude => _magnitude;

    private void Start() => _animator = GetComponent<Animator>();

    private void OnTriggerEnter2D(Collider2D other) => _animator.SetTrigger(OnLaunch);
}
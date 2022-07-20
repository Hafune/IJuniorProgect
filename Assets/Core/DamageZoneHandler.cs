using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DamageZoneHandler : MonoBehaviour
{
    [SerializeField] private UnityEvent _onDamaged;
    [SerializeField] private UnityEvent _onDamagedEnd;
    [SerializeField] private AnimationEventDispatcher _animation;

    private bool isDamaged;
    private float damagedTime = 1f;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (isDamaged || !col.TryGetComponent(out DamageZone zone))
            return;

        StartCoroutine(PlayAnimationDamaged());
        _onDamaged.Invoke();
    }

    private IEnumerator PlayAnimationDamaged()
    {
        _animation.SetDamaged(isDamaged = true);

        yield return new WaitForSeconds(damagedTime);

        _animation.SetDamaged(isDamaged = false);
        _onDamagedEnd.Invoke();
    }
}
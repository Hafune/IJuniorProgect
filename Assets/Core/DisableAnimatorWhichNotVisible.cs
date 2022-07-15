using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.U2D.Animation;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Animator))]
public class DisableAnimatorWhichNotVisible : MonoBehaviour
{
    private Animator _animator;
    private Renderer _renderer;
    private bool lastActive = true;
    
    [CanBeNull]private SpriteSkin _spriteSkin;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<Renderer>();
        
         TryGetComponent(out _spriteSkin);
    }

    private void FixedUpdate()
    {
        if (lastActive == _renderer.isVisible)
            return;

        var isVisible = _renderer.isVisible;
        
        lastActive = isVisible;
        _animator.enabled = isVisible;

        if (_spriteSkin != null) 
            _spriteSkin.enabled = _renderer.isVisible;
    }
}
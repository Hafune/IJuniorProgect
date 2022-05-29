using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SignalingHandler : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource = null!;
    private bool _isActive = false;
    private float _volumePowerIncrement = .3f;

    public void Enable()
    {
        _isActive = true;
        _audioSource.Play();
    }

    public void Disable() => _isActive = false;

    private void Update()
    {
        if (!_audioSource.isPlaying)
            return;

        if (_isActive)
            _audioSource.volume += _volumePowerIncrement * Time.deltaTime;
        else
            _audioSource.volume -= _volumePowerIncrement * Time.deltaTime;

        if (_audioSource.volume != 0)
            return;

        _audioSource.Stop();
        _audioSource.volume = 0;
    }
}
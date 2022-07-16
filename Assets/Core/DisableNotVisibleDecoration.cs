using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lib;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Animator))]
public class DisableNotVisibleDecoration : MonoBehaviour
{
    private Renderer _renderer;
    private bool lastActive = true;

    [CanBeNull] private IEnumerable<Behaviour> _components;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();

        _components = GetComponents<Component>()
            .Where(component =>
                component != this &&
                component.GetType() != typeof(Renderer) &&
                component is Behaviour
            )
            .Select(component => component as Behaviour);
    }

    private void FixedUpdate()
    {
        if (lastActive == _renderer.isVisible)
            return;

        var isVisible = _renderer.isVisible;
        lastActive = isVisible;
        _components.ForEach(component => component.enabled = isVisible);
    }
}
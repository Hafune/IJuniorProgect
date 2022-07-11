using System;
using Lib;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private Vector2 parallaxEffectMultiplier;
    [SerializeField] private bool infiniteHorizontal;
    [SerializeField] private bool infiniteVertical;

    private Transform cameraTransform;
    private Vector3 startCameraPosition;
    private Vector3 startDifference;
    private Vector3 startPosition;
    private float textureUnitSizeX;
    private float textureUnitSizeY;

    private void Start()
    {
        cameraTransform = Camera.main!.transform;
        startCameraPosition = cameraTransform.position;
        var sprite = GetComponent<SpriteRenderer>().sprite;
        var size = sprite.bounds.size;

        var lossyScale = transform.lossyScale;

        textureUnitSizeX = size.x / sprite.pixelsPerUnit * transform.localScale.x * lossyScale.x;
        textureUnitSizeY = size.y / sprite.pixelsPerUnit * transform.localScale.y * lossyScale.y;
        startPosition = transform.position;
    }

    private void LateUpdate()
    {
        var deltaMovement = cameraTransform.position - startCameraPosition;
        var multiplierMovement = deltaMovement * parallaxEffectMultiplier;

        var softOffset = (Vector3) multiplierMovement;

        var hardOffsetX = 0f;
        var hardOffsetY = 0f;

        if (infiniteHorizontal)
        {
            hardOffsetX = (int) (deltaMovement.x / textureUnitSizeX) * textureUnitSizeX;
            softOffset.x %= textureUnitSizeX;
        }

        if (infiniteVertical)
        {
            hardOffsetY = (int) (deltaMovement.y / textureUnitSizeY) * textureUnitSizeY;
            softOffset.y %= textureUnitSizeY;
        }

        transform.position = new Vector3(
            startPosition.x + hardOffsetX + softOffset.x,
            startPosition.y + hardOffsetY + softOffset.y,
            transform.position.z
        );
    }
}
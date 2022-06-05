using UnityEngine;
using System.Collections;
using Lib;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    public float jumpHeight = 1f;
    public float timeToJumpApex = 3f;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;
    float moveSpeed = 6f;

    float gravity = -9f;
    float jumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller = null!;
    private PlayerInput _playerInput = null!;

    void Start()
    {
        controller = GetComponent<Controller2D>();
        _playerInput = new PlayerInput();
        _playerInput.Enable();
        gravity = Physics2D.gravity.y;
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void FixedUpdate()
    {
        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }

        if (_playerInput.Player.Jump.IsPressed() && controller.collisions.below) {
            velocity.y = jumpVelocity;
        }

        var input = _playerInput.Player.Move.ReadValue<Vector2>();

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    public float moveSpeed = 12f;
    public float clampLeftX = -30.5f;   // adjust to your wall positions
    public float clampRightX = 30.5f;

    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float dir = 0f;

        if (Keyboard.current.aKey.isPressed)
            dir = -1f;
        else if (Keyboard.current.dKey.isPressed)
            dir = 1f;

        Vector3 vel = new Vector3(dir * moveSpeed, 0, 0);
        _rb.MovePosition(transform.position + vel * Time.fixedDeltaTime);

        // clamp so the paddle never leaves screen
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, clampLeftX, clampRightX),
            transform.position.y,
            transform.position.z
        );
    }
}

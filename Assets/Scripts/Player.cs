using UnityEngine;
using UnityEngine.InputSystem; // <-- needed for Mouse.current

public class Player : MonoBehaviour
{
    Rigidbody _rigidbody;
    bool _mouseInitialized = false;

    
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Mouse not ready yet (frame 0)
        if (mousePos == Vector2.zero && !_mouseInitialized)
            return;

        _mouseInitialized = true;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 50));

        _rigidbody.MovePosition(new Vector3(worldPos.x, -17, 0));
    }
}

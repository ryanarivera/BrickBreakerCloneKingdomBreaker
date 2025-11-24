using UnityEngine;

public class Ball : MonoBehaviour
{
    float _speed = 20f;
    Rigidbody _rigidbody;
    Vector3 _velocity;
    Renderer _renderer;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        Invoke("Launch", 0.5f);
    }

    void Launch()
    {
        _rigidbody.linearVelocity = Vector3.down * _speed;
    }

    void FixedUpdate()
    {
        // Ensure constant speed
        _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _speed;
        _velocity = _rigidbody.linearVelocity;

        if (!_renderer.isVisible)
        {
            GameManager.Instance.Balls--;
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Bounce
        _rigidbody.linearVelocity = Vector3.Reflect(_velocity, collision.contacts[0].normal);
    }
}

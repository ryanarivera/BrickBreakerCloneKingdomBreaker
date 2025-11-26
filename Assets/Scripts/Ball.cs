using UnityEngine;

public class Ball : MonoBehaviour
{
    float _speed = 20f;
    Rigidbody _rigidbody;
    Vector3 _velocity;
    Renderer _renderer;
    public int health = 3;


    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
    }

    public void Launch(Vector3 direction)
    {
        _rigidbody.linearVelocity = direction.normalized * _speed;
    }

    void FixedUpdate()
    {
        _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _speed;
        _velocity = _rigidbody.linearVelocity;

        if (!_renderer.isVisible)
        {
            // Ball just dies, no ammo refund
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Damage only when hitting a brick
        if (collision.gameObject.CompareTag("Brick"))
        {
            health--;
            if (health <= 0)
            {
                // Ball "dies" from losing health
                Destroy(gameObject);
                return;
            }
        }

        // Still bounce off whatever it hit
        _rigidbody.linearVelocity = Vector3.Reflect(_velocity, collision.contacts[0].normal);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ReturnWall"))
        {
            // return to player immediately
            GameManager.Instance.ReturnBallToPlayer(this.gameObject);
        }
    }
}

using UnityEngine;

public class Ball : MonoBehaviour
{
    float _speed = 20f;
    Rigidbody _rigidbody;
    Vector3 _velocity;
    Renderer _renderer;

    public int health = 3;
    public bool isEnemyBall = false;
    public int damageToReturnWall = 1;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();

        if (isEnemyBall)
        {
            health = 1;  // force 1 HP on enemy balls
        }
    }

    public void Launch(Vector3 direction)
    {
        _rigidbody.linearVelocity = direction.normalized * _speed;
    }

    void FixedUpdate()
    {
        _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _speed;
        _velocity = _rigidbody.linearVelocity;

        // enemy balls dying off-screen don't refund ammo
        if (!_renderer.isVisible)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Ball otherBall = collision.gameObject.GetComponent<Ball>();

        // PLAYER BALL ↔ ENEMY BALL DAMAGE EACH OTHER
        if (otherBall != null)
        {
            if (isEnemyBall != otherBall.isEnemyBall) // different factions
            {
                health--;
                otherBall.health--;

                if (health <= 0) Destroy(gameObject);
                if (otherBall.health <= 0) Destroy(otherBall.gameObject);

                return;
            }
        }

        // PLAYER BALL TAKES DAMAGE FROM BRICK
        if (!isEnemyBall && collision.gameObject.CompareTag("Brick"))
        {
            health--;
            if (health <= 0)
            {
                Destroy(gameObject);
                return;
            }
        }

        // bounce on collision
        _rigidbody.linearVelocity = Vector3.Reflect(_velocity, collision.contacts[0].normal);
    }


    private void OnTriggerEnter(Collider other)
    {
        // PLAYER BALL RETURNS AMMO
        if (!isEnemyBall && other.CompareTag("ReturnWall"))
        {
            GameManager.Instance.ReturnBallToPlayer(this.gameObject);
            return;
        }

        // ENEMY BALL DAMAGES RETURN WALL
        if (isEnemyBall && other.CompareTag("ReturnWall"))
        {
            other.GetComponent<ReturnWall>().TakeDamage(damageToReturnWall);
            Destroy(gameObject);
        }

        // PLAYER CAN CATCH ENEMY BALLS
        if (isEnemyBall && other.CompareTag("Player"))
        {
            GameManager.Instance.Balls++; // ammo reward
            Destroy(gameObject);
            return;
        }
    }
}

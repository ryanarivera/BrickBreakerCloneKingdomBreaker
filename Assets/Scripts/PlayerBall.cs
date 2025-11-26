using UnityEngine;

public class PlayerBall : MonoBehaviour
{
    public float speed = 20f;
    public int health = 10;

    private Rigidbody rb;
    private Vector3 lastVel;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    void FixedUpdate()
    {
        lastVel = rb.linearVelocity.normalized * speed;
        rb.linearVelocity = lastVel;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Damage bricks
        if (collision.collider.CompareTag("Brick"))
        {
            health--;
            if (health <= 0)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Damage enemy balls
        EnemyBall enemy = collision.collider.GetComponent<EnemyBall>();
        if (enemy != null)
        {
            enemy.health--;
            health--;

            if (enemy.health <= 0)
                Destroy(enemy.gameObject);
            if (health <= 0)
                Destroy(gameObject);

            return;
        }

        // Bounce
        rb.linearVelocity = Vector3.Reflect(lastVel, collision.contacts[0].normal);
    }

    void OnTriggerEnter(Collider other)
    {
        // Return to player for ammo
        if (other.CompareTag("ReturnWall"))
        {
            GameManager.Instance.ReturnBallToPlayer(gameObject);
        }
    }
}

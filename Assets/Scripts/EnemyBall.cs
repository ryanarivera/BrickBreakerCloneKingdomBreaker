using UnityEngine;

public class EnemyBall : MonoBehaviour
{
    public float speed = 20f;
    public int health = 1;
    public int damageToReturnWall = 1;

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
        // Damage player balls
        PlayerBall pb = collision.collider.GetComponent<PlayerBall>();
        if (pb != null)
        {
            pb.health--;
            health--;

            if (pb.health <= 0) Destroy(pb.gameObject);
            if (health <= 0) Destroy(gameObject);

            return;
        }

        // Bounce
        rb.linearVelocity = Vector3.Reflect(lastVel, collision.contacts[0].normal);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ReturnWall"))
        {
            other.GetComponent<ReturnWall>().TakeDamage(damageToReturnWall);
            Destroy(gameObject);
        }

        if (other.CompareTag("Player"))
        {
            GameManager.Instance.Balls++;
            Destroy(gameObject);
        }
    }
}

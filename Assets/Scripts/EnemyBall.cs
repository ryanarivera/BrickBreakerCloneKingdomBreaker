using UnityEngine;

public class EnemyBall : MonoBehaviour
{
    public float speed = 20f;
    public int health = 1;
    public int damageToReturnWall = 1;

    private Rigidbody rb;
    private Vector3 lastVel;
    private Material mat;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mat = GetComponent<Renderer>().material;
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

            pb.UpdateColor();
            UpdateColor();

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

    // COLOR UPDATE
    public void UpdateColor()
    {
        float t = 1f - (float)health / 1f; // enemy maxHealth = 1 for now
        Color bright = new Color(1f, 0.3f, 0.3f);
        Color dark   = new Color(0.3f, 0f, 0f);

        mat.color = Color.Lerp(bright, dark, t);
    }
}

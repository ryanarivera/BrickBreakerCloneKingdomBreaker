using UnityEngine;
using System.Collections;

public class PlayerBall : MonoBehaviour
{
    public float speed = 20f;
    public int health = 10;
    public int maxHealth = 10;

    private Rigidbody rb;
    private Vector3 lastVel;
    private Material mat;   // <-- holds our instance material

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Make sure we have our own material instance
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
        // Bounce off player → heal or speed boost + scale pop
        if (collision.collider.CompareTag("Player"))
        {
            if (health < maxHealth)
            {
                health = maxHealth;
            }
            else
            {
                speed += 5f; // boost if already full HP
            }

            // Trigger scale pop effect
            StartCoroutine(ScalePop(2f, 0.25f));

            UpdateColor(); // <-- update color

            // Apply boosted bounce
            rb.linearVelocity = Vector3.Reflect(lastVel, collision.contacts[0].normal).normalized * speed;
            return;
        }

        // Damage bricks
        if (collision.collider.CompareTag("Brick"))
        {
            health--;
            UpdateColor(); // <-- update color

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

            enemy.UpdateColor(); // <-- enemy darkens
            UpdateColor();       // <-- we darken

            if (enemy.health <= 0)
                Destroy(enemy.gameObject);
            if (health <= 0)
                Destroy(gameObject);

            return;
        }

        // Normal bounce
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

    // COLOR UPDATE
    public void UpdateColor()
    {
        float t = 1f - (float)health / maxHealth; // 0 = full, 1 = empty
        Color bright = new Color(0.3f, 0.7f, 1f); // light blue
        Color dark   = new Color(0f, 0.1f, 0.3f); // dark navy blue

        mat.color = Color.Lerp(bright, dark, t);
    }

    // SCALE POP EFFECT
    IEnumerator ScalePop(float popAmount = 1.2f, float popTime = 0.12f)
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * popAmount;

        float halfTime = popTime / 2f;
        float t = 0f;

        // Scale up
        while (t < 1f)
        {
            t += Time.deltaTime / halfTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        t = 0f;

        // Scale back down
        while (t < 1f)
        {
            t += Time.deltaTime / halfTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}

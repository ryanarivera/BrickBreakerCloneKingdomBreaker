using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int value = 1;

    [Header("Magnet Settings")]
    public float magnetRange = 2.5f;     // how far balls can pull from
    public float pullSpeed = 12f;        // base speed of magnet pull
    public float acceleration = 25f;     // accelerates toward ball
    public float pickupDelay = 0.35f;    // delay before coin can be picked up

    private Transform targetBall;         // the ball that owns this coin
    private float currentSpeed = 0f;      // speed ramps up
    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        // Do NOT allow pickup right away
        if (Time.time < spawnTime + pickupDelay)
            return;

        // If no ball has claimed this coin yet → find one
        if (targetBall == null)
        {
            FindClosestBall();
        }
        else
        {
            // Move toward the chosen ball
            MagnetMove();
        }
    }

    void FindClosestBall()
    {
        // Use fast registry instead of scene search
        var balls = PlayerBallRegistry.Balls;

        if (balls.Count == 0)
            return; // Safety check

        float bestDist = Mathf.Infinity;
        Transform bestBall = null;

        foreach (var ball in balls)
        {
            if (ball == null) continue; // Safety check

            float dist = Vector3.Distance(transform.position, ball.transform.position);

            if (dist < magnetRange && dist < bestDist)
            {
                bestDist = dist;
                bestBall = ball.transform;
            }
        }

        if (bestBall != null)
        {
            targetBall = bestBall;
            currentSpeed = pullSpeed * 0.5f; // start slower → ramp up
        }
    }

    void MagnetMove()
    {
        if (targetBall == null)
            return;

        // Accelerate speed for strong vacuum effect
        currentSpeed += acceleration * Time.deltaTime;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetBall.position,
            currentSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only allow pickup if enough time has passed
        if (Time.time < spawnTime + pickupDelay)
            return;

        if (other.CompareTag("PlayerBall"))
        {
            GameManager.Instance.AddCoins(value);
            Destroy(gameObject);
        }
    }
}

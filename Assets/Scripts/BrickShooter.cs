using UnityEngine;

public class BrickShooter : MonoBehaviour
{
    public GameObject enemyBallPrefab;
    public float shootInterval = 2f;   // every 2 seconds
    public float ballDamage = 1f;

    public float spawnOffset = 0.6f;   // distance below brick to prevent self-collision

    private float nextShootTime = 0f;

    void Start()
    {
        // Ignore collisions between brick & enemy balls
        int brickLayer = LayerMask.NameToLayer("Brick");
        int ballLayer = LayerMask.NameToLayer("EnemyBall");
        Physics.IgnoreLayerCollision(brickLayer, ballLayer, true);
    }

    void Update()
    {
        if (Time.time >= nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + shootInterval;
        }
    }

    void Shoot()
    {
        // safe spawn point BELOW the brick
        Vector3 spawnPos = transform.position + Vector3.down * spawnOffset;

        // spawn enemy ball
        GameObject obj = Instantiate(enemyBallPrefab, spawnPos, Quaternion.identity);

        // grab EnemyBall script
        EnemyBall ball = obj.GetComponent<EnemyBall>();

        if (ball != null)
        {
            ball.health = 1; 
            ball.damageToReturnWall = (int)ballDamage;
            ball.Launch(Vector3.down);
        }
        else
        {
            Debug.LogError("EnemyBall component missing from enemyBallPrefab!");
        }
    }
}

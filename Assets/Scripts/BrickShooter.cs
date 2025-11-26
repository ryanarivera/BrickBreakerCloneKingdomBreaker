using UnityEngine;

public class BrickShooter : MonoBehaviour
{
    public GameObject ballPrefab;
    public float shootInterval = 2f;   // every 2 seconds
    public float ballDamage = 1f;

    private float nextShootTime = 0f;

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
        GameObject b = Instantiate(ballPrefab, transform.position, Quaternion.identity);
        Ball ball = b.GetComponent<Ball>();

        ball.isEnemyBall = true;
        ball.damageToReturnWall = (int)ballDamage;

        ball.Launch(Vector3.down);
    }
}

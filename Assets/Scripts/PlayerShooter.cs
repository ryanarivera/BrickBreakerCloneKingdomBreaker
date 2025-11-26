using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    public Transform shootPoint;      // empty object at center of paddle
    public GameObject playerBallPrefab;
    public LineRenderer aimLine;

    public float aimAngle = 90f;      // straight up
    public float rotateSpeed = 120f;  // how fast A/D rotates the aim
    
    void Update()
    {
        // --- NEW: Disable shooting + aiming unless we're in PLAY ---
        if (GameManager.Instance.CurrentState != GameManager.State.PLAY)
        {
            aimLine.enabled = false;
            return;
        }

        // If PLAY, enable aim line
        aimLine.enabled = true;
        
        HandleAiming();
        DrawAimLine();
        HandleShoot();
    }

    void HandleAiming()
    {
        if (Keyboard.current.qKey.isPressed)
            aimAngle += rotateSpeed * Time.deltaTime;

        if (Keyboard.current.eKey.isPressed)
            aimAngle -= rotateSpeed * Time.deltaTime;

        aimAngle = Mathf.Clamp(aimAngle, 30f, 150f);  // prevent sideways shots
    }

    void DrawAimLine()
    {
        Vector3 dir = AngleToDirection(aimAngle);

        aimLine.positionCount = 2;
        aimLine.SetPosition(0, shootPoint.position);
        aimLine.SetPosition(1, shootPoint.position + dir * 4f);
    }

    void HandleShoot()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ShootBall();
        }
    }

    void ShootBall()
    {
        if (GameManager.Instance.Balls <= 0)
            return;

        Vector3 dir = AngleToDirection(aimAngle);

        // spawn player ball
        GameObject ball = Instantiate(playerBallPrefab, shootPoint.position, Quaternion.identity);

        // Launch using PlayerBall script
        ball.GetComponent<PlayerBall>().Launch(dir);

        // Spend one ammo
        GameManager.Instance.Balls--;
    }

    Vector3 AngleToDirection(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    public Transform shootPoint;
    public GameObject playerBallPrefab;
    public GameObject trajectoryDotPrefab;

    [Header("Aiming Settings")]
    public float minAngle = 30f;
    public float maxAngle = 150f;

    [Header("Trajectory Settings")]
    public int maxDots = 100;
    public float dotSpacing = 0.5f;
    public int maxBounces = 1;

    [Header("Dot Appearance")]
    public float startScale = 0.35f;
    public float endScale = 0.05f;

    [Range(0f, 1f)]
    public float startAlpha = 1f;

    [Range(0f, 1f)]
    public float endAlpha = 0.15f;


    [Header("Raycast Mask")]
    public LayerMask trajectoryMask;

    private Vector3 currentAimDirection;
    private GameObject[] dots;

    void Start()
    {
        // Create dot pool
        dots = new GameObject[maxDots];
        for (int i = 0; i < maxDots; i++)
        {
            dots[i] = Instantiate(trajectoryDotPrefab);
            dots[i].SetActive(false);
        }
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.State.PLAY)
        {
            HideDots();
            return;
        }

        HandleAiming();
        DrawTrajectoryDots();
        HandleShoot();
    }

    // ------------------------------ AIMING ------------------------------------

    void HandleAiming()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(mousePos.x, mousePos.y, 10f)
        );

        worldPos.z = shootPoint.position.z;

        bool mouseLeft = worldPos.x < shootPoint.position.x;
        bool mouseBelow = worldPos.y < shootPoint.position.y;

        Vector3 rawDir = (worldPos - shootPoint.position).normalized;
        float angle = Mathf.Atan2(rawDir.y, rawDir.x) * Mathf.Rad2Deg;

        if (mouseBelow)
        {
            angle = mouseLeft ? 150f : 30f;
        }
        else
        {
            if (mouseLeft)
                angle = Mathf.Clamp(angle, 90f, 150f);
            else
                angle = Mathf.Clamp(angle, 30f, 90f);
        }

        float rad = angle * Mathf.Deg2Rad;
        currentAimDirection = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
    }

    // --------------------------- TRAJECTORY DOTS --------------------------------

    void DrawTrajectoryDots()
    {
        HideDots();

        Vector3 origin = shootPoint.position;
        Vector3 direction = currentAimDirection.normalized;

        int dotIndex = 0;
        int bounces = 0;

        while (dotIndex < maxDots)
        {
            RaycastHit hit;

            // Cast a LONG ray forward
            if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity, trajectoryMask))
            {
                float distance = Vector3.Distance(origin, hit.point);
                int steps = Mathf.FloorToInt(distance / dotSpacing);

                // --- Place dots up to the hit point ---
                for (int i = 0; i < steps && dotIndex < maxDots; i++)
                {
                    Vector3 point = origin + direction * (i * dotSpacing);
                    point.z = shootPoint.position.z;

                    // ---------- DOT POSITION ----------
                    GameObject dot = dots[dotIndex];
                    dot.transform.position = point;
                    dot.SetActive(true);

                    // ---------- DOT FADE-out & SCALE ----------
                    float t = (float)dotIndex / (maxDots - 1);   // normalized [0..1]

                    // scale
                    float scale = Mathf.Lerp(startScale, endScale, t);
                    dot.transform.localScale = new Vector3(scale, scale, 1f);

                    // fade
                    SpriteRenderer sr = dot.GetComponent<SpriteRenderer>();
                    Color c = sr.color;
                    c.a = Mathf.Lerp(startAlpha, endAlpha, t);
                    sr.color = c;

                    dotIndex++;
                }

                // --- Compute 2D reflection ---
                Vector3 flatNormal = new Vector3(hit.normal.x, hit.normal.y, 0f).normalized;
                Vector3 flatDirection = new Vector3(direction.x, direction.y, 0f).normalized;

                direction = Vector3.Reflect(flatDirection, flatNormal).normalized;
                direction.z = 0f;

                // --- Auto offset AWAY from surface ---
                float offset = dotSpacing * 0.15f;
                origin = hit.point + flatNormal * offset;
                origin.z = shootPoint.position.z;

                // Continue spacing flow
                dotIndex++;

                // Bounce limit
                bounces++;
                if (bounces >= maxBounces)
                    return;
            }
            else
            {
                // ---- No more hits: place final straight dots ----
                while (dotIndex < maxDots)
                {
                    Vector3 point = origin + direction * (dotSpacing * dotIndex);
                    point.z = shootPoint.position.z;

                    GameObject dot = dots[dotIndex];
                    dot.transform.position = point;
                    dot.SetActive(true);

                    // fade + scale
                    float t = (float)dotIndex / (maxDots - 1);

                    float scale = Mathf.Lerp(startScale, endScale, t);
                    dot.transform.localScale = new Vector3(scale, scale, 1f);

                    SpriteRenderer sr = dot.GetComponent<SpriteRenderer>();
                    Color c = sr.color;
                    c.a = Mathf.Lerp(startAlpha, endAlpha, t);
                    sr.color = c;

                    dotIndex++;
                }

                return;
            }
        }
    }

    void HideDots()
    {
        foreach (var d in dots)
            d.SetActive(false);
    }

    // ------------------------------ SHOOTING ------------------------------------

    void HandleShoot()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            ShootBall();
    }

    void ShootBall()
    {
        if (GameManager.Instance.Balls <= 0)
            return;

        var ball = Instantiate(playerBallPrefab, shootPoint.position, Quaternion.identity);
        ball.GetComponent<PlayerBall>().Launch(currentAimDirection);

        GameManager.Instance.Balls--;
    }
}

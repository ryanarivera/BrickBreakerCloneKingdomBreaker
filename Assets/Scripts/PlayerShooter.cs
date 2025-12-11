using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public enum FireMode
{
    Single,
    Burst,
    HorizontalLine,
    Circle,
    Triangle
}

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

    [Header("Fire Mode Settings")]
    public FireMode currentFireMode = FireMode.Single;

    public int burstCount = 5;        // number of balls in burst
    public float burstDelay = 0.05f;  // time between shots in burst

    public int lineCount = 5;         // number of balls in horizontal line
    public float lineWidth = 4f;      // horizontal spread width

    public int circleCount = 12;      // number of balls in circle
    public float circleRadius = 2f;   // circle radius

    public int triangleCount = 6;     // balls per triangle layer
    public float triangleSpacing = 0.5f;

    private bool canShoot = true;

    void Start()
    {
        // Create dot pool
        dots = new GameObject[maxDots];
        for (int i = 0; i < maxDots; i++)
        {
            dots[i] = Instantiate(trajectoryDotPrefab);
            dots[i].SetActive(false);
        }

        // ---- REGISTER WITH UI BUTTONS ----
        FireModeButton[] buttons = FindObjectsByType<FireModeButton>(FindObjectsSortMode.None);
        foreach (var b in buttons)
        {
            b.SetShooter(this);
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
        if (Keyboard.current.spaceKey.wasPressedThisFrame && canShoot)
            FireBasedOnMode();
    }

    void ShootBall()
    {
        if (GameManager.Instance.Balls <= 0)
            return;

        var ball = Instantiate(playerBallPrefab, shootPoint.position, Quaternion.identity);
        ball.GetComponent<PlayerBall>().Launch(currentAimDirection);

        GameManager.Instance.Balls--;
    }

    // ------------------------------ SHOOTING FORMATIONS ------------------------------------

    public void SetFireMode(int modeIndex)
    {
        currentFireMode = (FireMode)modeIndex;
    }

    void FireBasedOnMode()
    {
        switch (currentFireMode)
        {
            case FireMode.Single:
                FireSingle();
                break;

            case FireMode.Burst:
                StartCoroutine(FireBurst());
                break;

            case FireMode.HorizontalLine:
                FireHorizontalLine();
                break;

            case FireMode.Circle:
                FireCircle();
                break;

            case FireMode.Triangle:
                FireTriangle();
                break;
        }
    }

    void FireSingle()
    {
        if (GameManager.Instance.Balls <= 0) return;

        SpawnBall(shootPoint.position, currentAimDirection);
        GameManager.Instance.Balls--;
    }

    IEnumerator FireBurst()
    {
        canShoot = false;

        int shots = Mathf.Min(burstCount, GameManager.Instance.Balls);

        for (int i = 0; i < shots; i++)
        {
            SpawnBall(shootPoint.position, currentAimDirection);
            GameManager.Instance.Balls--;

            yield return new WaitForSeconds(burstDelay);
        }

        canShoot = true;
    }

    void FireHorizontalLine()
    {
        int count = Mathf.Min(lineCount, GameManager.Instance.Balls);
        float half = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            float offset = (i - half) * (lineWidth / (lineCount - 1));
            Vector3 spawnPos = shootPoint.position + new Vector3(offset, 0, 0);

            SpawnBall(spawnPos, currentAimDirection);
        }

        GameManager.Instance.Balls -= count;
    }

    void FireCircle()
    {
        int count = Mathf.Min(circleCount, GameManager.Instance.Balls);
        if (count <= 0) return;

        // Ball collider radius
        float ballRadius = playerBallPrefab.GetComponent<SphereCollider>().radius * playerBallPrefab.transform.localScale.x;

        float spacing = ballRadius * 2.1f; // soft minimum
        float relaxSpacing = ballRadius * 2.0f; // target minimum during relaxation

        // Lift above the player
        float verticalOffset = circleRadius + 0.3f;
        Vector3 center = shootPoint.position + new Vector3(0, verticalOffset, 0);

        // ------------- BUILD RINGS (CONTINUOUS OPTION B) ----------------
        List<Vector3> points = new List<Vector3>();
        int remaining = count;
        int layer = 0;

        while (remaining > 0)
        {
            float ringRadius = layer * spacing * 0.9f; 
            if (layer == 0) ringRadius = 0;

            int capacity;

            if (layer == 0)
                capacity = 1; // center
            else
            {
                float circumference = 2f * Mathf.PI * ringRadius;
                capacity = Mathf.Max(6, Mathf.FloorToInt(circumference / spacing));
            }

            int take = Mathf.Min(remaining, capacity);

            if (layer == 0)
            {
                points.Add(center);
            }
            else
            {
                float step = 2f * Mathf.PI / take;
                for (int i = 0; i < take; i++)
                {
                    float ang = i * step;
                    float x = Mathf.Cos(ang) * ringRadius;
                    float y = Mathf.Sin(ang) * ringRadius;
                    points.Add(center + new Vector3(x, y, 0));
                }
            }

            remaining -= take;
            layer++;
        }

        // ------------- BUBBLE RELAXATION ----------------
        // pushes points apart and keeps shape circular
        int iterations = 12;
        float pullStrength = 0.1f;

        for (int k = 0; k < iterations; k++)
        {
            // 1) push apart if too close
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    Vector3 a = points[i];
                    Vector3 b = points[j];

                    Vector3 diff = b - a;
                    float dist = diff.magnitude;

                    if (dist < relaxSpacing)
                    {
                        float push = (relaxSpacing - dist) * 0.5f;
                        Vector3 dir = diff.normalized;

                        points[i] -= dir * push;
                        points[j] += dir * push;
                    }
                }
            }

            // 2) pull points toward circle center to maintain circular shape
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 dir = (points[i] - center);
                float dist = dir.magnitude;

                float idealDist = dist; 
                if (dist > spacing * 0.5f)
                    idealDist = dist - pullStrength;
                else
                    idealDist = dist + pullStrength;

                if (dist > 0.001f)
                    points[i] = center + dir.normalized * idealDist;
            }
        }

        // ------------- SPAWN BALLS --------------------
        int used = 0;

        foreach (var p in points)
        {
            SpawnBall(p, currentAimDirection);
            used++;
        }

        GameManager.Instance.Balls -= used;
    }

    void FireTriangle()
    {
        int available = GameManager.Instance.Balls;
        int perRow = triangleCount;
        int used = 0;

        for (int row = 0; row < perRow; row++)
        {
            int ballsInRow = row + 1;

            for (int i = 0; i < ballsInRow; i++)
            {
                if (used >= available) break;

                float offsetX = (i - ballsInRow / 2f) * triangleSpacing;
                float offsetY = row * triangleSpacing;

                Vector3 spawn = shootPoint.position + new Vector3(offsetX, offsetY, 0);
                SpawnBall(spawn, currentAimDirection);

                used++;
            }

            if (used >= available) break;
        }

        GameManager.Instance.Balls -= used;
    }

    void SpawnBall(Vector3 pos, Vector3 direction)
    {
        var ball = Instantiate(playerBallPrefab, pos, Quaternion.identity);
        ball.GetComponent<PlayerBall>().Launch(direction.normalized);
    }
}

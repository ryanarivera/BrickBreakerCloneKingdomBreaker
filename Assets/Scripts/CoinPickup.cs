using UnityEngine;
using UnityEngine.InputSystem;

public class CoinPickup : MonoBehaviour
{
    public int value = 1;
    public float pickupRadius = 1f;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Convert mouse to world position on the coin’s plane
        float zDist = Mathf.Abs(cam.transform.position.z - transform.position.z);

        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, zDist));

        // 2D distance on X/Y plane only
        if (Vector2.Distance(mouseWorld, transform.position) <= pickupRadius)
        {
            GameManager.Instance.AddCoins(value);
            Destroy(gameObject);
        }
    }
}

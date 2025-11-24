using UnityEngine;
public class Brick : MonoBehaviour
{
    public int hits = 1;
    public int points = 100;
    public Vector3 rotator;
    public Material hitMaterial;

    Material _orgMaterial;
    Renderer _renderer;

    void Start()
    {
        transform.Rotate(rotator * (transform.position.x + transform.position.y) * 0.1f);
        _renderer = GetComponent<Renderer>();
        _orgMaterial = _renderer.sharedMaterial;
    }
    void Update()
    {
        transform.Rotate(rotator * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        hits--;
        if (hits <= 0)
        {
            GameManager.Instance.Score += points;
            Destroy(gameObject); 
        }
        _renderer.sharedMaterial = hitMaterial;
        Invoke("RestoreMaterial", 0.05f);
    }

    void RestoreMaterial()
    {
        _renderer.sharedMaterial = _orgMaterial;
    }
}
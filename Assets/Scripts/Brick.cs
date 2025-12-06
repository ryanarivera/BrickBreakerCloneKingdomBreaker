using UnityEngine;
public class Brick : MonoBehaviour
{
    public int hits = 1;
    public int points = 100;
    public int minCoins = 1;
    public int maxCoins = 3;

    public GameObject coinPrefab;
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

            int amount = Random.Range(minCoins, maxCoins + 1);
            for (int i = 0; i < amount; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-0.3f, 0.3f),
                    Random.Range(-0.3f, 0.3f),
                    0f
                );

                Instantiate(coinPrefab, transform.position + offset, Quaternion.identity);
            }
            
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
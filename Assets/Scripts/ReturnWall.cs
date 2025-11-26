using UnityEngine;

public class ReturnWall : MonoBehaviour
{
    public int maxHealth = 10;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;

        if (currentHealth <= 0)
        {
            GameManager.Instance.SwitchState(GameManager.State.GAMEOVER);
        }
    }
}

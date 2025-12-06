using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BallGenerator : MonoBehaviour
{
    [Header("Settings")]
    public float baseGenerateTime = 8f;        // initial cooldown
    public float timeReductionPerUpgrade = 1f; // gets faster each upgrade
    public float minGenerateTime = 2f;         // cannot go faster than this

    [Header("Upgrade Settings")]
    public int baseUpgradeCost = 10;
    public int upgradeCostIncrease = 10;

    [Header("UI")]
    public Image fillBar;          // UI bar showing progress
    public Button upgradeButton;   // button on return wall
    public TextMeshProUGUI costText;

    private float _currentCooldown;
    private float _timer;
    private int _currentUpgrade = 0;

    void Start()
    {
        _currentCooldown = baseGenerateTime;
        upgradeButton.onClick.AddListener(UpgradeGenerator);
        UpdateCostText();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.State.PLAY)
            return;

        // Fill timer
        _timer += Time.deltaTime;
        fillBar.fillAmount = _timer / _currentCooldown;

        // Generate ball
        if (_timer >= _currentCooldown)
        {
            _timer = 0f;
            GameManager.Instance.Balls++;
        }
    }

    public void UpgradeGenerator()
    {
        int cost = baseUpgradeCost + (_currentUpgrade * upgradeCostIncrease);

        if (GameManager.Instance.Coins < cost)
            return;

        GameManager.Instance.Coins -= cost;

        _currentUpgrade++;
        _currentCooldown -= timeReductionPerUpgrade;

        if (_currentCooldown < minGenerateTime)
            _currentCooldown = minGenerateTime;

        UpdateCostText();
    }

    void UpdateCostText()
    {
        int cost = baseUpgradeCost + (_currentUpgrade * upgradeCostIncrease);
        costText.text = "UPGRADE (" + cost + ")";
    }
}

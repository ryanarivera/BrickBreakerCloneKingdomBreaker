using UnityEngine;
using UnityEngine.UI;

public class FireModeButton : MonoBehaviour
{
    public PlayerShooter shooter;       // assigned at runtime
    public int modeIndex;

    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);   // <-- BUTTON HANDLER HERE
    }

    void Update()
    {
        if (shooter == null)
            return;

        button.image.color =
            shooter.currentFireMode == (FireMode)modeIndex
            ? selectedColor
            : defaultColor;
    }

    public void SetShooter(PlayerShooter shooterRef)
    {
        shooter = shooterRef;
    }

    // -----------------------------------------------------
    // THIS IS CALLED WHEN THE UI BUTTON IS CLICKED
    // -----------------------------------------------------
    void OnClicked()
    {
        if (shooter == null)
            return;

        shooter.SetFireMode(modeIndex);
    }
}

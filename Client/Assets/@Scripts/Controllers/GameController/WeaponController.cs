using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private SpriteRenderer fillRenderer;
    private GameScene gameScene;

    public void Init(GameScene scene)
    {
        fillRenderer = GetComponent<SpriteRenderer>();
        gameScene = scene;

        fillRenderer.material.SetFloat("_FillAmount", 0);
    }

    private void OnEnable()
    {
        Managers.Game.OnWeaponHpChanged -= UpdateFill;
        Managers.Game.OnWeaponHpChanged += UpdateFill;
    }

    private void OnDisable()
    {
        Managers.Game.OnWeaponHpChanged -= UpdateFill;
    }

    public void UpdateFill()
    {
        float amount = ((float)Managers.Game.WeaponHp / (float)Managers.Game.WeaponMaxHp);
        fillRenderer.material.SetFloat("_FillAmount", amount);
    }
}

using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private SpriteRenderer fillRenderer;
    private GameScene gameScene;

    public void Init(GameScene scene)
    {
        fillRenderer = GetComponent<SpriteRenderer>();
        gameScene = scene;

        UpdateFill(0f);
    }

    public void UpdateFill(float amount)
    {
        fillRenderer.material.SetFloat("_FillAmount", amount);
    }
}

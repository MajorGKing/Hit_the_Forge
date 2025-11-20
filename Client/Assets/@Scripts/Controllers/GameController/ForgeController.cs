using UnityEngine;

public class ForgeController : BaseController
{

    private SpriteRenderer fillRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fillRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HitForge()
    {
        var value = Managers.Game.CalcWeaponHit();
        Debug.Log(value);

        UpdateFill(value);
    }

    

    void UpdateFill(float amount)
    {
        fillRenderer.material.SetFloat("_FillAmount", amount);
    }
}
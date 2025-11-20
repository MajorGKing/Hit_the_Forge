using UnityEngine;

public class GameScene : BaseScene
{
    public WeaponController weapon;
    public ForgeController forge;
    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        gameObject.AddComponent<CaptureScreenShot>();
#endif

        Debug.Log("@>> GameScene Init()");
        SceneType = Define.EScene.GameScene;
    }

    protected override void Start()
    {
        weapon.Init(this);
        forge.Init(this);

        Managers.Game.StartWeaponMake("Dagger");
    }

    public override void Clear()
    {
    }


    public void HitForge()
    {
        var value = Managers.Game.CalcWeaponHit();

        weapon.UpdateFill(value);
    }


}
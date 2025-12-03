using UnityEngine;

public class GameScene : BaseScene
{
    public WeaponController weapon;
    public ForgeController forge;
    public UI_BattleBarWorldSpace hpBar;
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

        Managers.Game.GameInit();

        Managers.Game.StartWeaponMake(1);

        Managers.Sound.Play(Define.ESound.Bgm, "BGM1", 0.2f);
    }

    public override void Clear()
    {
    }

    public void HitForge()
    {
        Managers.Game.CalcWeaponHit();
    }


}
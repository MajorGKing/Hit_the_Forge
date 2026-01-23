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

        LogMessage.Log("@>> GameScene Init()");
        SceneType = Define.EScene.GameScene;
    }

    protected override void Start()
    {
        weapon.Init(this);
        forge.Init(this);

        Managers.Game.GameInit();

        //Managers.Game.StartWeaponMake(1);

        Managers.Sound.Play(Define.ESound.Bgm, "BGM3", 1f, 0.5f);

        // 튜토리얼 체크 스테이지가 1이고 무기 숫자가 0인 경우
        if(Managers.Player.Stage == 1 && Managers.Player.ClearedWeaponCount == 0)
        {
            Managers.Game.BeginTutorial();
        }
    }

    public override void Clear()
    {
    }

    public void HitForge()
    {
        Managers.Game.CalcWeaponHit();   
    }


}
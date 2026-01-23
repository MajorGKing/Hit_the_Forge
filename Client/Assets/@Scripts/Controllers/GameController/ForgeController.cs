using UnityEngine;

public class ForgeController : BaseController
{
    private GameScene gameScene;
    public void Init(GameScene scene)
    {
        gameScene = scene;
    }

    public void HitForge()
    {
        // 튜토리얼 이면 체크 하기
        if(Managers.Game.isTutorial == false)
        {
            gameScene.HitForge();
        }
        else
        {
            if(Managers.Touch.IsObjectAllowed(this.gameObject) == true)
                gameScene.HitForge();
        }
    }
}
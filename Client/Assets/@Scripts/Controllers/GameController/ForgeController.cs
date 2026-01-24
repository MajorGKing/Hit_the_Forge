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
            //if(Managers.Touch.IsObjectAllowed(gameObject) == true)
            if(Managers.Game.tutorialStep == 2 || Managers.Game.tutorialStep == 4)
                gameScene.HitForge();
        }
    }
}
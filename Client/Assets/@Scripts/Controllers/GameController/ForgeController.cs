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
        gameScene.HitForge();
    }
}
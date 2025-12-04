

using UnityEngine;

public class UI_Scene : UI_Base
{
    public Canvas UICanvas;
    protected override void Awake()
    {
        base.Awake();

        //Managers.UI.SetCanvas(gameObject, false);

        UICanvas = Managers.UI.SetCanvas(gameObject, false);

        UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
        UICanvas.worldCamera = Camera.main;
    }
}

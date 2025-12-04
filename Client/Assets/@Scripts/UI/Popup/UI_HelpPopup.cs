using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_HelpPopup : UI_Popup
{
    enum GameObjects
    {
        BackGround,
    }

    int helpStep;

    protected override void Awake()
    {
        base.Awake();

        BindObjects((typeof(GameObjects)));

        GetGameObject((int)GameObjects.BackGround).BindEvent(OnClickedBack);

        helpStep = 1;
    }

    private void OnEnable() 
    {
        helpStep = 1;

        GetGameObject((int)GameObjects.BackGround).GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("help01");
    }

    private void OnClickedBack(PointerEventData eventData)
    {
        helpStep++;

        if(helpStep == 7)
        {
            Managers.UI.ClosePopupUI(this);
            return;
        }

        var helpName = "help0" + helpStep.ToString();
        Debug.Log(helpName);

        GetGameObject((int)GameObjects.BackGround).GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(helpName);
    }
}

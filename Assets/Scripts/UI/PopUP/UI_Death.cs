using UnityEngine;
using UnityEngine.UI;

public class UI_Death : UI_Popup
{
    enum GamObjects
    {
        LobyBtn
    }
    private void Start()
    {
        Init();
    }
    public override void Init()
    {
        Bind<GameObject>(typeof(GamObjects));
        GetObject((int)GamObjects.LobyBtn).BindEvent(OnEnter,Define.UIEvent.Enter);
        GetObject((int)GamObjects.LobyBtn).BindEvent(OnExit,Define.UIEvent.Exit);
        GetObject((int)GamObjects.LobyBtn).BindEvent(_=>
        {
            Managers.Stage.ReturnToLoby();
            ClosePopupUI();
        });
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StageEnding : UI_Popup
{
    enum Images
    {
        TitleBG,
        ReturnBtn
    }
    enum Texts
    {
        TitleText,
        PlayTimeText,
        KillCountText,
        GoldText
    }

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        
        var go =GetImage((int)Images.ReturnBtn).gameObject;
        go.BindEvent(_=>
        {
            RetunToLoby();
        });
        go.BindEvent(OnEnter,Define.UIEvent.Enter);
        go.BindEvent(OnExit,Define.UIEvent.Exit);
    }

    private void RetunToLoby()
    {
        Managers.Data.ClearAbility();
        Managers.Stage.ReturnToLoby();
        Managers.UI.ClosePopupUI(this);
    }
}

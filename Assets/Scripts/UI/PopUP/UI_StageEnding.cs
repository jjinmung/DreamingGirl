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

    private void Awake()
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
        Managers.Stage.ReturnToLoby();
        ClosePopupUI();
    }

    public void SetText(bool isWin ,float playTime, int killCount, int gold)
    {
        TimeSpan t = TimeSpan.FromSeconds(playTime);
        GetText((int)Texts.TitleText).text = isWin ? "승 리" : "패 배";
        GetText((int)Texts.PlayTimeText).text = $"던전 플레이 시간 : {t.Minutes+(t.Hours*60)}분 {t.Seconds}초";
        GetText((int)Texts.KillCountText).text = $"적 처시수 : {killCount}마리";
        GetText((int)Texts.GoldText).text = $"획득 골드량 : {gold}gold";
    }
}

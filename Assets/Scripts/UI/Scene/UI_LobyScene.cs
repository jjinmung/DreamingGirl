using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LobyScene : UI_Scene
{
    enum Texts
    {
        PlayNewText,
        PlayContinueText,
        OptionText,
        ExitText
    }
    

    enum Images
    {
        BtnBGImange,
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
        
        BindingEvents();

        GetImage((int)Images.BtnBGImange).gameObject.SetActive(false);
        TextsInit();
    }

    void BindingEvents()
    {
        GameObject go;
        int count = Enum.GetValues(typeof(Texts)).Length;
        for (int i=0; i<count; i++)
        {
            go = GetText(i).gameObject;
            go.BindEvent(OnEnter,Define.UIEvent.Enter);
            go.BindEvent(OnExit,Define.UIEvent.Exit);
        }
        
        GetText((int)Texts.PlayNewText).gameObject.BindEvent(OnClickedPlay);
        GetText((int)Texts.PlayContinueText).gameObject.BindEvent(async (data) =>
        {
            await Managers.UI.ShowPopupUI<UI_SaveData>();
        });
        
        GetText((int)Texts.OptionText).gameObject.BindEvent(async (data) =>
        {
            await Managers.UI.ShowPopupUI<UI_Setting>();
        });
        
        GetText((int)Texts.ExitText).gameObject.BindEvent((data) =>
        {
            Application.Quit();
        });
       
        
    }
    
    private void OnClickedPlay(PointerEventData eventData)
    {
        Managers.Data.LoadGame(true);
        Managers.UI.Clear();
        Managers.Camera.LobyToCut();
    }

    private void TextsInit()
    {
        foreach (var text in Enum.GetValues(typeof(Texts)))
        {
            GetText((int)text).alpha = 0;
            GetText((int)text).DOFade(1f, 3f);
        }
        GetImage((int)Images.BtnBGImange).color = new Color(1,1,1,0);
        GetImage((int)Images.BtnBGImange).DOFade(1f, 3f);
    }

    protected override void OnEnter(PointerEventData eventData)
    {
        base.OnEnter(eventData);
        var go = GetImage((int)Images.BtnBGImange).gameObject;
        go.SetActive(true);
        go.transform.position = eventData.pointerEnter.transform.position+new Vector3(0,10f,0);

    }
    protected override void OnExit(PointerEventData eventData)
    {
        base.OnExit(eventData);
        GetImage((int)Images.BtnBGImange).gameObject.SetActive(false);
    }
}

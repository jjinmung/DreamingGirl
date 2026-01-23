using System;
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

    enum GameObjects
    {
        Btns,
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
        Bind<GameObject>(typeof(GameObjects));
        
        BindingEvents();

        GetImage((int)Images.BtnBGImange).gameObject.SetActive(false);
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
        GetText((int)Texts.PlayContinueText).gameObject.BindEvent((data) =>
        {
            Managers.UI.ShowPopupUI<UI_SaveData>();
        });
       
        
    }
    
    private void OnClickedPlay(PointerEventData eventData)
    {
        Managers.Data.LoadGame(true);
        Managers.UI.Clear();
        Managers.Camera.LobyToBattle();
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

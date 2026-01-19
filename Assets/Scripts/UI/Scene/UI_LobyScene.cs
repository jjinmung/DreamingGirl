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
        Title
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
        
        go = GetText((int)Texts.PlayNewText).gameObject;
        go.BindEvent(OnClickedPlay);
       
        
    }
    
    private void OnClickedPlay(PointerEventData eventData)
    {
        GetImage((int)Images.Title).gameObject.SetActive(false);
        GetImage((int)Images.BtnBGImange).gameObject.SetActive(false);
        GetObject((int)GameObjects.Btns).SetActive(false);
        Managers.Camera.LobyToBattle();
    }
    
    

    private void OnEnter(PointerEventData eventData)
    {
        eventData.pointerEnter.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        
        var go = GetImage((int)Images.BtnBGImange).gameObject;
        go.SetActive(true);
        go.transform.position = eventData.pointerEnter.transform.position+new Vector3(0,10f,0);

    }
    private void OnExit(PointerEventData eventData)
    {
        eventData.pointerEnter.transform.localScale = Vector3.one;
        GetImage((int)Images.BtnBGImange).gameObject.SetActive(false);
    }
}

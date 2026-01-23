using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SaveData : UI_Popup
{
    enum Texts
    {
        Slot1Text,
        Slot2Text,
        Slot3Text,
        Slot4Text,
    }

    enum Images
    {
        Slot1CloseBtn,
        Slot2CloseBtn,
        Slot3CloseBtn,
        Slot4CloseBtn,
        CloseBtn,
        Slot1,
        Slot2,
        Slot3,
        Slot4,
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
        
        BindeEvents();
        SetText();
    }

    void BindeEvents()
    {
        foreach(var image in Enum.GetValues(typeof(Images)))
        {
            if ((int)image > (int)Images.CloseBtn) break;
            GetImage((int)image).gameObject.BindEvent(OnEnter,Define.UIEvent.Enter);
            GetImage((int)image).gameObject.BindEvent(OnExit,Define.UIEvent.Exit);
            if ((int)image < (int)Images.CloseBtn)
            {
                GetImage((int)image).gameObject.BindEvent((data) =>
                {
                    data.pointerClick.SetActive(false);
                    Managers.Data.ClearSlot((int)image+1);
                    SetText();
                });
            } 
           
        }

        GetImage((int)Images.CloseBtn).gameObject.BindEvent((data) =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }

    void SetText()
    {
        foreach(var text in Enum.GetValues(typeof(Texts)))
        {
            GetText((int)text).text = Managers.Data.GetSlotSummary((int)text + 1);
            if (Managers.Data.HasSaveFile((int)text + 1))
            {
                GetImage((int)text).gameObject.SetActive(true);
                GetImage((int)text+5).gameObject.BindEvent((data) =>
                {
                    Managers.Data.ChangeSlot((int)text+1);
                    Managers.Data.LoadGame();
                    Managers.UI.Clear();
                    Managers.Camera.LobyToBattle();
                });
            }
            else
            {
                GetImage((int)text).gameObject.SetActive(false);
            }
                
        }
    }
}

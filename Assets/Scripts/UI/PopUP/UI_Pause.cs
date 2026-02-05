using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
public class UI_Pause : UI_Popup
{
    public List<UI_Card> ablityList = new List<UI_Card>();
    public ScrollRect myScrollRect;

    enum GameObjects
    {
        ability1,
        ability2,
        ability3,
        ability4,
        ability5,
        ability6,
        ability7,
        ability8,
        ability9,
        ability10,
    }

    enum Images
    {
        Continue,
        Exit, 
    }

    enum Texts
    {
        MaxHpText,
        AttackText,
        MoveSpeedText,
        AttackSpeedText,
        CriticalText,
    }

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        SetAbility();
        SetStat();

        StartCoroutine(InitScrollAfterPopup());
    }
    IEnumerator InitScrollAfterPopup()
    {
        // 애니메이션이 어느 정도 진행될 때까지 기다리거나 
        // 혹은 완전히 끝날 때까지 WaitForSeconds 사용
        yield return new WaitForSeconds(0.3f); 

        Canvas.ForceUpdateCanvases();
        myScrollRect.verticalNormalizedPosition = 1f;
    }


    public override void Init()
    {
        base.Init();
        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        myScrollRect = GetComponentInChildren<ScrollRect>();
        BindEvents();
    }
   

    void BindEvents()
    {
        GetImage((int)Images.Continue).gameObject.BindEvent(_ =>
        {
            Time.timeScale = 1f;
            Managers.UI.ClosePopupUI(this);
        });
        GetImage((int)Images.Continue).gameObject.BindEvent(OnEnter, UIEvent.Enter);
        GetImage((int)Images.Continue).gameObject.BindEvent(OnExit, UIEvent.Exit);
        
        GetImage((int)Images.Exit).gameObject.BindEvent(_ =>
        {
            Managers.Data.SaveGame();
            Managers.Camera.BattleToLoby();
            Time.timeScale = 1f;
            Managers.UI.ClosePopupUI(this);
        });
        GetImage((int)Images.Exit).gameObject.BindEvent(OnEnter, UIEvent.Enter);
        GetImage((int)Images.Exit).gameObject.BindEvent(OnExit, UIEvent.Exit);
        
        foreach (var ability in Enum.GetValues(typeof(GameObjects)))
        {
            int index = (int)ability;  
            var uiCard = GetObject(index).GetComponent<UI_Card>();
            uiCard.Init();
            ablityList.Add(uiCard);
        }
    }
    

    void SetAbility()
    {
        for (int i = 0; i < Enum.GetValues(typeof(GameObjects)).Length; i++)
        {
            GetObject(i).SetActive(false);
        }

        int index = 0;
        foreach (var pair in Managers.Data.AbilityDict)
        {
            var ability = pair.Value;

            // 스택이 0인 능력은 제외
            if (ability.stack <= 0)
                continue;

            ablityList[index].gameObject.SetActive(true);
            ablityList[index].SetCard(pair.Key);
            index++;
        }
    }

    void SetStat()
    {
        GetText((int)Texts.MaxHpText).text = $"{Mathf.RoundToInt(Managers.Player.Data.maxHp.TotalValue)}";
        GetText((int)Texts.AttackText).text = $"{Mathf.RoundToInt(Managers.Player.Data.damage.TotalValue)}";
        GetText((int)Texts.MoveSpeedText).text = $"{Mathf.RoundToInt(Managers.Player.Data.moveSpeed.TotalValue)}";
        GetText((int)Texts.AttackSpeedText).text = $"{Mathf.RoundToInt(Managers.Player.Data.attackSpeed.TotalValue)}";
        GetText((int)Texts.CriticalText).text = $"{Mathf.RoundToInt(Managers.Player.Data.criticalChance.TotalValue)}%";
        
    }
}

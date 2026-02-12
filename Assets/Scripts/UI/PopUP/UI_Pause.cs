using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
public class UI_Pause : UI_Popup
{
    public List<UI_Card> ablityList = new List<UI_Card>();
    public ScrollRect myScrollRect;
    private Color activeRank = new Color32(234, 167, 89, 255);
    private Color inActive = new Color32(90, 90, 90, 255);
    

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
        SubDescribe,
        Rank1,
        Rank2,
        Rank3,
        Rank4,
        Rank5,
    }

    enum Images
    {
        Continue,
        Option,
        Exit, 
    }


    enum Texts
    {
        MaxHpText,
        AttackText,
        MoveSpeedText,
        AttackSpeedText,
        CriticalText,
        NameText,
        AbilityText,
        description1Text,
        description2Text,
        description3Text,
        description4Text,
        description5Text,
    }

    private void Awake()
    {
        Init();
    }

    protected override void OnEnable()
    {
        Time.timeScale = 0;
        SetAbility();
        SetStat();
        StartCoroutine(InitScrollAfterPopup());
        base.OnEnable();
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
    }

    IEnumerator InitScrollAfterPopup()
    {
        // 애니메이션이 어느 정도 진행될 때까지 기다리거나 
        // 혹은 완전히 끝날 때까지 WaitForSeconds 사용
        yield return new WaitForSeconds(0.5f); 

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
        foreach (var ability in Enum.GetValues(typeof(GameObjects)))
        {
            int index = (int)ability;
            if (index >= 10) break;
            var uiCard = GetObject(index).GetComponent<UI_Card>();
            uiCard.Init();
            ablityList.Add(uiCard);
        }
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

        
        GetImage((int)Images.Option).gameObject.BindEvent(_ =>
        {
            Managers.UI.ShowPopupUI<UI_Setting>().Forget();
        });
        GetImage((int)Images.Option).gameObject.BindEvent(OnEnter, UIEvent.Enter);
        GetImage((int)Images.Option).gameObject.BindEvent(OnExit, UIEvent.Exit);
        
        GetImage((int)Images.Exit).gameObject.BindEvent(_ =>
        {
            Managers.Data.SaveGame();
            Managers.Camera.BattleToLoby();
            Time.timeScale = 1f;
            Managers.UI.ClosePopupUI(this);
        });
        GetImage((int)Images.Exit).gameObject.BindEvent(OnEnter, UIEvent.Enter);
        GetImage((int)Images.Exit).gameObject.BindEvent(OnExit, UIEvent.Exit);
        
        
    }
    

    void SetAbility()
    {
        for (int i = 0; i < 10; i++)
        {
            GetObject(i).SetActive(false);
        }
        GetObject((int)GameObjects.SubDescribe).SetActive(false);

        int index = 0;
        foreach (var pair in Managers.Data.AbilityDict)
        {
            var ability = pair.Value;
            var abilityID = pair.Key;
            // 스택이 0인 능력은 제외
            if (ability.stack <= 0)
                continue;

            ablityList[index].gameObject.SetActive(true);
            ablityList[index].SetCard(abilityID);
            
            //이벤트 바인딩
            ablityList[index].gameObject.ClearEvent();
            ablityList[index].gameObject.BindEvent(_ =>
            {
                ShowAbility(abilityID);
            },UIEvent.Enter);
            ablityList[index].gameObject.BindEvent(_ =>
            {
                GetObject((int)GameObjects.SubDescribe).SetActive(false);
            }, UIEvent.Exit);
            
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
    
    void ShowAbility(AbilityID id)
    {
        var ability = Managers.Data.AbilityDict[id];
        GetObject((int)GameObjects.SubDescribe).SetActive(true);
        GetText((int)Texts.AbilityText).text = ability.data.abilityName;
        var rank = ability.stack;
        var RankStartIndex = (int)GameObjects.Rank1;
        var TesxtStartIndex = (int)Texts.description1Text;
        for (int i = 0; i < 5; i++)
        {
            var stars=GetObject(RankStartIndex+i).GetComponentsInChildren<Image>();
            if (i < rank)
            {
                GetText(TesxtStartIndex+i).color = Color.white;
                foreach (var star in stars)
                    star.color = activeRank;
            }
            else
            {
                GetText(TesxtStartIndex+i).color = inActive;
                foreach (var star in stars)
                    star.color = inActive;
            }
            GetText(TesxtStartIndex+i).text = $"{ability.data.description[i]}";
            
        }
        


        
        
    }
}

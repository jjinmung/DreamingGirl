using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;
public class UI_Ability :UI_Popup
{
    bool isInit = false;
    private AbilityID[] abilityIDs;
    private List<UI_Card> uiCards;
    enum GameObjects
    {
        Card1,
        Card2,
        Card3,
    }

    enum Images
    {
        Image1,
        Image2, 
        Image3,
    }

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        SetCard();
        foreach (var card in Enum.GetValues(typeof(GameObjects)))
        {
            int index = (int)card;
            uiCards[index].SetCard(abilityIDs[index]);
           
        }
    }

    public override void Init()
    {
        base.Init();
		Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));
        
        uiCards=new List<UI_Card>();
        
        foreach (var card in Enum.GetValues(typeof(GameObjects)))
        {
            int index = (int)card;  
            var uiCard = GetObject(index).GetComponent<UI_Card>();
            uiCard.Init();
            uiCards.Add(uiCard);
        }
        foreach (var image in Enum.GetValues(typeof(Images)))
        {
            int index = (int)image;  
            var go = GetImage(index).gameObject;
            go.BindEvent(OnEnter,UIEvent.Enter);
            go.BindEvent(OnExit,UIEvent.Exit);
            go.BindEvent(_ =>
            {
                Managers.Data.AbilityDict[abilityIDs[index]].AddStack();
                Managers.UI.ClosePopupUI(this);
                //시간 재개
                Time.timeScale = 1;
            });
        }

	}
    
    public void SetCard()
    {
        if(abilityIDs==null)
            abilityIDs = new AbilityID[3];
        
        // 1. 후보 AbilityID 리스트 만들기
        List<AbilityID> candidates = new List<AbilityID>();

        foreach (var pair in Managers.Data.AbilityDict)
        {
            var ability = pair.Value;

            // maxStack 도달한 능력은 제외
            if (ability.stack >= ability.data.maxStack)
                continue;

            candidates.Add(pair.Key);
        }

        // 후보가 3개 미만이면 그대로 처리
        if (candidates.Count <= 3)
        {
            for (int i = 0; i < candidates.Count; i++)
                abilityIDs[i] = candidates[i];

            return;
        }

        // 2. 랜덤 셔플
        for (int i = 0; i < candidates.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, candidates.Count);
            (candidates[i], candidates[rand]) = (candidates[rand], candidates[i]);//두 값을 swap
        }

        // 3. 앞에서 3개 선택
        for (int i = 0; i < 3; i++)
        {
            abilityIDs[i] = candidates[i];
        }
    }
}
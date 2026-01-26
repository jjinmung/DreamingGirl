using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;
public class UI_PassiveSkill : UI_Popup
{
    Color InactiveColor= new Color(100/255f,100/255f,100/255f,1);
    Color ActiveColor= new Color(1,1,1,1);
    
    PassiveSkillID selectedSkillID;
    enum Texts
    {
        Skill_Name,
        Skill_Des,
        Skill_Price
    }

    

    enum Images
    {
        DungeonBreath_Icon,     
        LifeDrain_Icon,         

        CriticalSense_Icon,     
        PowerAwakening_Icon,    
        HasteInstinct_Icon,    
        DecisiveStrike_Icon,    

        UnyieldingVitality_Icon,
        LastChance_Icon,       

        SwiftBoots_Icon,        
        FireSpirit_Icon,       
        IceSpirit_Icon,       
        FateChoice_Icon,
        
        Background,
        CloseBtn
    }
    enum GameObjects
    {
        DungeonBreath,     
        LifeDrain,         

        CriticalSense,     
        PowerAwakening,    
        HasteInstinct,     
        DecisiveStrike,    

        UnyieldingVitality,
        LastChance,        

        SwiftBoots,        
        FireSpirit,        
        IceSpirit,       
        FateChoice,
        
        SKill_Info,
        Skill_InActive,
        Skill_Active
    }

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        Managers.Player.PlayerControl.InputActive(false);
    }

    public override void Init()
    {
        base.Init();
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        BindEvents();
        RefreshUI();
        
        GetObject((int)GameObjects.SKill_Info).SetActive(false);
    }

    private void BindEvents()
    {
        // 1. 배경화면 이벤트
        GetImage((int)Images.Background).gameObject.BindEvent(_ => HideInfo());

        //2.닫기 버튼 이벤트
        var closebtn = GetImage((int)Images.CloseBtn).gameObject;
        closebtn.BindEvent(_ =>
        {
            Close();
        });
        closebtn.BindEvent(OnEnter,UIEvent.Enter);
        closebtn.BindEvent(OnExit,UIEvent.Exit);

        // 3. 패시브 슬롯 이벤트 루프
        foreach (PassiveSkillID id in Enum.GetValues(typeof(PassiveSkillID)))
        {
            PassiveSkillID targetID = id;
            // 이벤트 바인딩
            GetObject((int)targetID).BindEvent(_ => ShowInfo(targetID));
        }
        
        // 4. 구매버튼 이벤트
        GetObject((int)GameObjects.Skill_InActive).BindEvent(BuyPassive);
        GetObject((int)GameObjects.Skill_InActive).BindEvent(OnEnter, UIEvent.Enter);
        GetObject((int)GameObjects.Skill_InActive).BindEvent(OnExit, UIEvent.Exit);
    }

    private void RefreshUI()
    {
        // 소유 여부에 따라 아이콘(Images)의 색상을 변경
        foreach (PassiveSkillID id in Enum.GetValues(typeof(PassiveSkillID)))
        {
            
            Image iconImage = GetImage((int)GetIconEnum(id)); 
            
            if (iconImage != null)
            {
                bool hasSkill = Managers.Data.SaveData.player.HasPassive(id);
                iconImage.color = hasSkill ? ActiveColor : InactiveColor;
            }
        }
    }

    // 스킬 ID에 대응하는 Images Enum 값을 찾아주는 헬퍼
    private Images GetIconEnum(PassiveSkillID id)
    {
        // Enum.Parse를 활용해 문자열로 매칭
        return (Images)Enum.Parse(typeof(Images), $"{id}_Icon");
    }
    
    private void ShowInfo(PassiveSkillID id)
    {
        if (!Managers.Data.PassiveDict.TryGetValue(id, out var skillData)) return;

        GetObject((int)GameObjects.SKill_Info).SetActive(true);
        GetText((int)Texts.Skill_Name).text = skillData.skillName;
        GetText((int)Texts.Skill_Des).text = skillData.description;
        GetText((int)Texts.Skill_Price).text = skillData.price.ToString();

        // 소태유 여부에 따른 버튼 상 분기
        bool hasPassive = Managers.Data.SaveData.player.HasPassive(id);
        GetObject((int)GameObjects.Skill_Active).SetActive(hasPassive);
        GetObject((int)GameObjects.Skill_InActive).SetActive(!hasPassive);

        selectedSkillID = id;
    }

    private void HideInfo() => GetObject((int)GameObjects.SKill_Info).SetActive(false);

    public void Close()
    {
        HideInfo();
        Managers.Player.PlayerControl.InputActive(true);
        Managers.UI.ClosePopupUI(this);
    }

    private void BuyPassive(PointerEventData data)
    {
        if (Managers.Data.SaveData.player.HasPassive(selectedSkillID)) return;

        var skillData = Managers.Data.PassiveDict[selectedSkillID];
        int currentGold = Managers.Data.SaveData.player.gold;

        if (currentGold >= skillData.price)
        {
            // 1. 재화 차감 및 데이터 추가
            Managers.Player.AddGold(-skillData.price);
            Managers.Data.SaveData.player.ownedPassives.Add(selectedSkillID);
        
            // 2. 패시브 효과 즉시 적용 (Player에게 알림)
            //skillData.GetEffect().Apply();

            // 3. UI 갱신
            RefreshUI();
            ShowInfo(selectedSkillID);
        
            // 4. 저장
            Managers.Data.SaveGame();
            Debug.Log($"{selectedSkillID} 구매 성공!");
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
            // TODO: "골드 부족" 팝업이나 텍스트 연출
        }
    }
    
    
    
}

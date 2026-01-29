using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BattleScene : UI_Scene
{
    private PlayerData _playerData => Managers.Player.Data;

    private float _currentGold = 0;
    
    // 캐싱을 위한 딕셔너리
    private readonly Dictionary<Define.RoomType, Sprite> _spriteCache = new Dictionary<Define.RoomType, Sprite>();

    enum Texts 
    { 
        GoldText, 
        StageText,
        LevelText 
    }

    enum Images
    {
        FadeOut, 
        Node1, 
        Node2, 
        Node3,
        Image_Skill1,
        Image_Skill2,
        Image_Skill3,
        Image_Skill4,
        Image_DashCool,
        Image_Skill1Cool,
        Image_Skill2Cool,
        Image_Skill3Cool,
        Image_Skill4Cool,
    }

    enum GameObjects
    {
        Line1,
        Line2, 
        Line3, 
        Map,
        SkillBar,
        Info
    }

    enum Sliders
    {
        LevelBar
    }

    private void Start() => Init();

    public override void Init()
    {
        base.Init();
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Slider>(typeof(Sliders));
        
        // 이벤트 연결 (중복 방지 safe subtract)
        Managers.Stage.ExitRoom -= FadeOut;
        Managers.Stage.ExitRoom += FadeOut;
        Managers.Stage.EnterRoom -= FadeIn;
        Managers.Stage.EnterRoom += FadeIn;
        Managers.Player.OnDataChanged -= RefreshGoldUI;
        Managers.Player.OnDataChanged += RefreshGoldUI;

        // 초기 연출
        var fadeImg = GetImage((int)Images.FadeOut);
        fadeImg.color = Color.black;
        fadeImg.DOFade(0f, 4f).SetEase(Ease.InQuad);

        //골드 UI동기화
        RefreshGoldUI();
        
        //첫 진입은 로비라 전투 UI 비활성화
        BattleUIActive(false);
        
        //경험치 초기화
        InitExp();
        Managers.Player.OnLevelUp -= AddExp;
        Managers.Player.OnLevelUp += AddExp;
        
        //스킬바 초기화
        GetImage((int)Images.Image_DashCool).fillAmount = 0f;
        GetImage((int)Images.Image_Skill1Cool).fillAmount = 0f;
        GetImage((int)Images.Image_Skill2Cool).fillAmount = 0f;
        GetImage((int)Images.Image_Skill3Cool).fillAmount = 0f;
        GetImage((int)Images.Image_Skill4Cool).fillAmount = 0f;
        RefreshSkillBar();
        Managers.Player.PlayerControl.OnGetActiveSKill -= RefreshSkillBar;
        Managers.Player.PlayerControl.OnGetActiveSKill += RefreshSkillBar;
        
        Managers.Player.PlayerControl.OnUseActiveSKill -= UseSkill;
        Managers.Player.PlayerControl.OnUseActiveSKill += UseSkill;
    }

    private void FadeOut()
    {
        var fadeImg = GetImage((int)Images.FadeOut);
        fadeImg.DOKill();
        fadeImg.DOFade(1f, 2f).SetEase(Ease.InQuart);
    }
    private void FadeIn()
    {
        var fadeImg = GetImage((int)Images.FadeOut);
        fadeImg.DOKill();
        fadeImg.DOFade(0f, 1f).SetEase(Ease.InQuad);
    }

    #region 골드 관련 함수

    private void RefreshGoldUI() => UpdateGold(Managers.Player.Data.gold);

    public void UpdateGold(float targetGold)
    {
        DOTween.To(() => _currentGold, x => _currentGold = x, targetGold, 1f)
            .OnUpdate(() => {
                GetText((int)Texts.GoldText).text = Mathf.FloorToInt(_currentGold).ToString();
            });
    }

    #endregion
  

    #region 맵 관련 함수
    public void SetMap(List<RoomNode> roomNodes,int depth)
    {
        GetText((int)Texts.StageText).text = $"{depth}";
        
        int count = roomNodes.Count;

        // 1. 모든 라인과 노드 초기화 
        for (int i = 1; i <= 3; i++)
        {
            GetObject(i - 1).SetActive(false); 
        }

        // 2. 개수에 따른 로직 분기 
        if (count == 1)
        {
            SetNodeActive(1, true, roomNodes[0].type); // Node2만 활성화
        }
        else if (count == 2)
        {
            SetNodeActive(0, true, roomNodes[0].type); // Node1
            SetNodeActive(2, true, roomNodes[1].type); // Node3
        }
        else if (count == 3)
        {
            for (int i = 0; i < 3; i++) SetNodeActive(i, true, roomNodes[i].type);
        }
    }

    private void SetNodeActive(int index, bool isActive, Define.RoomType type)
    {
        // Line은 GameObjects enum 순서대로 (0, 1, 2)
        // Node는 Images enum 순서대로 (FadeOut이 0이므로 Node1은 1, 2, 3)
        GetObject(index).SetActive(isActive);
        if (isActive)
        {
            GetImage(index + 1).sprite = GetSprite(type);
        }
    }

    private Sprite GetSprite(Define.RoomType type)
    {
        if (_spriteCache.TryGetValue(type, out Sprite cachedSprite))
            return cachedSprite;

        string address = type switch
        {
            Define.RoomType.Monster => Address.EnemyMap,
            Define.RoomType.Event => Address.EventMap,
            Define.RoomType.Boss => Address.BossMap,
            _ => null
        };

        if (string.IsNullOrEmpty(address)) return null;

        Sprite sprite = Managers.Resource.Load<Sprite>(address);
        _spriteCache[type] = sprite;
        return sprite;
    }
    

    #endregion

    #region 경험치 관련 함수

    public void InitExp()
    {
        GetText((int)Texts.LevelText).text = _playerData.level.ToString();
        // 초기화 시에는 즉시 반영
        Get<Slider>((int)Sliders.LevelBar).value = (float)_playerData.currentExp / _playerData.nextLevelExp;
    }

    public void AddExp(int amount)
    {
        var levelBar = Get<Slider>((int)Sliders.LevelBar);
        int totalTargetExp = _playerData.currentExp + amount;

        // DOTween Sequence 생성
        Sequence levelUpSeq = DOTween.Sequence();

        // 연속 레벨업 처리 루프
        while (totalTargetExp >= _playerData.nextLevelExp)
        {
            // 1. 현재 레벨의 최대치까지 바를 채움
            levelUpSeq.Append(levelBar.DOValue(1f, 0.4f).SetEase(Ease.OutQuad));

            // 2. 바가 다 찬 직후 실행될 로직 (데이터 갱신 및 텍스트 업데이트)
            levelUpSeq.AppendCallback(() => {
                levelBar.value = 0; // 바 초기화
                Levelup();          // 레벨 및 다음 경험치 통 갱신
                Managers.Player.LevelUp(); 
            });

            // 계산: 남은 경험치 갱신
            amount -= (_playerData.nextLevelExp - _playerData.currentExp);
            _playerData.currentExp = 0; 
            totalTargetExp = amount;
        }

        // 3. 마지막으로 남은 경험치만큼 바를 채움
        _playerData.currentExp = totalTargetExp;
        float finalRatio = (float)_playerData.currentExp / _playerData.nextLevelExp;
        levelUpSeq.Append(levelBar.DOValue(finalRatio, 0.4f).SetEase(Ease.OutQuad));
    }

    private void Levelup()
    {
        _playerData.level++;
        // currentExp는 위 로직에서 소진되므로 통(Max)만 키워줌
        _playerData.nextLevelExp = Mathf.RoundToInt(_playerData.nextLevelExp * 1.2f);
        
        // 텍스트 업데이트 및 살짝 커지는 연출 추가 
        var levelText = GetText((int)Texts.LevelText);
        levelText.text = _playerData.level.ToString();
        levelText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f); 
    }

    #endregion

    #region 스킬 관련함수

    public void SkillBarInit()
    {
        GetImage((int)Images.Image_Skill1).gameObject.SetActive(false);
        GetImage((int)Images.Image_Skill2).gameObject.SetActive(false);
        GetImage((int)Images.Image_Skill3).gameObject.SetActive(false);
        GetImage((int)Images.Image_Skill4).gameObject.SetActive(false);
    }

    private void RefreshSkillBar()
    {
        SkillBarInit();
        var skills = Managers.Player.PlayerControl.ActiveSkills;
        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i]!=Define.AbilityID.None)
            {
                GetImage(i+4).gameObject.SetActive(true);
                GetImage(i + 4).sprite = Managers.Data.AbilityDict[skills[i]].data.icon;
            }
        }
    }

    public void UseSkill(int index, float cooldown)
    {
        // index 0: Dash
        // index 1~4: Skill 1~4
        // Images enum에서 Image_DashCool(8)부터 순서대로 배치되어 있으므로 index를 더해줍니다.
        int targetImageIndex = (int)Images.Image_DashCool + index;
    
        Image coolImage = GetImage(targetImageIndex);
    
        if (coolImage == null) return;

        // 기존에 돌고 있던 트윈이 있다면 제거 (연속 입력 대비)
        coolImage.DOKill();
    
        // 쿨타임 연출 시작
        coolImage.fillAmount = 1f; // 먼저 꽉 채우고
        coolImage.DOFillAmount(0f, cooldown) // 0까지 cooldown 시간 동안
            .SetEase(Ease.Linear) // 쿨타임은 일정하게 줄어들어야 하므로 Linear 권장
            .OnComplete(() => {
                // 완료 후 추가 연출이 필요하다면 여기에 작성 (예: 반짝임)
                coolImage.fillAmount = 0f;
            });
    }
    #endregion

    public void BattleUIActive(bool isActive)
    {
        Get<Slider>((int)Sliders.LevelBar).gameObject.SetActive(isActive);
        GetObject((int)GameObjects.Map).SetActive(isActive);
        GetObject((int)GameObjects.SkillBar).SetActive(isActive);
    }
    
    public void AllUIActive(bool isActive)
    {
        Get<Slider>((int)Sliders.LevelBar).gameObject.SetActive(isActive);
        GetObject((int)GameObjects.Map).SetActive(isActive);
        GetObject((int)GameObjects.SkillBar).SetActive(isActive);
        GetObject((int)GameObjects.Info).SetActive(isActive);
    }
    
    
}
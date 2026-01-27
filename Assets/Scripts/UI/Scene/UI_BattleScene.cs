using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BattleScene : UI_Scene
{
    private float _currentGold = 0;
    
    // 캐싱을 위한 딕셔너리
    private readonly Dictionary<Define.RoomType, Sprite> _spriteCache = new Dictionary<Define.RoomType, Sprite>();

    enum Texts { GoldText }
    enum Images { FadeOut, Node1, Node2, Node3 }
    enum GameObjects { Line1, Line2, Line3, Map, LevelBar,SkillBar,Info }

    private void Start() => Init();

    public override void Init()
    {
        base.Init();
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

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
    }

    private void FadeOut() => GetImage((int)Images.FadeOut).DOFade(1f, 2f).SetEase(Ease.InQuart);
    private void FadeIn() => GetImage((int)Images.FadeOut).DOFade(0f, 1f).SetEase(Ease.InQuad);
   
    private void RefreshGoldUI() => UpdateGold(Managers.Player.GetGold());

    public void UpdateGold(float targetGold)
    {
        DOTween.To(() => _currentGold, x => _currentGold = x, targetGold, 1f)
            .OnUpdate(() => {
                GetText((int)Texts.GoldText).text = Mathf.FloorToInt(_currentGold).ToString();
            });
    }

    #region 맵 관련 함수
    public void SetMap(List<RoomNode> roomNodes)
    {
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

    public void BattleUIActive(bool isActive)
    {
        GetObject((int)GameObjects.LevelBar).SetActive(isActive);
        GetObject((int)GameObjects.Map).SetActive(isActive);
        GetObject((int)GameObjects.SkillBar).SetActive(isActive);
    }
    
    public void AllUIActive(bool isActive)
    {
        GetObject((int)GameObjects.LevelBar).SetActive(isActive);
        GetObject((int)GameObjects.Map).SetActive(isActive);
        GetObject((int)GameObjects.SkillBar).SetActive(isActive);
        GetObject((int)GameObjects.Info).SetActive(isActive);
    }
    
}
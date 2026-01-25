using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BattleScene : UI_Scene
{
    private float _currentGold = 0;
    enum Texts
    {
        GoldText
    }
    

    enum Images
    {
        FadeOut
    }

    enum GameObjects
    {
        
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

        Managers.Stage.ExitRoom -= FadeOut;
        Managers.Stage.ExitRoom += FadeOut;
        
        Managers.Stage.EnterRoom -= FadeIn;
        Managers.Stage.EnterRoom += FadeIn;

        
        GetImage((int)Images.FadeOut).color = new Color(0, 0, 0, 1);
        GetImage((int)Images.FadeOut).DOFade(0f, 4f).SetEase(Ease.InQuad);
        
        // 1. 이벤트 구독 (데이터가 바뀔 때마다 UpdateGold 실행)
        Managers.Player.OnDataChanged -= RefreshUI;
        Managers.Player.OnDataChanged += RefreshUI;

        // 2. 초기 데이터 반영
        RefreshUI();
    }
    
    private void FadeOut()
    {
        Sequence FadeSeq = DOTween.Sequence();
        
        FadeSeq.AppendCallback(() =>
        {
            GetImage((int)Images.FadeOut).DOFade(1f, 2f).SetEase(Ease.InQuart);
        });
    }
    
    private void FadeIn()
    {
        GetImage((int)Images.FadeOut).DOFade(0f, 1f).SetEase(Ease.InQuad);
    }

    private void RefreshUI()
    {
        // Managers.Player를 통해 현재 골드 값을 전달
        UpdateGold(Managers.Player.GetGold()); 
    }

    public void UpdateGold(float targetGold)
    {
        // 이전 답변에서 알려드린 DOTween 코드를 여기에 작성
        DOTween.To(() => _currentGold, x => _currentGold = x, targetGold, 1f)
            .OnUpdate(() =>
            {
                GetText((int)Texts.GoldText).text = $"{(int)_currentGold}";
            });
    }
    
    
}


using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BattleScene : UI_Scene
{
    enum Texts
    {
        
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
}


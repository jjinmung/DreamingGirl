using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerHPBar : UI_Base
{
    private float hp;
    private float hpMax;
    private Coroutine _subBarCoroutine;
    
    enum Texts
    {
        HPText
    }

    enum Sliders
    {
        MainBar,
        SubBar,
    }

    enum GameObjects
    {
        HPLines
    }

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        Bind<GameObject>(typeof(GameObjects));
        
        Managers.Player.TakeDamageAction-=TakeDamage;
        Managers.Player.TakeDamageAction+=TakeDamage;
        Managers.Player.DieAcation -= Destroy;
        Managers.Player.DieAcation  += Destroy;
       
        
    }


    public void SetMaxHP(float maxHp)
    {
        this.hpMax = maxHp;
        this.hp = maxHp;
        Get<Slider>((int)Sliders.MainBar).value = 1f;
        Get<Slider>((int)Sliders.SubBar).value = 1f;
        GetText((int)Texts.HPText).text = $"{maxHp}";

        //체력바 선 그리기
        float ScaleX = 1000f / maxHp;
        var children = Get<GameObject>((int)GameObjects.HPLines).
            transform.Cast<Transform>()
            .Select(t => t.gameObject.transform)
            .ToArray();

        foreach (var child in children)
        {
            child.localScale = new Vector3(ScaleX, 1, 1);
        }
    }
    
    public void TakeDamage(float damage)
    {
        hp = Mathf.Max(0, hp - damage); // 0 이하로 내려가지 않게 방지
        
        Get<Slider>((int)Sliders.MainBar).value = hp / hpMax;
        GetText((int)Texts.HPText).text = $"{Mathf.CeilToInt(hp)}"; // 현재 남은 피 표시

        // 이전 애니메이션이 돌고 있다면 멈추고 새로 시작
        if (_subBarCoroutine != null)
            StopCoroutine(_subBarCoroutine);
        if (hp > 0)
            _subBarCoroutine = StartCoroutine(SubBarAnim());
    }
    
    IEnumerator SubBarAnim()
    {
        yield return new WaitForSeconds(0.3f);

        Slider subSlider = Get<Slider>((int)Sliders.SubBar);
        float targetValue = hp / hpMax;
        float startValue = subSlider.value;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            subSlider.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null;
        }

        subSlider.value = targetValue;
        _subBarCoroutine = null;
    }

    public void Destroy()
    {
        Managers.Resource.Destroy(gameObject);
    }

}

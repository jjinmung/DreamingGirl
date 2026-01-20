using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_EnemyHPBar : UI_Base
{
    private float hp;
    private float hpMax;
    private Coroutine _subBarCoroutine;
    
    enum Sliders
    {
        MainBar,
        SubBar,
    }


    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        Bind<Slider>(typeof(Sliders));
        
    }
    
    public void SetMaxHP(float maxHp)
    {
        this.hpMax = maxHp;
        this.hp = maxHp;
        Get<Slider>((int)Sliders.MainBar).value = 1f;
        Get<Slider>((int)Sliders.SubBar).value = 1f;
    }
    
    public void TakeDamage(float damage)
    {
        hp = Mathf.Max(0, hp - damage); // 0 이하로 내려가지 않게 방지
        
        Get<Slider>((int)Sliders.MainBar).value = hp / hpMax;

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

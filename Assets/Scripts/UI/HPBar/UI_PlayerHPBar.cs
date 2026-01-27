using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerHPBar : UI_Base
{
    private float _currentHp;
    private float _maxHp;
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


    public void SetMaxHP(float maxHp,float currentHp)
    {
        _maxHp = maxHp;
        _currentHp = currentHp;
        Get<Slider>((int)Sliders.MainBar).value = _currentHp/_maxHp;
        Get<Slider>((int)Sliders.SubBar).value =  _currentHp/_maxHp;
        GetText((int)Texts.HPText).text = $"{(int)currentHp}";

        //체력바 선 그리기
        float ScaleX = 1000f / _maxHp;
        var children = Get<GameObject>((int)GameObjects.HPLines).
            transform.Cast<Transform>()
            .Select(t => t.gameObject.transform)
            .ToArray();

        foreach (var child in children)
        {
            child.localScale = new Vector3(ScaleX, 1, 1);
        }
        Get<GameObject>((int)GameObjects.HPLines).SetActive(false);
        Get<GameObject>((int)GameObjects.HPLines).SetActive(true);
    }
    
    public void TakeDamage(float damage)
    {
        _currentHp = Mathf.Max(0, _currentHp - damage); // 0 이하로 내려가지 않게 방지
        
        Get<Slider>((int)Sliders.MainBar).value = _currentHp / _maxHp;
        GetText((int)Texts.HPText).text = $"{(int)(_currentHp)}"; // 현재 남은 피 표시

        // 이전 애니메이션이 돌고 있다면 멈추고 새로 시작
        if (_subBarCoroutine != null)
            StopCoroutine(_subBarCoroutine);
        if (_currentHp > 0)
            _subBarCoroutine = StartCoroutine(SubBarAnim());
    }
    
    IEnumerator SubBarAnim()
    {
        yield return new WaitForSeconds(0.3f);

        Slider subSlider = Get<Slider>((int)Sliders.SubBar);
        float targetValue = _currentHp / _maxHp;
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

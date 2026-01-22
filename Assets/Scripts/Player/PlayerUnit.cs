using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUnit : MonoBehaviour,IDamageable
{
    public float Damage =>Managers.Player.data.damage.TotalValue;
    private bool isDead;
    public void Init()
    {
        var hpBar = GetComponentInChildren<UI_PlayerHPBar>();
        hpBar.Init();
        hpBar.SetMaxHP(Managers.Player.data.maxHp.TotalValue);
        
        isDead = false;
        Debug.Log("플레이어 데이터 동기화 완료");
    }
    
    
    public void TakeDamage(float damage)
    {
        ShowBloodEffect();
        Managers.UI.ShowFloatingText(transform.position,$"-{damage}",Color.red,false);
        Managers.Player.TakeDamage(damage);
    }
    

    private void ShowBloodEffect()
    {
        var blood = Managers.Resource.Instantiate(Address.UI_Blood);
        var image = blood.GetComponentInChildren<Image>();
    
        float duration = 0.5f;
        float startAlpha = image.color.a;

        // 1. 투명도를 0으로 만드는 트윈 생성
        image.DOFade(0, duration)
            .From(startAlpha) // 시작 알파값 설정
            .SetEase(Ease.Linear) // 선형적으로 변화 (기존 Lerp와 동일)
            .OnComplete(() => 
            {
                // 2. 완료 후 처리
                // 다음 사용을 위해 알파값 복구 후 파괴(풀링 반납)
                image.color = new Color(1, 1, 1, startAlpha);
                Managers.Resource.Destroy(blood);
            });
    }


}
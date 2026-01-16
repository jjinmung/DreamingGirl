using UnityEngine;

public class PlayerUnit : BaseUnit
{
    public override void Init()
    {
        // 1. 매니저에 저장된 영구 데이터를 BaseUnit의 변수에 동기화
        maxHealth = Managers.Player.data.maxHp.TotalValue;
        currentHealth = Managers.Player.data.currentHp;
        damage = Managers.Player.data.attackPower.TotalValue;
        
        isDead = false;
        
        Debug.Log("플레이어 데이터 동기화 완료");
    }
    
    
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        
        // 데이터 갱신
        Managers.Player.TakeDamage(damage);
    }
}
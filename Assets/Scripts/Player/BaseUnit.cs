using UnityEngine;

public abstract class BaseUnit : MonoBehaviour
{
    [Header("Unit Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;
    public float damage = 20f;
    public virtual void Init()
    {
        currentHealth = maxHealth;
    }

    // 데미지를 입는 공통 로직
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name}이(가) {damage}의 데미지를 입었습니다. 남은 체력: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    
    protected virtual void Die()
    {
        isDead = true;
    }
    
    

}
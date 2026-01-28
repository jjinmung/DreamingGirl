// 액티브 스킬을 위한 추상 클래스 추가
public class ActiveAbilityEffect : AbilityEffect
{
    // 애니메이터에서 재생할 상태 이름
    public string AnimationName { get; private set;}

    public float Cooldown { get; private set;}

    public float Damage { get; private set;}

    public void SetStat(float damage, float cooldown, string animationName)
    {
        Damage = damage;
        Cooldown = cooldown;
        AnimationName = animationName;
        
    }

    // 실제 스킬 로직 (데미지 계산, 투사체 생성 등)
    public virtual void Execute(){}
}
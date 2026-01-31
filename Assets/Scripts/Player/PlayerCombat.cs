using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public enum AttackType { Normal, Ice, Fire }
    public AttackType currentAttackType = AttackType.Fire;

    [SerializeField] private ParticleSystem[] swordTrails;
    [SerializeField] private Transform[] attackEffectPos;

    private Queue<(string type, float time)> inputBuffer = new Queue<(string type, float time)>();
    private float bufferTimeout = 0.5f;
    private Animator _animator;
    public ActiveAbilityEffect CurrentActiveEffect { get; set; }
    public bool CanAttack { get; set; } = true;
    public int ComboIndex { get; private set; } = 0;

    private void Awake() => _animator = GetComponent<Animator>();

    public void AddBuffer(string type) => inputBuffer.Enqueue((type, Time.time));

    public void ProcessBuffer()
    {
        // 1. 큐가 비어있거나 공격 불가능하면 리턴
        if (!CanAttack || inputBuffer.Count == 0) return;
        
        // 2. 가장 오래된 입력을 확인 (꺼내지 않고 확인만)
        var input = inputBuffer.Peek();
       
        // 3. 버퍼 유효 시간 만료 체크
        if (Time.time - input.time > bufferTimeout)
        {
            inputBuffer.Dequeue(); // 만료된 것만 제거
            return;
        }

        // 4. 여기서 실제로 데이터를 하나 꺼냄 (한 번만 호출!)
        inputBuffer.Dequeue();

        // 5. 꺼낸 데이터의 타입에 따라 동작 수행
        if (input.type.StartsWith("Skill_"))
        {
            string skillName = input.type.Replace("Skill_", "");
            PerformSkill(skillName);
        }
        else if (input.type == "Attack")
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        LookAtMouse();
        CanAttack = false;
        _animator.SetTrigger($"ATTACK{ComboIndex + 1}");
        if (ComboIndex == 2)
            ComboIndex--;
        else
            ComboIndex++;
    }
    
    private void PerformSkill(string animName)
    {
        LookAtMouse();
        CanAttack = false;
        _animator.SetTrigger(animName);

       
        ResetCombo();
    }
    //애니메이션 이벤트 함수
    public void ActiveSkillExecute()
    {
        CurrentActiveEffect.Execute();
    }
    
    public void ResetCombo() => ComboIndex = 0;
    public void ClearBuffer() => inputBuffer.Clear();

    // 애니메이션 이벤트에서 호출
    public void PlayAttackEffect(int index)
    {
        var trail = swordTrails[(int)currentAttackType];
        trail.transform.position = attackEffectPos[index].position;
        trail.transform.rotation = attackEffectPos[index].rotation;
        trail.Play();
    }
    
    private void LookAtMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        if (float.IsNaN(mousePos.x) || float.IsNaN(mousePos.y))
            return;

        // 화면 해상도 범위를 완전히 벗어난 값인지 체크
        if (mousePos.x < 0 || mousePos.x > Screen.width || mousePos.y < 0 || mousePos.y > Screen.height)
        {
            // 화면 밖이면 무시하거나 직전의 정상적인 위치를 사용
            return;
        }
        
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (new Plane(Vector3.up, transform.position).Raycast(ray, out float dist))
        {
            Vector3 dir = (ray.GetPoint(dist) - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
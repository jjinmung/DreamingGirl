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

    public bool CanAttack { get; set; } = true;
    public int ComboIndex { get; private set; } = 0;

    private void Awake() => _animator = GetComponent<Animator>();

    public void AddBuffer(string type) => inputBuffer.Enqueue((type, Time.time));

    public void ProcessBuffer()
    {
        if (!CanAttack || inputBuffer.Count == 0) return;

        var input = inputBuffer.Dequeue();
        if (Time.time - input.time <= bufferTimeout)
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
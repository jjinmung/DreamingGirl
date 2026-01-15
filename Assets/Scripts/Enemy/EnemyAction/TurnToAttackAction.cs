using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;


[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TurnToAttack", story: "[Agent] TurnTo [Target]", category: "Action", id: "4c161d3117e79d2385710c5b778e3ce1")]
public partial class TurnToAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    private float rotationSpeed = 20f;
    
    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Target.Value == null || Agent.Value == null)
            return Status.Failure;

        // 1. 타겟으로 향하는 방향 벡터 계산
        Vector3 direction = Target.Value.transform.position - Agent.Value.transform.position;
        direction.y = 0; // 높이 차이 무시
        

        // 2. 목표 회전값 계산
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // 3. 부드러운 회전 적용
        Agent.Value.transform.rotation = Quaternion.Slerp(
            Agent.Value.transform.rotation, 
            targetRotation, 
            rotationSpeed * Time.deltaTime
        );

        // 4. [핵심] 현재 회전과 목표 회전 사이의 각도 차이 계산
        float angleDiff = Quaternion.Angle(Agent.Value.transform.rotation, targetRotation);

        // 각도 차이가 1도 이내라면 완료로 간주
        if (angleDiff < 1.0f)
        {
            // 정확히 목표 방향을 바라보도록 최종 보정
            Agent.Value.transform.rotation = targetRotation;
            //공경 범위 활성화
            SetAttackRange();
            return Status.Success;
        }

        // 아직 도는 중이라면 계속 Running 상태 유지
        return Status.Running;
    }

    protected override void OnEnd()
    {
        
    }

    void SetAttackRange()
    {
        var enemy = Agent.Value.GetComponent<EnemyBase>();
        if (enemy is FishGuardS fishGuardS)
        {
            fishGuardS.SetAttackArange(true);
            fishGuardS.IsAttack = true;
        }
    }
}


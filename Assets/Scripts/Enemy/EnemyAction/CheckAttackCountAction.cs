using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckAttackCount", story: "[Agent] [Attack] [Count]", category: "Action", id: "d44257cfba9de1140be59eec52892636")]
public partial class CheckAttackCountAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<bool> Attack;
    [SerializeReference] public BlackboardVariable<int> Count;
    private EnemyBase enemy;
    protected override Status OnStart()
    {
        if (Count.Value == 2)
        {
            Count.Value = 0;
            Attack.Value = false;
            if (enemy == null)
                enemy = Agent.Value.GetComponent<EnemyBase>();
            enemy.IsAttack = false;
            return Status.Failure;
        }
        Count.Value++;
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


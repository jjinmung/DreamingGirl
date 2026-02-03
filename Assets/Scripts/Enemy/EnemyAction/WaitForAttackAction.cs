using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitForAttack", story: "Wait Unitl [IsAttack] = false", category: "Action", id: "2146b7b575439c3651dd3d1d499c9cbb")]
public partial class WaitForAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> IsAttack;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (IsAttack)
            return Status.Running;
        
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


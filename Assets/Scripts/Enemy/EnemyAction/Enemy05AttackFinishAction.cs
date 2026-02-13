using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Enemy05AttackFinish", story: "[Agent] AttackFinsihed", category: "Action", id: "aa4c58abdc2a62454ee3ec8d64c556cd")]
public partial class Enemy05AttackFinishAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    private Enemy05 enemy05;
    protected override Status OnStart()
    {
        if (enemy05 == null)
            enemy05 = Agent.Value.GetComponent<Enemy05>();
        enemy05.AttackFinished();
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


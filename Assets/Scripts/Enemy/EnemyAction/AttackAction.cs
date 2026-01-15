using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Rendering;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Attack", story: "[Agent] attacks Target", category: "Action", id: "30fe3d4d36fa7b1b6781ed3e7d1c8de6")]
public partial class AttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    private EnemyBase enemy;
    protected override Status OnStart()
    {
        enemy = Agent.Value.GetComponent<EnemyBase>();
        
        if(enemy!=null)
            enemy.Attack();
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


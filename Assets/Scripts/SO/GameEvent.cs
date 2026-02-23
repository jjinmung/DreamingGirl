using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    private Action onEventRaised;

    public void Raise()
    {
        int count = (onEventRaised != null) ? onEventRaised.GetInvocationList().Length : 0;
        Debug.Log($"[GameEvent] {this.name} 발생 시도! 등록된 리스너 수: {count}");
        onEventRaised?.Invoke();
    }

    // 컴포넌트용 등록 메서드
    public void RegisterAction(Action action)
    {
        onEventRaised += action;
    }

    public void UnregisterAction(Action action)
    {
        onEventRaised -= action;
    }
}
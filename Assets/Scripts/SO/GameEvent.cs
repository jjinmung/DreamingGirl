using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    // 이 이벤트를 구독 중인 리스너들의 목록
    private List<GameEventListener> listeners = new List<GameEventListener>();

    // 이벤트 발생! (방송 송출)
    public void Raise()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
            listeners[i].OnEventRaised();
    }

    public void RegisterListener(GameEventListener listener)
    { listeners.Add(listener); }

    public void UnregisterListener(GameEventListener listener)
    { listeners.Remove(listener); }
}
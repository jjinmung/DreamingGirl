using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    public string eventName;
    public UnityEvent Response; // 이벤트 터지면 실행할 함수 (인스펙터에서 연결)

    private void OnEnable() => Managers.Dialogue.Subscribe(eventName, OnEventRaised);
    private void OnDisable() => Managers.Dialogue.Unubscribe(eventName, OnEventRaised);

    public void OnEventRaised() => Response.Invoke();
}
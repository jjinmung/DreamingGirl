using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    public GameEvent Event;      // 어떤 이벤트를 감시할지 (파일 드래그)
    public UnityEvent Response; // 이벤트 터지면 실행할 함수 (인스펙터에서 연결)

    private void OnEnable() => Event.RegisterListener(this);
    private void OnDisable() => Event.UnregisterListener(this);

    public void OnEventRaised() => Response.Invoke();
}
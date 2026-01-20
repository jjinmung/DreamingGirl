using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable _currentInteractable; // 현재 범위 안에 있는 상호작용 대상

    private void Start()
    {
        // 인풋 매니저에 F키 이벤트 연결
        Managers.Input.OnInteract += HandleInteractInput;
         
    }

    private void HandleInteractInput()
    {
        // 범위 안에 있을 때만 실행
        if (_currentInteractable != null)
        {
            _currentInteractable.OnInteract();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 들어온 물체가 IInteractable을 가지고 있는지 확인
        if (other.TryGetComponent(out IInteractable interactable))
        {
            _currentInteractable = interactable;
            Debug.Log("상호작용 가능: " + other.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 나가면 대상을 비움
        if (other.TryGetComponent(out IInteractable interactable))
        {
            if (_currentInteractable == interactable)
            {
                _currentInteractable = null;
                Debug.Log("상호작용 범위 벗어남");
            }
        }
    }
}

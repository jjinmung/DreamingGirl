public interface IInteractable
{
    bool IsInteractable { get; }
    void OnInteract(); // F키를 눌렀을 때 실행될 함수
}
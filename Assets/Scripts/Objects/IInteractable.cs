using System.Threading.Tasks;

public interface IInteractable
{
    bool IsInteractable { get; }
    Task OnInteract(); // F키를 눌렀을 때 실행될 함수
}
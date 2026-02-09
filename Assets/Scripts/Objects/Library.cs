using System.Threading.Tasks;
using UnityEngine;

public class Library:MonoBehaviour,IInteractable
{
    public bool IsInteractable => canInteract;
    private bool canInteract=true;

    public async Task OnInteract()
    {
        await Managers.UI.ShowPopupUI<UI_PassiveSkill>();
    }
}
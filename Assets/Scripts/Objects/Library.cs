using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Library:MonoBehaviour,IInteractable
{
    public bool IsInteractable => canInteract;
    private bool canInteract=true;

    public async void OnInteract()
    {
        await Managers.UI.ShowPopupUI<UI_PassiveSkill>();
    }
}
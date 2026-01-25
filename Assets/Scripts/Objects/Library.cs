using UnityEngine;

public class Library:MonoBehaviour,IInteractable
{
    public void OnInteract()
    {
        Managers.UI.ShowPopupUI<UI_PassiveSkill>();
    }
}
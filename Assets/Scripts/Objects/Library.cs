using UnityEngine;

public class Library:MonoBehaviour,IInteractable
{
    private bool IsInteracting = false;
    private UI_PassiveSkill ui;
    public void OnInteract()
    {
        if (!IsInteracting)
        {
            Managers.Player.PlayerControl.InputActive(false);
            ui =Managers.UI.ShowPopupUI<UI_PassiveSkill>();
            
        }
        else
        {
            ui.Close();
        }

        IsInteracting = !IsInteracting;
    }
}
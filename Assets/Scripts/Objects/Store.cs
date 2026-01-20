using System;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class Store : MonoBehaviour,IInteractable
{
    public Transform NPC;
    bool IsInteracting = false;
    Vector3 defalutRoation = new Vector3(0,90,0);
    Vector3 InteractionRotation = new Vector3(0,60,0);
    public void OnInteract()
    {
        Managers.Camera.SetStoreCam(!IsInteracting);
        Managers.Player.playerTrans.GetComponent<PlayerController>().enabled = IsInteracting;
        //Managers.UI.ShowPopupUI<>()
        if (!IsInteracting)
        {
            NPC.GetComponent<Animator>().SetTrigger("Interacting");
            NPC.DORotate(InteractionRotation, 1f);
        }
        else
        {
            NPC.GetComponent<Animator>().SetTrigger("Interacting");
            NPC.DORotate(defalutRoation, 1f);
        }
        IsInteracting = !IsInteracting;
    }
}   

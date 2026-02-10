using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class Store : MonoBehaviour,IInteractable
{
    public Transform NPC;
    bool IsInteracting = false;
    Vector3 defalutRoation = new Vector3(0,90,0);
    Vector3 InteractionRotation = new Vector3(0,60,0);
    public bool IsInteractable => true;
    public void OnInteract()
    {
        Managers.Camera.SetStoreCam(!IsInteracting);
        Managers.Player.Control.InputActive(IsInteracting);
        //Managers.UI.ShowPopupUI<>()
        NPC.GetComponent<Animator>().SetTrigger("Interacting");
        NPC.DORotate(InteractionRotation, 1f);
        IsInteracting = !IsInteracting;
    }
}   

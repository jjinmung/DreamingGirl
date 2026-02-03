using System;
using UnityEngine;

public class LobyPlayer : MonoBehaviour
{
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetFloat("MOVE",0.5f);
    }
}

using System;
using UnityEngine;

public class TutorialArrow : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Player"))
            gameObject.SetActive(false);
    }
}

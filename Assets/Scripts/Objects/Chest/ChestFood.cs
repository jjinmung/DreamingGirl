using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ChestFood :ChestBase
{
    [SerializeField]Material foodMaterial;
    public override void OnEvent()
    {
        base.OnEvent();
        StartCoroutine(GetHeal());

    }

    IEnumerator GetHeal()
    {
        yield return new WaitForSeconds(1.5f);
        foodMaterial.DOFade(0,0.5f);
        Managers.Player.Heal(100);
        yield return new WaitForSeconds(0.5f);
        Managers.Stage.ClearRoom();
    }
    
    public override void Init()
    {
        base.Init();
        foodMaterial.color = new  Color(1, 1, 1, 1);
    }
}
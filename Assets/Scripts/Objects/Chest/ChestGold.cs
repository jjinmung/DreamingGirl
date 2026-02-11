using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ChestGold :ChestBase
{
    [SerializeField]Material goldMaterial;
    public override void OnEvent()
    {
        base.OnEvent();
        StartCoroutine(GetGold());
    }

    IEnumerator GetGold()
    {
        yield return new WaitForSeconds(1.5f);
        goldMaterial.DOFade(0,0.5f);
        var amount = Random.Range(40, 60);
        Managers.Stage.AddGold(amount);
        yield return new WaitForSeconds(0.5f);
        Managers.Stage.ClearRoom();
    }

    public override void Init()
    {
        base.Init();
        goldMaterial.color = new  Color(1, 1, 1, 1);
    }
}
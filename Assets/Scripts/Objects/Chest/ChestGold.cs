using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ChestGold :ChestBase
{
    public override void OnEvent()
    {
        base.OnEvent();
        StartCoroutine(GetGold());

    }

    IEnumerator GetGold()
    {
        yield return new WaitForSeconds(1.5f);
        var amoun = Random.Range(40, 60);
        Managers.Stage.AddGold(amoun);
        yield return new WaitForSeconds(0.5f);
        Managers.Stage.ClearRoom();
    }
}
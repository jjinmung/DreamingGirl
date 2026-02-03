using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ChestMimic:ChestBase
{
    public override void OnEvent()
    {
        base.OnEvent();
        StartCoroutine(Attack());

    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(1.5f);
        ChsetLid.DOLocalRotate(Close, 0.5f).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(0.5f);
        Managers.Player.PlayerUnit.TakeDamage(25f);
        ChsetLid.DOLocalRotate(Open, 0.5f).SetEase(Ease.InOutQuad);
        
        yield return new WaitForSeconds(0.5f);
        Managers.Stage.ClearRoom();
    }
}
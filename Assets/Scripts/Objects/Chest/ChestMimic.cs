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
        ChsetLid.DOLocalRotate(Close, 0.25f).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(0.25f);
        Managers.Player.Unit.TakeDamage(25f);
        ChsetLid.DOLocalRotate(Open, 0.25f).SetEase(Ease.InOutQuad);
        
        yield return new WaitForSeconds(0.5f);
        Managers.Stage.ClearRoom();
    }
}
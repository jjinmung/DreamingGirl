using System.Collections;
using UnityEngine;

public class ChestFood :ChestBase
{
    public override void OnEvent()
    {
        base.OnEvent();
        StartCoroutine(GetHeal());

    }

    IEnumerator GetHeal()
    {
        yield return new WaitForSeconds(1.5f);
        Managers.Player.Heal(100);
        yield return new WaitForSeconds(0.5f);
        Managers.Stage.ClearRoom();
    }
}
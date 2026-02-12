using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class UI_Popup : UI_Base
{
    public override void Init()
    {
        Managers.UI.SetCanvas(gameObject, true);
    }

    public virtual void ClosePopupUI()
    {
        Managers.UI.ClosePopupUI(this);
    }

    protected virtual void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(1.0f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
    }
}

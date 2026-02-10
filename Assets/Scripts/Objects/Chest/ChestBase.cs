using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class ChestBase:MonoBehaviour,IInteractable
{
    [SerializeField]protected Transform ChsetLid;
    protected Vector3 Open =new Vector3(-120,0,0);
    protected Vector3 Close = new Vector3(0,0,0);
    public bool IsInteractable => _canInteract;
    public bool _canInteract;
    public virtual void OnEvent()
    {
        ChsetLid.DOLocalRotate(Open,1f).SetEase(Ease.InOutQuad);
    }
    public void OnInteract()
    {
        if (_canInteract)
        {
            _canInteract = false;
            OnEvent();
        }
            
    }

    public virtual void Init()
    {
        _canInteract = true;
        ChsetLid.localEulerAngles = Close;
    }
}
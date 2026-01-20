using System;
using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스 추가

public class Door : MonoBehaviour
{
    [SerializeField] private Transform door;
    Vector3 close = new Vector3(0, 0, 0);
    Vector3 ExitRoomVec = new Vector3(0, -90f, 0);
    Vector3 EnterRoomVec = new Vector3(0, 90f, 0);
    private BoxCollider doorCollider;

    public bool IsEnterDoor=false;
    public Transform ExitPos;
    public Transform EnterPos;

    public Vector3 dir;
    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        door.localEulerAngles = close;
        doorCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Managers.Stage.OnExitRoom(this);
        }
    }

    public void ExitRoomOpen()
    {
        doorCollider.enabled = true;
        door.DOLocalRotate(ExitRoomVec, 1f).SetEase(Ease.InOutQuad);
    }
    public void EnterRoomOpen()
    {
        door.DOLocalRotate(EnterRoomVec, 1f).SetEase(Ease.InOutQuad);
    }

    public void Close()
    {
        door.DOLocalRotate(close, 1f).SetEase(Ease.InOutQuad);
    }
}
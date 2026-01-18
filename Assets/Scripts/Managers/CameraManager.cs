using UnityEngine;
using System;
using DG.Tweening;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{

    private CinemachineCamera _thirdPersonCam;
    private CinemachineCamera _quarterViewCam;
    private CinemachineCamera _middleViewCam;
    public Action OnSwitchedToThirdPerson;
    public Action OnSwitchedToQuarterView;

    private Camera _mainCam;
    private int _ceilingLayer;

    private bool _isQuarterView;
    
    private Transform _player;
    public void Init()
    {
        _mainCam = Camera.main;
        _ceilingLayer = LayerMask.NameToLayer("Ceiling"); 
        
        OnSwitchedToThirdPerson -= HandleCullingMaskForThirdPerson;
        OnSwitchedToThirdPerson += HandleCullingMaskForThirdPerson;

        OnSwitchedToQuarterView -= HandleCullingMaskForQuarterView;
        OnSwitchedToQuarterView += HandleCullingMaskForQuarterView;

        Managers.Input.OnChangeCamera -= ChanageCamera;
        Managers.Input.OnChangeCamera += ChanageCamera;
        
        _isQuarterView = true;
        
        _player= GameObject.FindGameObjectWithTag("Player").transform;
        _middleViewCam =_player.GetComponentInChildren<CinemachineCamera>();
        RefreshCameras();
    }

    private void HandleCullingMaskForThirdPerson()
    {
        _player.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 2f);
    }

    private void HandleCullingMaskForQuarterView()
    {
        _player.DOScale(new Vector3(2f, 2f, 2f), 2f);
    }
    public void RefreshCameras()
    {
        var parent =GameObject.Find("Cameras");
        var qvObj = parent.transform.GetChild(0);
        var tpObj = parent.transform.GetChild(1);

        if (tpObj != null) _thirdPersonCam = tpObj.GetComponent<CinemachineCamera>();
        if (qvObj != null) _quarterViewCam = qvObj.GetComponent<CinemachineCamera>();
        
    }

    public void ChanageCamera()
    {
        if (_thirdPersonCam == null || _quarterViewCam == null) RefreshCameras();
        if (_mainCam == null) _mainCam = Camera.main;
        
        // 목표 카메라 설정
        var targetCam = _isQuarterView ? _thirdPersonCam : _quarterViewCam;

        // 이벤트 호출
        if (_isQuarterView) OnSwitchedToThirdPerson?.Invoke();
        else OnSwitchedToQuarterView?.Invoke();
        
        // DOTween 시퀀스 생성
        Sequence camSeq = DOTween.Sequence();

        camSeq.AppendCallback(() => {
            ResetAllPriorities();
            _middleViewCam.Priority = 30; // 1단계: 중간 카메라로
        });

        camSeq.AppendInterval(1f); // 2단계: 중간에서 대기 (혹은 블렌딩 시간)

        camSeq.AppendCallback(() => {
            if(_isQuarterView)
                _mainCam.cullingMask |= 1 << _ceilingLayer;// 3인칭: 천장 레이어를 켠다 (OR 연산)
            else
                _mainCam.cullingMask &= ~(1 << _ceilingLayer);// 쿼터뷰: 천장 레이어를 끈다 (AND NOT 연산)
            
            targetCam.Priority = 40; // 3단계: 최종 카메라로
            _isQuarterView = !_isQuarterView;
        });
    }

    void ResetAllPriorities()
    {
        _thirdPersonCam.Priority = 10;
        _quarterViewCam.Priority = 10;
        _middleViewCam.Priority = 10;
    }

    public void SetTarget(Transform target)
    {
        _thirdPersonCam.Target.TrackingTarget= target;
        _quarterViewCam.Target.TrackingTarget= target;
        _middleViewCam.Target.TrackingTarget = target;
    }
}
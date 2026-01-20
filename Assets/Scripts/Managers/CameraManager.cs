using UnityEngine;
using System;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    //로비씬 카메라
    private CinemachineCamera _defalutCam;
    private CinemachineCamera _startCam;
    

    //전투씬 카메라
    private CinemachineCamera _thirdPersonCam;
    private CinemachineCamera _quarterViewCam;
    private CinemachineCamera _middleViewCam;
    private CinemachineCamera _StoreCam;
    
    public Action OnSwitchedToThirdPerson;
    public Action OnSwitchedToQuarterView;

    private Camera _mainCam;
    private int _ceilingLayer;

    private bool _isQuarterView;
    
    private Transform _player;


    #region 로비씬 함수

    public void LobyInit()
    {
        var parent =GameObject.Find("LobyCam");
        var defaultcam = parent.transform.GetChild(0);
        var startcam = parent.transform.GetChild(1);

        if (defaultcam != null) _defalutCam = defaultcam.GetComponent<CinemachineCamera>();
        if (startcam != null) _startCam = startcam.GetComponent<CinemachineCamera>();
        _defalutCam.Priority = 10;
        _startCam.Priority = 0;
        
    }

    public void LobyToBattle()
    {
        Sequence camSeq = DOTween.Sequence();
        camSeq.AppendInterval(1f);
        
        camSeq.AppendCallback(() => {
            _defalutCam.Priority = 5;
            _startCam.Priority = 10;
        });

        camSeq.AppendInterval(3f); 

        camSeq.AppendCallback(() => {
            SceneManager.sceneLoaded += OnBattleSceneLoaded;
            SceneManager.LoadScene("BattleScene");
        });
    }

    private void OnBattleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "BattleScene")
        {
            var go = Managers.Player.CreatePlayer();
            _player= go.transform;
            Managers.Camera.BattleInit();
            Managers.Camera.SetTarget(_player);
            Managers.Stage.Init();
            SceneManager.sceneLoaded -= OnBattleSceneLoaded;
        }
    }
    #endregion
    
        
    

    #region 전투씬 함수
    public void BattleInit()
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
        _middleViewCam =_player.GetComponentInChildren<CinemachineCamera>();
        RefreshCameras();
        
    }

    private void HandleCullingMaskForThirdPerson()
    {
        _player.DOScale(new Vector3(1f, 1f, 1f), 2f);
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
        var storeObj = parent.transform.GetChild(2);
        
        if (qvObj != null) _quarterViewCam = qvObj.GetComponent<CinemachineCamera>();
        if (tpObj != null) _thirdPersonCam = tpObj.GetComponent<CinemachineCamera>();
        if (storeObj != null) _StoreCam = storeObj.GetComponent<CinemachineCamera>();
        
        
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

        camSeq.AppendInterval(0.9f); // 2단계: 중간에서 대기 (혹은 블렌딩 시간)

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
        _StoreCam.Priority = 10;
    }

    public void SetTarget(Transform target)
    {
        _thirdPersonCam.Target.TrackingTarget= target;
        _quarterViewCam.Target.TrackingTarget= target;
        _middleViewCam.Target.TrackingTarget = target;
    }

    public void SetStoreCam(bool isStore)
    {
        if (isStore)
        {
            ResetAllPriorities();
            _StoreCam.Priority = 20;
        }
        else
        {
            ResetAllPriorities();
            _quarterViewCam.Priority = 20;
        }
    }

    #endregion
    
}
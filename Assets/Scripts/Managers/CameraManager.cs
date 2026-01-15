using UnityEngine;
using System;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    // Managers 클래스에서 사용한다면 싱글톤 중복은 필요 없으나, 단독 사용 시 유지
    public static CameraManager Instance { get; private set; }

    private CinemachineVirtualCamera _thirdPersonCam;
    private CinemachineVirtualCamera _quarterViewCam;

    public Action OnSwitchedToThirdPerson;
    public Action OnSwitchedToQuarterView;

    private Camera _mainCam;
    private int _ceilingLayer;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        // DontDestroyOnLoad는 Managers에서 처리한다고 가정
        Init();
    }
    
    public void Init()
    {
        _mainCam = Camera.main;
        _ceilingLayer = LayerMask.NameToLayer("Ceiling"); 
        
        OnSwitchedToThirdPerson -= HandleCullingMaskForThirdPerson;
        OnSwitchedToThirdPerson += HandleCullingMaskForThirdPerson;

        OnSwitchedToQuarterView -= HandleCullingMaskForQuarterView;
        OnSwitchedToQuarterView += HandleCullingMaskForQuarterView;
    }

    private void HandleCullingMaskForThirdPerson()
    {
        if (_mainCam == null) _mainCam = Camera.main;
        
        // 3인칭: 천장 레이어를 켠다 (OR 연산)
        _mainCam.cullingMask |= (1 << _ceilingLayer);
    }

    private void HandleCullingMaskForQuarterView()
    {
        if (_mainCam == null) _mainCam = Camera.main;

        // 쿼터뷰: 천장 레이어를 끈다 (AND NOT 연산)
        _mainCam.cullingMask &= ~(1 << _ceilingLayer);
    }
    public void RefreshCameras()
    {
        // 방법 1: 이름으로 찾기
        GameObject tpObj = GameObject.Find("ThirdPersonCam");
        GameObject qvObj = GameObject.Find("QuarterViewCam");

        if (tpObj != null) _thirdPersonCam = tpObj.GetComponent<CinemachineVirtualCamera>();
        if (qvObj != null) _quarterViewCam = qvObj.GetComponent<CinemachineVirtualCamera>();
        
    }

    public void ChangeToThirdPerson()
    {
        if (_thirdPersonCam == null) RefreshCameras(); // 카메라가 없으면 새로 고침

        _thirdPersonCam.Priority = 20;
        _quarterViewCam.Priority = 10;
        OnSwitchedToThirdPerson?.Invoke();
    }

    public void ChangeToQuarterView()
    {
        if (_quarterViewCam == null) RefreshCameras();

        _thirdPersonCam.Priority = 10;
        _quarterViewCam.Priority = 20;
        OnSwitchedToQuarterView?.Invoke();
    }
}
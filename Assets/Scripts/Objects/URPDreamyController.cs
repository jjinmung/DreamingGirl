using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URPWateryDream : MonoBehaviour
{
    private Volume _volume;
    private LensDistortion _lensDistortion;
    private ChromaticAberration _chromatic; // 색수차 추가로 몽롱함 극대화

    [Header("물결 설정")]
    public float speed = 3f;       // 물결치는 속도

    void Start()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _lensDistortion);
    }

    void Update()
    {
        if (_lensDistortion == null) return;
        
        _lensDistortion.xMultiplier.value = 0.5f + (Mathf.Cos(Time.time * speed)*0.5f);
        _lensDistortion.yMultiplier.value = 0.5f + (Mathf.Sin(Time.time * speed) *0.5f);

    }
}
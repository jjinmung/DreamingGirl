using UnityEngine;

public class ParticleHandler : MonoBehaviour
{
    // 파티클이 완전히 멈췄을 때 자동으로 호출됩니다.
    void OnParticleSystemStopped()
    {
        Managers.Resource.Destroy(gameObject);
    }
}
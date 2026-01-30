using System.Collections;
using UnityEngine;

namespace PixPlays.ElementalVFX
{
    public class BeamVfx : BaseVfx
    {
        [Header("Effect References")]
        [SerializeField] private ParticleSystem _BeamBodyEffect;
        [SerializeField] private ParticleSystem _CastEffect;
        [SerializeField] private ParticleSystem _HitEffect;
        [SerializeField] private ParticleSystem _BodyTip;

        [Header("Settings")]
        [SerializeField] private float _ScaleSpeed = 50f; // 빔이 뻗어나가는 속도
        [SerializeField] private float _MaxDistance = 100f;
        [SerializeField] private LayerMask _hitLayer;

        private float _currentLength;
        private bool _isFiring;
        private Vector3 _currentHitPoint;
        private float damage;
        private float attackDelay =0.2f;
        private float LastAttackTime =-999f;
        public void Play(float duration,float damage =10f)
        {
            _isFiring = true;
            _currentLength = 0;
           
            this.damage = damage;
            Debug.Log(this.damage);
            StopAllCoroutines();
            StartCoroutine(Coroutine_BeamLogic());
            Invoke(nameof(Stop),duration);
        }
        public override void Play()
        {
            _isFiring = true;
            _currentLength = 0;
            this.damage = damage;
            StopAllCoroutines();
            StartCoroutine(Coroutine_BeamLogic());
            
        }

        // VfxData를 받는 버전도 호환성을 위해 유지하되, 내부 로직은 동일하게 처리 가능
        public override void Play(VfxData data)
        {
            _data = data; // 필요 시 데이터 저장
            Play();
        }

        public override void Stop()
        {
            base.Stop();
            _isFiring = false;
            _BeamBodyEffect.Stop();
            _CastEffect.Stop();
            _HitEffect.Stop();
            _BodyTip.Stop();
            
            _BeamBodyEffect.gameObject.SetActive(false);
            _CastEffect.gameObject.SetActive(false);
            _HitEffect.gameObject.SetActive(false);
            _BodyTip.gameObject.SetActive(false);
            _HitEffect.gameObject.SetActive(false);
        }

        private IEnumerator Coroutine_BeamLogic()
        {
            SetupEffects(true);

            Vector3 startScale = _BeamBodyEffect.transform.localScale;

            while (_isFiring)
            {
                // 1. 레이캐스트: 내 위치에서 정면(forward)으로 발사
                UpdateRaycast();

                // 2. 수평 거리 계산 (Y값 무시)
                Vector3 sourcePos = transform.position;
                Vector3 targetPos = _currentHitPoint;
                targetPos.y = sourcePos.y; 

                float targetLength = Vector3.Distance(sourcePos, targetPos);

                // 3. 빔이 한 번에 팍 생기는 게 아니라 _ScaleSpeed에 따라 서서히 늘어남
                if (_currentLength < _currentLength)
                {
                    _currentLength = Mathf.MoveTowards(_currentLength, targetLength, _ScaleSpeed * Time.deltaTime);
                }
                else
                {
                    _currentLength = targetLength;
                }

                // 4. 트랜스폼 업데이트
                UpdateBeamTransform(startScale);

                // 5. Hit 이펙트 처리
                UpdateHitEffect(targetLength);

                yield return null;
            }
            
            SetupEffects(false);
        }

        [SerializeField] private float _beamRadius = 0.5f; // 빔의 반지름 (너비의 절반)

        private void UpdateRaycast()
        {
            // SphereCast: 시작점, 반지름, 방향, 결과, 최대거리, 레이어
            // transform.position에서 정면으로 _beamRadius 두께의 구체를 발사합니다.
            if (Physics.SphereCast(transform.position, _beamRadius, transform.forward, out RaycastHit hit, _MaxDistance, _hitLayer))
            {
                _currentHitPoint = hit.point;

                if (hit.collider.CompareTag("Player"))
                {
                    if (Time.time > attackDelay + LastAttackTime)
                    {
                        LastAttackTime = Time.time;
                
                        // SphereCast는 충돌 지점뿐만 아니라 충돌한 물체 자체를 가져오기 쉽습니다.
                        if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                        {
                            damageable.TakeDamage(damage);
                        }
                    }
                }
            }
            else
            {
                // 아무것도 안 맞으면 최대 거리 지점
                _currentHitPoint = transform.position + (transform.forward * _MaxDistance);
            }
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            // 빔의 시작과 끝을 원기둥 형태로 가늠해볼 수 있게 그립니다.
            Gizmos.DrawWireSphere(transform.position, _beamRadius);
            Vector3 endPos = transform.position + transform.forward * _MaxDistance;
            Gizmos.DrawWireSphere(endPos, _beamRadius);
            Gizmos.DrawLine(transform.position, endPos);
        }
        private void UpdateBeamTransform(Vector3 startScale)
        {
            // 빔의 방향은 항상 현재 오브젝트의 정면(forward)을 따름
            Vector3 direction = transform.forward;

            // 본체
            _BeamBodyEffect.transform.position = transform.position;
            _BeamBodyEffect.transform.forward = direction;
            _BeamBodyEffect.transform.localScale = new Vector3(startScale.x, startScale.y, _currentLength);

            // 입구
            _CastEffect.transform.position = transform.position;
            _CastEffect.transform.forward = direction;

            // 팁(끝부분)
            _BodyTip.transform.position = transform.position + direction * _currentLength;
            _BodyTip.transform.forward = direction;
        }

        private void UpdateHitEffect(float targetLength)
        {
            // 빔이 실제 충돌 지점에 거의 도달했을 때만 Hit 이펙트 활성화
            if (_currentLength >= targetLength - 0.2f)
            {
                if (!_HitEffect.gameObject.activeSelf) _HitEffect.gameObject.SetActive(true);
                
                _HitEffect.transform.position = _currentHitPoint;
                _HitEffect.transform.forward = -transform.forward; // 내 정면의 반대 방향

                if (!_HitEffect.isPlaying) _HitEffect.Play();
            }
            else
            {
                if (_HitEffect.isPlaying) _HitEffect.Stop();
                _HitEffect.gameObject.SetActive(false);
            }
        }

        private void SetupEffects(bool active)
        {
            _CastEffect.gameObject.SetActive(active);
            _BeamBodyEffect.gameObject.SetActive(active);
            _BodyTip.gameObject.SetActive(active);

            if (active)
            {
                _CastEffect.Play();
                _BeamBodyEffect.Play();
                _BodyTip.Play();
            }
        }
    }
}
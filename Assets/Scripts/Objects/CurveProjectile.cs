using System;
using System.Collections;
using UnityEngine;

public class CurveProjectile : MonoBehaviour
{
    public AnimationCurve curve; // 에디터에서 포물선 모양을 그림
    public float heightMultiplier = 5.0f; // 포물선 높이 배율
    
    [SerializeField] ParticleSystem _CastEffect;
    [SerializeField] ParticleSystem _HitEffect;
    [SerializeField] ParticleSystem _ProjectileEffect;
    [SerializeField] Transform _source;
    public float duration = 3f;// 총 이동 시간
    [SerializeField] float _ProjectileFlyDelay;//발사 지연시간
    [SerializeField] float _ProjectileDeactivateDelay;//총알 활성화시간


    public void Launch(Vector3 _target)
    {
        StartCoroutine(Coroutine_Projectile(_target));
    }
    
    
    IEnumerator Coroutine_Projectile(Vector3 _target)
        {
            _CastEffect.gameObject.SetActive(true);
            _CastEffect.transform.position = _source.position;
            _CastEffect.transform.forward = (_target - _source.position);
            _CastEffect.Play();

            yield return new WaitForSeconds(_ProjectileFlyDelay);
            _ProjectileEffect.gameObject.SetActive(true);
            _ProjectileEffect.transform.position = _CastEffect.transform.position;
            _ProjectileEffect.Play();
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration; // 0에서 1까지 진행률

                // 1. 수평 이동 (X, Z)
                Vector3 currentPos = Vector3.Lerp(_source.position, _target, t);

                // 2. 수직 이동 (Y축에 커브 값 적용)
                // curve.Evaluate(t)는 0~1 사이의 시간에 따른 커브의 Y값을 리턴함
                float curveHeight = curve.Evaluate(t) * heightMultiplier;
                currentPos.y += curveHeight;

                // 3. 위치 업데이트 및 회전
                _ProjectileEffect.transform.forward = currentPos - _ProjectileEffect.transform.position; // 진행 방향 바라보기
                _ProjectileEffect.transform.position = currentPos;

                yield return null;
            }
            _ProjectileEffect.transform.position = _target;
            _HitEffect.transform.forward = (_ProjectileEffect.transform.position -  _target);
            _ProjectileEffect.transform.position =  _target;
            _ProjectileEffect.Stop();

            
            _HitEffect.transform.position = _target;
            _HitEffect.gameObject.SetActive(true);
            _HitEffect.Play();

            yield return new WaitForSeconds(_ProjectileDeactivateDelay);
            _ProjectileEffect.gameObject.SetActive(false);
        }

        public void Stop()
        {
            if (gameObject != null)
            {
                _HitEffect.Stop();
                _ProjectileEffect.Stop();
                _CastEffect.Stop();
            }
        }
}
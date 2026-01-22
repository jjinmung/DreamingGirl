using System;
using System.Collections;
using CartoonFX;
using UnityEngine;

public class fireTile : MonoBehaviour
{
    [SerializeField] private ParticleSystem _mainFire;
    private BoxCollider _collider;
    private WaitForSeconds _firedurationTime = new WaitForSeconds(2f);
    private WaitForSeconds _firestartTime = new WaitForSeconds(2f);
    private WaitForSeconds _beforeFiretime = new WaitForSeconds(2f);
    public float Damage = 5f;
    private CFXR_Effect effect;
    
    public float DamageInterval = 1.0f; // 데미지 주기 (1초)
    private float _nextDamageTime = 0f;  // 다음 데미지 발생 시간 저
    private void Start()
    {
        _collider = GetComponent<BoxCollider>();
        effect =GetComponentInChildren<CFXR_Effect>();
        playFire();
    }

    private void playFire()
    {
       
        StartCoroutine(StartFire());
    }

    IEnumerator StartFire()
    {
        while (true)
        {
            effect.enabled = false;
            _mainFire.Play();
            _collider.enabled = false;
            
            yield return _beforeFiretime;

            effect.enabled = true;

            
            yield return _firestartTime;
            
            _collider.enabled = true;
            
            yield return _firedurationTime;
            
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_collider.enabled && other.CompareTag("Player"))
        {
            if (other.TryGetComponent<IDamageable>(out var unit))
            {
                unit.TakeDamage(Damage);
                // 들어오자마자 데미지를 줬으니, 다음 데미지는 1초 뒤로 설정
                _nextDamageTime = Time.time + DamageInterval;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_collider.enabled && other.CompareTag("Player"))
        {
            // 현재 시간이 '다음 데미지 시간'보다 커졌는지 확인
            if (Time.time >= _nextDamageTime)
            {
                if (other.TryGetComponent<IDamageable>(out var unit))
                {
                    unit.TakeDamage(Damage);
                
                    // 다음 데미지 시간 갱신 (현재 시간 + 1초)
                    _nextDamageTime = Time.time + DamageInterval;
                }
            }
        }
    }
}

    

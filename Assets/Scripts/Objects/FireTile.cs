using System;
using System.Collections;
using CartoonFX;
using UnityEngine;

public class fireTile : MonoBehaviour
{
    [SerializeField] private ParticleSystem _mainFire;
    [SerializeField] private Light _light;
    private BoxCollider _collider;
    private WaitForSeconds _firedurationTime = new WaitForSeconds(2f);
    private WaitForSeconds _firestartTime = new WaitForSeconds(2f);
    private WaitForSeconds _beforeFiretime = new WaitForSeconds(2f);
    public float Damage = 5f;
    private CFXR_Effect effect;
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
            _light.intensity = 0;
            _mainFire.Play();
            _collider.enabled = false;
            
            yield return _beforeFiretime;

            StartCoroutine(ActiveBeforeFire());
            
            yield return _firestartTime;
            
            _collider.enabled = true;
            
            yield return _firedurationTime;
            
            _collider.enabled = true;
            
        }
    }

    IEnumerator ActiveBeforeFire()
    {
        float intensity = 0;
        _light.intensity = intensity;
        while (intensity<10)
        {
            intensity += 0.5f;
            _light.intensity = intensity;
            yield return null;
        }
        effect.enabled = true;
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<BaseUnit>().TakeDamage(Damage);
        }
    }
}

    

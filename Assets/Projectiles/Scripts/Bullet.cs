using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MasterStylizedProjectile
{
    public class Bullet:MonoBehaviour
    {
        public float Speed = 5;
        public ParticleSystem OnHitEffect;
        public AudioClip bulletClip;
        public AudioClip onHitClip;

        public bool isTargeting;
        public Transform target;
        public float rotSpeed = 0;
        public bool isFlatShoot = false;
        
        private float initialYPosition;
        
        private void Start()
        {
            Init();
        }

        private void OnEnable()
        {
            Init();
        }

        private void Update()
        {
            if (isTargeting  && target != null)
            {
                Vector3 targetDirection = target.position - transform.position;
                
                
                if (isFlatShoot)
                {
                    targetDirection.y = 0;
                    targetDirection = targetDirection.normalized;
                }
                
                transform.forward = Vector3.RotateTowards(transform.forward, targetDirection, rotSpeed * Time.deltaTime, 0.0f);
                
                
                if (isFlatShoot)
                {
                    Vector3 flatForward = transform.forward;
                    flatForward.y = 0;
                    if (flatForward.sqrMagnitude > 0.001f)
                    {
                        flatForward = flatForward.normalized;
                        transform.forward = flatForward;
                    }
                }
            }
            else
            {
                
                if (isFlatShoot)
                {
                    Vector3 flatForward = transform.forward;
                    flatForward.y = 0;
                    if (flatForward.sqrMagnitude > 0.001f)
                    {
                        flatForward = flatForward.normalized;
                        transform.forward = flatForward;
                    }
                }
            }
            
            
            Vector3 forward = Vector3.forward;
            transform.Translate(forward * Speed * Time.deltaTime, Space.Self);
            
            
            if (isFlatShoot)
            {
                Vector3 pos = transform.position;
                transform.position = new Vector3(pos.x, initialYPosition, pos.z);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.CompareTag("Enemy")) return;
            if (OnHitEffect != null)
            {
                var onHitObj = 
                    Managers.Resource.Instantiate(Address.PurpleShoot_Hit, 
                    transform.position, Quaternion.identity);
                
                /*var onHit = onHitObj.gameObject.AddComponent<AudioTrigger>();
                if (onHitClip != null)
                {
                    onHit.onClip = onHitClip;
                }*/
                
            }
            Managers.Resource.Destroy(gameObject);
        }

        private void Init()
        {
            initialYPosition = transform.position.y;
            
            if (bulletClip != null)
            {
                var audio = gameObject.AddComponent<AudioSource>();
                audio.clip = bulletClip;
                audio.Play();
            }
        }
        

    }
}

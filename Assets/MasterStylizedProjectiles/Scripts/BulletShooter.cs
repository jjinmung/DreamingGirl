using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MasterStylizedProjectile
{
    [System.Serializable]
    public class EffectsGroup
    {
        public string EffectName;
        public float Speed = 20;
        public ParticleSystem ChargeParticles;
        public float ChargeParticleTime;
        public AudioClip ChargeClip;
        public ParticleSystem StartParticles;
        public ParticleSystem BulletParticles;
        public ParticleSystem HitParticles;
        public AudioClip startClip;
        public AudioClip bulletClip;
        public AudioClip hitClip;
        public bool isTargeting;
        public float RotSpeed;
        [Tooltip("是否平射，如果为true，子弹在y轴方向的速度为0，且不能朝上下旋转")]
        public bool isFlatShoot = false;
    }
    public class BulletShooter : MonoBehaviour
    {
        //public List<EffectsGroup> Effects = new List<EffectsGroup>();

        public BulletDatas datas;
        public int Index = 0;
        public EffectsGroup CurEffect => datas.Effects[Index];
        public Transform StartNodeTrans;

        public float Speed;
        public float ShootInterval = 0.2f;
        float LastShootTime = 0;
         // Start is called before the first frame update


        public void Shoot()
        {
            DoShoot();
            //StartCoroutine(ShootIE());
        }
        public IEnumerator ShootIE()
        {
            yield return Charge();
            DoShoot();
        }
        public IEnumerator Charge()
        {
            if (CurEffect.ChargeParticles != null)
            {
                //var ChargePar = Managers.Pool.SpawnFromPool(CurEffect.ChargeParticles.gameObject, StartNodeTrans.position, Quaternion.identity);
                //var onStart = gameObject.AddComponent<AudioTrigger>();
                //if (CurEffect.ChargeClip != null)
                //{
                //    onStart.onClip = CurEffect.startClip;
                //}

            
                if (CurEffect.ChargeClip != null)
                {
                    GameObject AudioObj = new GameObject();
                    var audiosource = AudioObj.AddComponent<AudioSource>();
                    audiosource.clip = CurEffect.ChargeClip;
                    audiosource.Play();
                }
                yield return new WaitForSeconds(CurEffect.ChargeParticleTime);
                //Managers.Pool.ReturnToPool(ChargePar.gameObject);
            }
           
        }
        public void DoShoot()
        {
            //var targetPos = GetMouseTargetPos();
            //var targetDir = targetPos - StartNodeTrans.position;
            var targetDir = transform.forward;
            
            if (CurEffect.isFlatShoot)
            {
                targetDir.y = 0;
            }
            
            targetDir = targetDir.normalized;
            
            if (CurEffect.StartParticles != null)
            {
                var StartPar = Managers.Pool.SpawnFromPool(CurEffect.StartParticles.gameObject, StartNodeTrans.position, Quaternion.identity);
                StartPar.transform.forward = targetDir;

                /*var onStart = StartPar.gameObject.AddComponent<AudioTrigger>();
                if (CurEffect.startClip != null)
                {
                    onStart.onClip = CurEffect.startClip;
                }*/

            }
            if (CurEffect.BulletParticles != null)
            {
                var bulletObj = Managers.Pool.SpawnFromPool(CurEffect.BulletParticles.gameObject, StartNodeTrans.position, Quaternion.identity);
                bulletObj.transform.forward = targetDir;

                var bullet = bulletObj.gameObject.GetComponent<Bullet>();
                if(bullet == null)
                    bullet =bulletObj.gameObject.AddComponent<Bullet>();
                //bullet.Speed = CurEffect.Speed;
                bullet.Speed = Speed;
                bullet.isTargeting = CurEffect.isTargeting;
                bullet.isFlatShoot = CurEffect.isFlatShoot;
                bullet.OnHitEffect = CurEffect.HitParticles;
                
                if (CurEffect.isTargeting)
                {
                    var target = FindNearestTarget("Respawn");
                    if (target != null)
                    {
                        bullet.rotSpeed = CurEffect.RotSpeed;
                        bullet.target = target.transform;
                    }
                }

                   
                if (CurEffect.hitClip != null)
                {
                    bullet.onHitClip = CurEffect.hitClip;
                }
                if (CurEffect.bulletClip != null)
                {
                    bullet.bulletClip = CurEffect.bulletClip;
                }


                var collider = bulletObj.gameObject.GetComponent<SphereCollider>();
                if (collider == null)
                {
                    collider =bulletObj.gameObject.AddComponent<SphereCollider>();
                    collider.isTrigger = true;
                    collider.radius = 0.6f;
                }
               
            }
      
        }


        public Vector3 GetMouseTargetPos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray,out hit,100))
            {
                return hit.point;
            }
            return Vector3.zero;
        }

        public GameObject FindNearestTarget(string tag)
        {
            var gameObjects = GameObject.FindGameObjectsWithTag(tag).ToList().OrderBy(
                (x) => Vector3.Distance(transform.position, x.transform.position));
            return gameObjects.FirstOrDefault();
        }
    }

}

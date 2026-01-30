using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MasterStylizedProjectile
{
    [System.Serializable]
    
    public class BulletShooter : MonoBehaviour
    {

        public Transform StartNodeTrans;
        public float Speed;
        float LastShootTime = 0;
        
         // Start is called before the first frame update
         private EnemyBase enemy;
         [SerializeField] private AssetReference startEffect;
         [SerializeField] private AssetReference BulletEffect;
         [SerializeField] private AssetReference HitEffect;
         public float damage
         {
             get
             {
                 if (enemy == null)
                     enemy = GetComponent<EnemyBase>();
                 return enemy.stat.Damage;
             }
         }

        public void Shoot()
        {
            DoShoot();
            //StartCoroutine(ShootIE());
        }
        
        public void DoShoot()
        {
            //var targetPos = GetMouseTargetPos();
            //var targetDir = targetPos - StartNodeTrans.position;
            var targetDir = transform.forward;
            
            targetDir.y = 0;
            targetDir = targetDir.normalized;
            
            if (startEffect != null)
            {
                var StartPar = 
                    Managers.Resource.Instantiate(startEffect, 
                        StartNodeTrans.position, Quaternion.identity);
                StartPar.transform.forward = targetDir;

                /*var onStart = StartPar.gameObject.AddComponent<AudioTrigger>();
                if (CurEffect.startClip != null)
                {
                    onStart.onClip = CurEffect.startClip;
                }*/

            }
            if (BulletEffect != null)
            {
                var bulletObj = 
                    Managers.Resource.Instantiate(BulletEffect, 
                        StartNodeTrans.position, Quaternion.identity);
                bulletObj.transform.forward = targetDir;

                var bullet = bulletObj.gameObject.GetOrAddComponent<Bullet>();
                
                //bullet.Speed = CurEffect.Speed;
                bullet.Speed = Speed;
                bullet.OnHitEffect = HitEffect;
                bullet.Damage = damage;

                   
                /*if (CurEffect.hitClip != null)
                {
                    bullet.onHitClip = CurEffect.hitClip;
                }
                if (CurEffect.bulletClip != null)
                {
                    bullet.bulletClip = CurEffect.bulletClip;
                }*/


                var collider = bulletObj.gameObject.GetOrAddComponent<SphereCollider>();
                collider.isTrigger = true;
               
            }
      
        }
        
    }

}

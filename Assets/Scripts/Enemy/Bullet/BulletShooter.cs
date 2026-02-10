using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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
         // Start is called before the first frame update
         private EnemyBase enemy;
         [SerializeField] private AssetReference startEffect;
         [SerializeField] private AssetReference BulletEffect;
         [SerializeField] private AssetReference HitEffect;
         [SerializeField] private AssetReference hitClip;
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
        
        public async void DoShoot()
        {
            //var targetPos = GetMouseTargetPos();
            //var targetDir = targetPos - StartNodeTrans.position;
            var targetDir = StartNodeTrans.forward;
            
            targetDir.y = 0;
            targetDir = targetDir.normalized;
            
            if (startEffect != null)
            {
                var StartPar =  await 
                    Managers.Resource.InstantiateAsync(startEffect, 
                        StartNodeTrans.position, Quaternion.identity);
                StartPar.transform.forward = targetDir;


                

            }
            if (BulletEffect != null)
            {
                var bulletObj = await 
                    Managers.Resource.InstantiateAsync(BulletEffect, 
                        StartNodeTrans.position, Quaternion.identity);
                bulletObj.transform.forward = targetDir;

                var bullet = bulletObj.gameObject.GetOrAddComponent<Bullet>();
                
                bullet.Speed = Speed;
                bullet.OnHitEffect = HitEffect;
                bullet.Damage = damage;
                
                   
                if (hitClip != null)
                {
                    bullet.HitClipAddress = hitClip.RuntimeKey.ToString();
                }


                var collider = bulletObj.gameObject.GetOrAddComponent<SphereCollider>();
                collider.isTrigger = true;
               
            }
      
        }
        
    }

}

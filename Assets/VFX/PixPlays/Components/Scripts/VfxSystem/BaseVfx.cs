using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.ElementalVFX
{
    public class BaseVfx : MonoBehaviour
    {
        [SerializeField] float _SafetyDestroy; //Destroy the object after a certan time in case user error keeps it active.
        [SerializeField] float _DestoyDelay; //Wait for effect to finish stopping before destroying the GameObject
        protected VfxData _data;
        
         
        [SerializeField] Transform _source;
        [SerializeField] Transform _Target;
        [SerializeField] protected float duration = 3f;
        [SerializeField] private float radius = 1f;
        public virtual void Play()
        {
            VfxData data = new VfxData(_source.position, _Target.position, duration, radius);
            _data = data;
            if (_data.Duration > _SafetyDestroy)
            {
                _SafetyDestroy += _data.Duration;//Offset the safety destroy by the duration if bigger;
            }
            //Destroy(gameObject, _SafetyDestroy);
            //Invoke(nameof(Stop),duration);
            //StopAllCoroutines();
        }
        public virtual void Play(VfxData data)
        {
            _data = data;
            if (_data.Duration > _SafetyDestroy)
            {
                _SafetyDestroy += _data.Duration;//Offset the safety destroy by the duration if bigger;
            }
            Destroy(gameObject, _SafetyDestroy);
            Invoke(nameof(Stop), _data.Duration);
            StopAllCoroutines();
        }

        public virtual void Stop()
        {
            StopAllCoroutines();
            //Destroy(gameObject, _DestoyDelay);
        }
    }
}
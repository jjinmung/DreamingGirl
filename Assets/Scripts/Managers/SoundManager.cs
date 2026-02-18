using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Define;
using Object = UnityEngine.Object;

public class SoundManager
{
    private AudioSource[] _audioSources = new AudioSource[(int)Sound.MaxCount];
    private List<AudioSource> _loopSources = new();
    private Dictionary<string, float> _lastPlayTimes = new();

    private float _bgmVolume = 1f;
    public float BGMVolume
    {
        get => _bgmVolume;
        set {
            _bgmVolume = Math.Clamp(value, 0f, 1f);
            _audioSources[(int)Sound.Bgm].volume = _bgmVolume;
        }
    }

    private float _effectVolume = 0.5f;
    public float EffectVolume
    {
        get => _effectVolume;
        set {
            _effectVolume = Math.Clamp(value, 0f, 1f);
            _audioSources[(int)Sound.Effect].volume = _effectVolume;
            // 루프 중인 소스들의 볼륨도 일괄 조절
            foreach (var s in _loopSources) if (s.isPlaying) s.volume = _effectVolume;
        }
    }

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound") ?? new GameObject { name = "@Sound" };
        Object.DontDestroyOnLoad(root);

        string[] soundNames = Enum.GetNames(typeof(Sound));
        for (int i = 0; i < soundNames.Length - 1; i++)
        {
            GameObject go = new GameObject { name = soundNames[i] };
            _audioSources[i] = go.AddComponent<AudioSource>();
            go.transform.parent = root.transform;
        }

        _audioSources[(int)Sound.Bgm].loop = true;
        
        // 초기 BGM 실행 (AddressableData 활용)
        PlayBgm(Managers.Resource.Data.LobyBGM).Forget();
    }

    #region BGM Logic
    
    public async UniTask PlayBgm(AssetReference assetRef, float fadeTime = 1.0f)
    {
        if (assetRef == null || !assetRef.RuntimeKeyIsValid()) return;
        await PlayBgm(assetRef.RuntimeKey.ToString(), fadeTime);
    }

    public async UniTask PlayBgm(string address, float fadeTime = 1.0f)
    {
        AudioClip clip = await Managers.Resource.LoadAsync<AudioClip>(address);
        if (clip == null) return;

        AudioSource source = _audioSources[(int)Sound.Bgm];
        if (source.clip == clip && source.isPlaying) return;

        source.DOKill();

        if (source.isPlaying && fadeTime > 0)
        {
            await source.DOFade(0, fadeTime).AsyncWaitForCompletion();
            source.Stop();
        }

        source.clip = clip;
        source.Play();
        source.DOFade(_bgmVolume, fadeTime);
    }
    #endregion

    #region Effect Logic

    public async UniTask PlayEffect(AssetReference assetRef, float pitch = 1.0f)
    {
        if (assetRef == null || !assetRef.RuntimeKeyIsValid()) return;
        await PlayEffect(assetRef.RuntimeKey.ToString(), pitch);
    }

    public async UniTask PlayEffect(string address, float pitch = 1.0f)
    {
        // 중복 재생 방지 (0.03초)
        if (_lastPlayTimes.TryGetValue(address, out float lastTime))
        {
            if (Time.time - lastTime < 0.03f) return;
        }
        _lastPlayTimes[address] = Time.time;

        AudioClip clip = await Managers.Resource.LoadAsync<AudioClip>(address);
        if (clip == null) return;

        // 피치 변형 (선택 사항)
        float variedPitch = pitch * UnityEngine.Random.Range(0.95f, 1.05f);
        _audioSources[(int)Sound.Effect].PlayOneShot(clip, _effectVolume);
        _audioSources[(int)Sound.Effect].pitch = variedPitch;
    }

    public async UniTask<AudioSource> PlayEffectLoop(AssetReference assetRef, float pitch = 1.0f)
    {
        if (assetRef == null || !assetRef.RuntimeKeyIsValid()) return null;
        
        AudioClip clip = await Managers.Resource.LoadAsync<AudioClip>(assetRef);
        if (clip == null) return null;

        AudioSource source = GetOrCreateLoopSource();
        source.clip = clip;
        source.pitch = pitch;
        source.loop = true;
        source.volume = _effectVolume;
        source.Play();

        return source;
    }
    public void StopLoop(AudioSource source, float fadeTime = 0.5f)
    {
        if (source == null || !source.isPlaying) return;
        
        if (fadeTime > 0)
        {
            source.DOFade(0, fadeTime).OnComplete(() =>
            {
                source.Stop();
                source.clip = null; // 참조 해제
            });

        }
        else
        {
            source.Stop();
            source.clip = null;

        }

    }


    #endregion

    private AudioSource GetOrCreateLoopSource()
    {
        foreach (var source in _loopSources)
            if (!source.isPlaying) return source;

        GameObject go = new GameObject { name = $"LoopEffect_{_loopSources.Count}" };
        go.transform.parent = GameObject.Find("@Sound").transform;
        AudioSource newSource = go.AddComponent<AudioSource>();
        _loopSources.Add(newSource);
        return newSource;
    }

    public void Stop(Sound type) => _audioSources[(int)type].Stop();
    public void StopFade(Sound type, float fadeTime =2f)
    {
        AudioSource audioSource = _audioSources[(int)type];
        if (audioSource.isPlaying)
        {
            audioSource.DOFade(0, fadeTime).OnComplete(() =>
            {
                audioSource.Stop();
            });
        }
    }
    public void Clear()
    {
        foreach (var source in _audioSources)
        {
            source.clip = null;
            source.Stop();
        }
        _lastPlayTimes.Clear();
    }
}
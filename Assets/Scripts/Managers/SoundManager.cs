using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Define;
using Object = UnityEngine.Object;

public class SoundManager
{
    AudioSource[] _audioSources = new AudioSource[(int)Sound.MaxCount];
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
    private List<AudioSource> _loopSources = new List<AudioSource>();
    Dictionary<string, UniTaskCompletionSource<AudioClip>> _loadTasks
	    = new Dictionary<string, UniTaskCompletionSource<AudioClip>>();
    private Dictionary<string, float> _lastPlayTimes = new Dictionary<string, float>();
    
    public float BGMVolume { get; set; }

    public float EffectVolume { get; set; }
    // MP3 Player   -> AudioSource
    // MP3 음원     -> AudioClip
    // 관객(귀)     -> AudioListener

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            string[] soundNames = Enum.GetNames(typeof(Sound));
            for (int i = 0; i < soundNames.Length - 1; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root.transform;
            }

            _audioSources[(int)Sound.Bgm].loop = true;
        }

        BGMVolume = 1f;
        EffectVolume = 0.5f;
        PlayBgm(Address.LobyBGM).Forget();
    }

    // BGM 전용 (비동기 & 페이드)
    public async UniTask PlayBgm(string address, float fadeTime = 2.0f)
    {
	    AudioClip clip = await GetOrAddAudioClip(address, Sound.Bgm);
	    AudioSource source = _audioSources[(int)Sound.Bgm];

	    // 1. 이미 같은 음악이 재생 중이라면 무시 (중복 호출 방지)
	    if (source.clip == clip && source.isPlaying)
		    return;

	    // 2. DOTween 트윈이 겹치지 않도록 기존 트윈 제거
	    source.DOKill(); 

	    if (source.isPlaying)
	    {
		    // 3. 페이드 아웃 후 새로운 클립 재생 (Sequence 활용)
		    Sequence seq = DOTween.Sequence();
		    await seq.Append(source.DOFade(0, fadeTime))
			    .AppendCallback(() => 
			    {
				    source.Stop();
				    source.clip = clip;
				    source.Play();
			    })
			    .Append(source.DOFade(BGMVolume, fadeTime));
	    }
	    else
	    {
		    // 즉시 재생
		    source.clip = clip;
		    source.volume = 0;
		    source.Play();
		    await source.DOFade(BGMVolume, fadeTime);
	    }
    }


	// Effect 전용 (주소 기반 비동기)
	public async UniTask PlayEffect(string address, float pitch = 1.0f)
	{
		// 2. 0.03초 사이의 중복 호출은 무시 
		if (_lastPlayTimes.TryGetValue(address, out float lastTime))
		{
			if (Time.time - lastTime < 0.03f) return;
		}
    
		_lastPlayTimes[address] = Time.time;

		AudioClip clip = await GetOrAddAudioClip(address, Sound.Effect);
    
		// 3. 미세한 피치 변형 추가 (타격감이 훨씬 풍성해짐)
		float variedPitch = pitch * UnityEngine.Random.Range(0.95f, 1.05f);
    
		Play(clip, Sound.Effect, variedPitch);
	}

    #region 반복이펙트 전용 함수

    // 1. 사용 가능한(재생 중이지 않은) 루프용 소스를 찾거나 생성하는 메서드
    private AudioSource GetOrCreateLoopSource()
    {
	    // 이미 생성된 소스 중 놀고 있는(isPlaying == false) 소스 찾기
	    foreach (var source in _loopSources)
	    {
		    if (!source.isPlaying) return source;
	    }

	    // 없다면 새로 생성
	    GameObject root = GameObject.Find("@Sound");
	    GameObject go = new GameObject { name = $"LoopEffect_{_loopSources.Count}" };
	    go.transform.parent = root.transform;
    
	    AudioSource newSource = go.AddComponent<AudioSource>();
	    _loopSources.Add(newSource);
	    return newSource;
    }

// 2. 이펙트 루프 재생 메서드
	public async UniTask<AudioSource> PlayEffectLoop(string address, float pitch = 1.0f)
    {
	    AudioClip clip = await GetOrAddAudioClip(address, Sound.Effect);
	    if (clip == null) return null;

	    AudioSource source = GetOrCreateLoopSource();
    
	    source.clip = clip;
	    source.pitch = pitch;
	    source.loop = true; // 루프 설정
	    source.volume = EffectVolume;
	    source.Play();

	    return source; // 나중에 멈추기 위해 리턴해줌
    }

	// 3. 특정 루프 사운드 정지 (페이드 아웃 포함 가능)
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
 

// 공용 핵심 재생 로직 (Private으로 보호)
    private void Play(AudioClip audioClip, Sound type = Sound.Effect, float pitch = 1.0f)
    {
	    if (audioClip == null) return;

	    if (type == Sound.Bgm)
	    {
		    AudioSource audioSource = _audioSources[(int)Sound.Bgm];
		    audioSource.pitch = pitch;
		    audioSource.clip = audioClip;
		    audioSource.volume = BGMVolume;
		    audioSource.Play();
	    }
	    else
	    {
		    // Effect 채널 소스 사용
		    AudioSource audioSource = _audioSources[(int)Sound.Effect];
		    audioSource.pitch = pitch;
		    // PlayOneShot은 여러 소리가 겹쳐서 나게 해줍니다.
		    audioSource.PlayOneShot(audioClip, EffectVolume);
	    }
    }
    
    // 사운드 중지 기능 추가
    public void Stop(Define.Sound type)
    {
	    AudioSource audioSource = _audioSources[(int)type];
	    audioSource.Stop();
    }
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
    

    async UniTask<AudioClip> GetOrAddAudioClip(string address, Sound type = Sound.Effect)
    {
	    if (type == Sound.Bgm)
		    return await Managers.Resource.LoadAsync<AudioClip>(address);

	    // 1. 이미 로드 완료된 클립
	    if (_audioClips.TryGetValue(address, out var cachedClip))
		    return cachedClip;

	    // 2. 이미 로딩 중이면 그 결과를 기다림
	    if (_loadTasks.TryGetValue(address, out var existingTcs))
		    return await existingTcs.Task;

	    // 3. 새 로딩 시작
	    var tcs = new UniTaskCompletionSource<AudioClip>();
	    _loadTasks[address] = tcs;

	    try
	    {
		    var clip = await Managers.Resource.LoadAsync<AudioClip>(address);

		    if (clip != null)
			    _audioClips[address] = clip;

		    tcs.TrySetResult(clip);
		    return clip;
	    }
	    catch (Exception e)
	    {
		    tcs.TrySetException(e);
		    Debug.LogError($"Failed to load clip: {address}\n{e}");
		    return null;
	    }
	    finally
	    {
		    _loadTasks.Remove(address);
	    }
    }

	
}

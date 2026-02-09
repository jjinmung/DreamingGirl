using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using static Define;

public class SoundManager
{
    AudioSource[] _audioSources = new AudioSource[(int)Sound.MaxCount];
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
    public float BGMVolume { get; set; }

    public float EffectVolume { get; set; }
    // MP3 Player   -> AudioSource
    // MP3 음원     -> AudioClip
    // 관객(귀)     -> AudioListener

    public async Task Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            string[] soundNames = System.Enum.GetNames(typeof(Sound));
            for (int i = 0; i < soundNames.Length - 1; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root.transform;
            }

            _audioSources[(int)Sound.Bgm].loop = true;
        }

        BGMVolume = 1f;
        EffectVolume = 1f;
        await PlayBgm(Address.LobyBGM);
    }

    // BGM 전용 (비동기 & 페이드)
    public async Task PlayBgm(string address, float fadeTime = 1.0f)
    {
	    AudioClip clip = await GetOrAddAudioClip(address, Sound.Bgm);
	    AudioSource source = _audioSources[(int)Sound.Bgm];

	    if (source.isPlaying)
	    {
		    await source.DOFade(0, fadeTime).AsyncWaitForCompletion();
		    source.Stop();
	    }

	    source.clip = clip;
	    source.volume = 0;
	    source.Play();
	    source.DOFade(BGMVolume, fadeTime);
    }

// Effect 전용 (주소 기반 비동기)
    public async Task PlayEffect(string address, float pitch = 1.0f)
    {
	    AudioClip clip = await GetOrAddAudioClip(address, Sound.Effect);
	    Play(clip, Sound.Effect, pitch);
    }

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
    

	async Task<AudioClip> GetOrAddAudioClip(string address, Sound type = Sound.Effect)
    {
		AudioClip audioClip = null;

		if (type == Sound.Bgm)
		{
			audioClip = await Managers.Resource.LoadAsync<AudioClip>(address);
		}
		else
		{
			if (_audioClips.TryGetValue(address, out audioClip) == false)
			{
				audioClip =await Managers.Resource.LoadAsync<AudioClip>(address);
				_audioClips.Add(address, audioClip);
			}
		}

		if (audioClip == null)
			Debug.Log($"AudioClip Missing ! {address}");

		return audioClip;
    }
}

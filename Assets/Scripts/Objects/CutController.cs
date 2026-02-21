using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutController : MonoBehaviour
{
    [SerializeField] private AudioSource[] audioSources;
    public void CutToBattle()
    {
        SceneManager.sceneLoaded += Managers.Camera.OnBattleSceneLoaded;
        SceneManager.LoadScene("BattleScene");
    }

    public void FadeSound()
    {
        foreach (var audioSource in audioSources)
        {
            audioSource.DOFade(0f, 1f);
        }
    }
    
}

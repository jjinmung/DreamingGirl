using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutController : MonoBehaviour
{
    [SerializeField] private AudioSource[] audioSources;
    public void CutToBattle()
    {
        SceneManager.sceneLoaded += OnBattleSceneLoaded;
        SceneManager.LoadScene("BattleScene");
    }
    
    public async void OnBattleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "BattleScene")
        {
            var battleUI = await Managers.UI.ShowSceneUI<UI_BattleScene>();
            battleUI.Init();
            
            //맵 생성
            await Managers.Stage.Init();
            
            //캐릭터 생성
            var go = await Managers.Player.CreatePlayer();
            
            //카메라 세팅
            Managers.Camera.BattleInit(go.transform);
            Managers.Camera.SetTarget(go.transform);
            
            //씬 세팅
            battleUI.LazyInit();
            
            //튜토리얼 대화
            Managers.Player.Control.InputActive(false);
            await UniTask.Delay(3000); 
            Managers.Dialogue.StartDialogue("1");
            SceneManager.sceneLoaded -= OnBattleSceneLoaded;
        }
    }
    

    public void FadeSound()
    {
        foreach (var audioSource in audioSources)
        {
            audioSource.DOFade(0f, 1f);
        }
    }
    
}

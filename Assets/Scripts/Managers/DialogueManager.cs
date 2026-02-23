using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    // 1. 에디터에서 대화 리스트를 관리 (Dictionary 대용으로 ID 사용 가능)
    [SerializeField] private List<DialogueData> dialogueDB = new List<DialogueData>();
    
    private UI_Dialogue dialogueUI;
    public bool IsDialogueActive { get; private set; }

    public void Init()
    {
        // 팝업 UI 시스템이 있다면 거기서 가져오거나 생성
        // 예: dialogueUI = Managers.UI.ShowPopupUI<UI_Dialogue>();
    }

    // ID나 이름을 통해 특정 대화를 시작하는 함수
    public async void StartDialogue(string dialogueName)
    {
        if (IsDialogueActive) return;

        // 리스트에서 이름이 일치하는 데이터 찾기
        DialogueData data = dialogueDB.Find(x => x.name == dialogueName);

        if (data != null)
        {
            IsDialogueActive = true;
            
            // UI 열기 및 대화 시작
            dialogueUI = await Managers.UI.MakeSubItem<UI_Dialogue>(Managers.Resource.Data.UI_Diaogue);
            dialogueUI.StartDialogue(data);
            
            Managers.Player.Control.InputActive(false);
        }
        else
        {
            Debug.LogError($"대화 데이터를 찾을 수 없습니다: {dialogueName}");
        }

        Managers.Sound.PlayBgm(Managers.Resource.Data.DialogueBGM).Forget();
    }

    // 대화 종료 시 호출 (UI_Dialogue의 EndDialogue에서 호출해주면 좋습니다)
    public void OnDialogueEnd()
    {
        IsDialogueActive = false;
        Managers.Player.Control.InputActive(true);
        Managers.Sound.StopFade(Define.Sound.Bgm,1f);
    }
}
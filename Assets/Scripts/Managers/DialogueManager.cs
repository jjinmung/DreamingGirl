using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    // 1. 에디터에서 대화 리스트를 관리 (Dictionary 대용으로 ID 사용 가능)
    [SerializeField] private List<DialogueData> dialogueDB = new List<DialogueData>();
    private Dictionary<DialogueData, Action> _events = new Dictionary<DialogueData, Action>();

    public void Subscribe(string dialogueName, Action action)
    {
        // 1. 이름이 비어있는지 확인
        if (string.IsNullOrEmpty(dialogueName))
        {
            Debug.LogError("Subscribe 실패: 전달된 dialogueName이 null이나 빈 문자열입니다.");
            return;
        }

        // 2. DB에서 데이터 찾기
        DialogueData data = dialogueDB.Find(x => x.name == dialogueName);

        // 3. 찾지 못했을 경우 예외 처리
        if (data == null)
        {
            Debug.LogError($"Subscribe 실패: DB에서 '{dialogueName}'라는 이름의 DialogueData를 찾을 수 없습니다. (현재 DB 개수: {dialogueDB.Count})");
            return;
        }

        // 4. 안전하게 딕셔너리 작업
        if (!_events.ContainsKey(data)) _events[data] = null;
        _events[data] += action;
    }

    public void Unubscribe(string dialogueName, Action action)
    {
        DialogueData data = dialogueDB.Find(x => x.name == dialogueName);
        if (_events.ContainsKey(data))
            _events[data] -= action;
    }
    public void Publish(DialogueData evt)
    {
        if (_events.ContainsKey(evt)) _events[evt]?.Invoke();
    }
    private UI_Dialogue dialogueUI;
    public bool IsDialogueActive { get; private set; }
    

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
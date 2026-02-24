using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Dialogue : UI_Base
{
    private DialogueData currentData;
    private int currentIndex = -1;
    private Sprite nowPortrait;
    enum Images
    {
        PortraitL,
        PortraitR
    }

    enum Texts
    {
        DialogueText
    }

    private void Awake()
    {
        Init();
    }
    

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }
    
    public void StartDialogue(DialogueData data)
    {
        GetImage((int)Images.PortraitL).gameObject.SetActive(false);
        GetImage((int)Images.PortraitR).gameObject.SetActive(false);
        currentData = data;
        currentIndex = 0;
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // 1. 대화가 끝났는지 확인
        if (currentIndex >= currentData.dialogues.Count)
        {
            EndDialogue();
            return;
        }

        // 3. 다음 대화 출력
        var entry = currentData.dialogues[currentIndex];
        //nameText.text = entry.speakerName;
        
        //4.초상화 적용
        SetPortrait(entry.portrait, entry.isRight);
            
        StopAllCoroutines();
        StartCoroutine(TypeSentence(entry.dialogueText));
        
        currentIndex++;
    }

    IEnumerator TypeSentence(string sentence)
    {
        var dialogueText = GetText((int)Texts.DialogueText);
        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.03f);
        }
        yield return new WaitForSeconds(3f);
        DisplayNextSentence();
    }

    void EndDialogue()
    {
        Debug.Log("모든 대화 완료");
        //이벤트 호출
        Managers.Dialogue.Publish(currentData);
        
        currentIndex = -1;
        Managers.Dialogue.OnDialogueEnd();
        Managers.Resource.Destroy(gameObject);
    }

    void SetPortrait(Sprite sprite, bool isRight)
    {
        Image targetImage = isRight ? GetImage((int)Images.PortraitR) : GetImage((int)Images.PortraitL);
        Image otherImage = isRight ? GetImage((int)Images.PortraitL) : GetImage((int)Images.PortraitR);
        
        // 1. 빠른 클릭 시 애니메이션 꼬임 방지 (진행 중인 트윈 취소)
        targetImage.DOKill();
        otherImage.DOKill();
        
        // 2. 이미지가 바뀐 경우 (표정이 바뀌거나 화자가 바뀜)
        if (sprite != nowPortrait&&sprite!=null)
        {
            nowPortrait = sprite;
            // 케이스 A: 이미 켜져 있던 타겟 이미지라면 (같은 위치, 표정만 바뀜)
            if (targetImage.gameObject.activeSelf) 
            {
                // 부드럽게 페이드 아웃 -> 사진 교체 -> 페이드 인
                targetImage.DOFade(0f, 0.15f).OnComplete(() =>
                {
                    targetImage.sprite = nowPortrait;
                    targetImage.DOFade(1f, 0.15f);
                });
            }
            // 케이스 B: 새로 켜지는 경우 (반대쪽에서 넘어온 새로운 화자)
            else 
            {
                // 일단 투명하게 만든 후 사진을 넣고 스르륵 나타나게 함
                Color c = targetImage.color;
                c.a = 0f;
                targetImage.color = c;
            
                targetImage.sprite = nowPortrait;
                targetImage.DOFade(1f, 0.2f); // 0.2초 동안 페이드 인
            }
        }
        // 반대쪽 이미지(이전 화자)가 켜져 있다면 부드럽게 페이드 아웃 후 끄기
        if (otherImage.gameObject.activeSelf)
        {
            otherImage.DOFade(0f, 0.15f).OnComplete(() =>
            {
                otherImage.gameObject.SetActive(false);
            });
        }

        if (sprite != null)
        {
            targetImage.gameObject.SetActive(true);
        }
        else
        {
            targetImage.gameObject.SetActive(false);
            nowPortrait= null;
        }
            

    }
}

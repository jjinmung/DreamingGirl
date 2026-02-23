using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Configs/DialogueData")]
public class DialogueData:ScriptableObject
{
    public List<DialogueEntry> dialogues;
}
[System.Serializable]
public class DialogueEntry
{
    public string speakerName;
    [TextArea(3, 10)]
    public string dialogueText;
    
    public Sprite portrait; 
    public bool isRight; // 초상화의 위치
    
    
}
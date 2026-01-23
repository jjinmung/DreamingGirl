using UnityEngine;

[CreateAssetMenu(fileName = "PassiveData", menuName = "ScriptableObjects/PassiveData")]
public class PassiveData : ScriptableObject
{
    public Define.PassiveSkillID skillID;
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
    public int price;

    // 이 데이터가 어떤 로직을 실행할지 연결
    public PassiveEffect GetEffect()
    {
        return PassiveFactory.CreateEffect(skillID);
    }
}

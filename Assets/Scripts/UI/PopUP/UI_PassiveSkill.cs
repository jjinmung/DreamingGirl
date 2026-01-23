using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PassiveSkill : UI_Popup
{
    Color InactiveColor= new Color(100,100,100,1);
    Color ActiveColor= new Color(255,255,255,1);
    enum Texts
    {
        
    }

    

    enum Images
    {
        DungeonBreath_Icon,     
        LifeDrain_Icon,         

        CriticalSense_Icon,     
        PowerAwakening_Icon,    
        HasteInstinct_Icon,    
        DecisiveStrike_Icon,    

        UnyieldingVitality_Icon,
        LastChance_Icon,       

        SwiftBoots_Icon,        
        FireSpirit_Icon,       
        WaterSpirit_Icon,       
        FateChoice_Icon         
    }
    enum GameObjects
    {
        DungeonBreath,     
        LifeDrain,         

        CriticalSense,     
        PowerAwakening,    
        HasteInstinct,     
        DecisiveStrike,    

        UnyieldingVitality,
        LastChance,        

        SwiftBoots,        
        FireSpirit,        
        WaterSpirit,       
        FateChoice         
    }

    private void Start()
    {
        Init();
    }
    
    public override void Init()
    {
        base.Init();
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));
    }
}

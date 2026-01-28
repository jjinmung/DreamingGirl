public class Define
{
    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Game,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        Drag,
        Enter,
        Exit,
    }
    

    public enum CameraMode
    {
        QuarterView,
        ThirdView,
    }
    
    public enum PassiveSkillID
    {
        DungeonBreath,     // 던전의 숨결
        LifeDrain,         // 생명 흡수

        CriticalSense,     // 치명의 감각
        PowerAwakening,    // 힘의 각성
        HasteInstinct,     // 가속의 본능
        DecisiveStrike,    // 결정타

        UnyieldingVitality,// 불굴의 체력
        LastChance,        // 마지막 기회

        SwiftBoots,        // 신속의 장화
        FireSpirit,        // 불의 정령
        IceSpirit,       // 얼음의 정령
        FateChoice         // 운명의 선택
    }

    public enum PlayerStat
    {
        Attack, 
        MaxHP, 
        MoveSpeed, 
        Critical,
        DashCooldown,
        attackSpeed
    }
    


    public enum AbilityID
    {
        Fire,//불의 낙인 
        Frost,//빙결의 손길
        Flurry_Slash,//연속베기
        Thunderflash,//벽력일섬
        Pact_Abyss,//심연의 계약
        Chain_Instinct,//연쇄 본능
        Divine_Orbs,//신의 가호
        Enhanced_Strength,//근력 강화
        Enhanced_Vitality,//육체 강화
        Swift_Execution,//속전속결
        None,
    }
    
    public enum RoomType { Monster, Event, Boss,Loby }
}

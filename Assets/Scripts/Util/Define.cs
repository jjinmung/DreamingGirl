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
        WaterSpirit,       // 물의 정령
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
    
}

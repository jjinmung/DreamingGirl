using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "AddressableData", menuName = "Configs/AddressableData")]
public class AddressableData : ScriptableObject
{
    [Header("Prefabs")]
    public AssetReference Player;
    public AssetReference Boss;
    public AssetReference Coin;

    [Header("UI")] 
    public AssetReference Boss_HP_BAR;
    public AssetReference UI_Blood;
    public AssetReference UI_FloatingText;
    public AssetReference CircleAttackRange;
    public AssetReference UI_Diaogue;

    [Header("Texture")] 
    public AssetReference EnemyMap;
    public AssetReference EventMap;
    public AssetReference BossMap;

    [Header("BGM")] 
    public AssetReference LobyBGM;
    public AssetReference BossMapBGM;
    public AssetReference EventRoomBGM;
    public AssetReference OnBattleBGM;
    public AssetReference StoreMapBGM;
    public AssetReference DeathBGM;
    public AssetReference DialogueBGM;
    
    [Header("EnemySFX")]
    public AssetReference Enemy01Die;
    public AssetReference Enemy01Shoot;
    public AssetReference Enemy02Shoot;
    public AssetReference Enemy02Die;
    public AssetReference Enemy03Roar;
    public AssetReference Enemy03Beam;
    public AssetReference Enemy03Blast;
    public AssetReference Enemy03Dash;
    public AssetReference Enemy03BallShoot;
    public AssetReference Enemy04Die;
    public AssetReference Enemy05Die;
    public AssetReference Hit;

    [Header("PlayerSFX")]
    public AssetReference PlayerNormalAttack;
    public AssetReference FireAttack;
    public AssetReference IceAttack;
    public AssetReference LevelUp;
    public AssetReference PlayerDash;
    public AssetReference PlayerWalk;
    public AssetReference Flash;
    public AssetReference FoodEat;
    public AssetReference Mimic;
    public AssetReference Stun;
    
    [Header("UISFX")]
    public AssetReference ClosePopup;
    public AssetReference OpenPopup;
    public AssetReference Gold;

}
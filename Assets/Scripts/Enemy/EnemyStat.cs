using System;
using Data;

[Serializable]
public class EnemyStat
{
    public int ID;
    public string Name;
    public float MaxHp;
    public float Damage;
    public float Speed;
    public float AttackDelay;
    public float currentHp;
    public int Exp;
    public int Gold;
    public  EnemyStat(MonsterStat stat)
    {
        ID = stat.ID;
        Name = stat.Name;
        MaxHp = stat.MaxHp;
        currentHp = MaxHp;
        Damage = stat.Damage;
        Speed = stat.Speed;
        AttackDelay = stat.AttackDelay;
        Exp = stat.Exp;
        Gold = stat.Gold;
    }
}
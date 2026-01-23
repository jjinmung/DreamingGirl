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
    public  EnemyStat(MonsterStat stat)
    {
        ID = stat.ID;
        Name = stat.Name;
        this.MaxHp = stat.MaxHp;
        this.currentHp = MaxHp;
        this.Damage = stat.Damage;
        this.Speed = stat.Speed;
        this.AttackDelay = stat.AttackDelay;
    }
}
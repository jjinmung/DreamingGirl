using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    // --- [정적 데이터: 구글 시트용] ---
    [Serializable]
    public class MonsterStat
    {
        public int ID;
        public string Name;
        public float MaxHp;
        public float Damage;
        public float Speed;
        public float AttackDelay;
    }
    [Serializable]
    public class PlayerBasicStat
    {
        public int ID;
        public float MaxHp;
        public float Damage;
        public float Speed;
        public float DashCooldown;
        public float CritChance;
        public float AttackSpeed;
    }
    
    [Serializable]
    public class StaticDataRoot
    {
        public List<MonsterStat> monsters;
        public List<PlayerBasicStat> PlayerBasicStat;
    }
    

    // --- [동적 데이터: 세이브 파일용] ---
    [Serializable]
    public class SaveData
    {
        public PlayerSaveData player = new();
        public SettingsSaveData settings = new();
    }
    [Serializable]
    public class PlayerSaveData
    {
        public float currentHp;
        public int gold;
        public SerializableVector3 position;
        public List<Define.PassiveSkillID> ownedPassives = new();
        // 초기화를 위한 메서드
        public void ResetStats(float maxHp,Vector3 pos)
        {
            currentHp = maxHp;
            gold = 0;
            position = pos;
        }
        
        public bool HasPassive(Define.PassiveSkillID skillID)
        {
            foreach (var passiveSkill in ownedPassives)
                if(passiveSkill==skillID )
                    return true;
            return false;
        }
    }
    [Serializable]
    public struct SerializableVector3
    {
        public float x, y, z;

        public SerializableVector3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
    
        // Vector3와 상호 변환이 쉽도록 연산자 오버로딩
        public static implicit operator Vector3(SerializableVector3 v) => new Vector3(v.x, v.y, v.z);
        public static implicit operator SerializableVector3(Vector3 v) => new SerializableVector3(v);
    }
    [Serializable]
    public class SettingsSaveData
    {
        public float playTime;
        public string lastSaveDate;
        public float bgmVolume = 1.0f;
        public float effectVolume = 1.0f;
    }
    
}
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json; 
using Data;

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
        // 초기화를 위한 메서드
        public void ResetStats(float maxHp,Vector3 pos)
        {
            currentHp = maxHp;
            gold = 0;
            position = pos;
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

public class DataManager
{
    public Dictionary<int, MonsterStat> MonsterDict { get; private set; }
    public Dictionary<int, PlayerBasicStat> PlayerBasicStat { get; private set; }

    private int playerIndex = 1;

    // [동적 데이터 (세이브)]
    public SaveData SaveData { get; private set; } = new SaveData();
    private int savedataCount;
    // 현재 선택된 슬롯 (1~4)
    private int _saveDataIndex = 1;
    public int SaveDataIndex 
    { 
        get => _saveDataIndex; 
        set => _saveDataIndex = Mathf.Clamp(value, 1, 4); // 1~4 사이로 제한
    }

    // 파일 경로 생성 시 현재 인덱스 반영
    private string GetSavePath(int index) => Path.Combine(Application.persistentDataPath, $"SaveData_{Mathf.Clamp(index, 1, 4)}.json");
    private string CurrentSavePath => GetSavePath(SaveDataIndex);
    
    private float _sessionStartTime;
    
    private Vector3 StartPos = new Vector3(12f, 0f, 5f);
    public void Init()
    {
        // 1. 정적 데이터 로드
        StaticDataRoot root = LoadStaticData("StaticData");
        if (root == null) return;

        // 몬스터 딕셔너리 채우기
        MonsterDict = new Dictionary<int, MonsterStat>();
        foreach (var m in root.monsters) MonsterDict.Add(m.ID, m);

        // 플레이어 기본 스탯 딕셔너리 채우기
        PlayerBasicStat = new Dictionary<int, PlayerBasicStat>();
        foreach (var p in root.PlayerBasicStat) PlayerBasicStat.Add(p.ID, p);

        savedataCount = 0;
        //슬롯 갯수세기
        for (int i = 1; i <= 4; i++)
        {
            if (HasSaveFile(i)) savedataCount++;
            else break;
        }
    }
    // 슬롯 변경 및 해당 데이터 로드
    public void ChangeSlot(int index)
    {
        SaveDataIndex = index;
        LoadGame();
    }

    // 특정 슬롯에 데이터가 있는지 확인 (UI 슬롯 표시용)
    public bool HasSaveFile(int index)
    {
        return File.Exists(GetSavePath(index));
    }

    // --- [세이브/로드 로직] ---

    public void SaveGame()
    {
        // 1. 플레이 타임 계산
        float sessionDuration = Time.unscaledTime - _sessionStartTime;
        SaveData.settings.playTime += sessionDuration;
        _sessionStartTime = Time.unscaledTime;

        // 2. [핵심] 현재 날짜와 시간을 문자열로 기록
        // 포맷 예시: 2023-10-27 15:30:45
        SaveData.settings.lastSaveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 3. JSON 변환 및 저장
        string json = JsonConvert.SerializeObject(SaveData, Formatting.Indented);
        File.WriteAllText(CurrentSavePath, json);
        Debug.Log($"Saved. Total PlayTime: {GetFormattedPlayTime()}");
    }

    public void LoadGame(bool isNew = false)
    {
        if (!File.Exists(CurrentSavePath) || isNew)
        {
            savedataCount++;
            _saveDataIndex = savedataCount;
            // 신규 세이브 데이터 생성 (새 데이터 만들기)
            SaveData = new SaveData();
        
            if (PlayerBasicStat.TryGetValue(playerIndex, out PlayerBasicStat basicStat))
            {
                SaveData.player.ResetStats(basicStat.MaxHp,StartPos);
                Debug.Log($"슬롯 {SaveDataIndex} 신규 생성. HP: {SaveData.player.currentHp}");
            }
            
            // 처음 생성 시 파일로 한 번 저장
            SaveGame();
            _sessionStartTime = Time.unscaledTime;
            return;
        }
        _sessionStartTime = Time.unscaledTime;
        string json = File.ReadAllText(CurrentSavePath);
        SaveData = JsonConvert.DeserializeObject<SaveData>(json);
        Debug.Log($"Slot {SaveDataIndex} Loaded.");
    }

    // 슬롯 삭제 (초기화 기능)
    public void ClearSlot(int index)
    {
        string path = GetSavePath(index);
        if (File.Exists(path))
        {
            File.Delete(path);
            savedataCount--;
            Debug.Log($"Slot {index} Deleted.");
        }
    }
    // 세이브 데이터 요약 함수
    public string GetSlotSummary(int index)
    {
        string path = GetSavePath(index);
        if (!File.Exists(path)) return "데이터 없음";

        string json = File.ReadAllText(path);
        SaveData data = JsonConvert.DeserializeObject<SaveData>(json);

        // 슬롯에 표시될 텍스트 예시: "마지막 접속 시간 : 2023-10-27 15:30 \n 플레이 시간 : 00:00:00 \n 레벨 : 5 골드 : 100"
        return $" 마지막 접속 시간 : {data.settings.lastSaveDate}\n 플레이 시간 : {GetFormattedPlayTime()}\n " +
               $"현재 골드 : {data.player.gold}";
    }
    
    public string GetFormattedPlayTime()
    {
        // 누적 시간 + 현재 세션 진행 시간
        float totalSeconds = SaveData.settings.playTime + (Time.unscaledTime - _sessionStartTime);
        
        TimeSpan t = TimeSpan.FromSeconds(totalSeconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", 
            t.Hours + (t.Days * 24), // 24시간 넘어가도 시간으로 표시
            t.Minutes, 
            t.Seconds);
    }
    private StaticDataRoot LoadStaticData(string path)
    {
        TextAsset asset = Managers.Resource.Load<TextAsset>($"Assets/Json/{path}.json");

        if (asset == null)
        {
            Debug.LogError($"[DataManager] Missing JSON: {path}");
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<StaticDataRoot>(asset.text);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataManager] JSON Parse Error\n{e}");
            return null;
        }
    }

}



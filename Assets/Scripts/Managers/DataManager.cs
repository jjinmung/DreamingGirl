using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json; 
using Data;
using static Define;



public class DataManager
{
    public Dictionary<int, MonsterStat> MonsterDict { get; private set; }
    public Dictionary<int, PlayerBasicStat> PlayerBasicStat { get; private set; }
    
    public Dictionary<PassiveSkillID, PassiveData> PassiveDict { get; private set; }
    public Dictionary<AbilityID, AbilityInstance> AbilityDict { get; private set; }

    private int playerIndex = 1;

    // [동적 데이터 (세이브)]
    public SaveData SaveData { get; private set; } = new SaveData();
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
        // 1. 정적 데이터 json파일 로드
        StaticDataRoot root = LoadStaticData("StaticData");
        if (root == null) return;

        // 몬스터 딕셔너리 채우기
        MonsterDict = new Dictionary<int, MonsterStat>();
        foreach (var m in root.monsters) MonsterDict.Add(m.ID, m);

        // 플레이어 기본 스탯 딕셔너리 채우기
        PlayerBasicStat = new Dictionary<int, PlayerBasicStat>();
        foreach (var p in root.PlayerBasicStat) PlayerBasicStat.Add(p.ID, p);
        
        //패시브 스킬 딕셔너리 채우기
        PassiveDict = new Dictionary<PassiveSkillID, PassiveData>();
        PassiveData[] passiveDatas = Managers.Resource.LoadAll<PassiveData>("PassiveSkill");
        if (passiveDatas != null)
        {
            foreach (var data in passiveDatas)
            {
                if (!PassiveDict.ContainsKey(data.skillID))
                    PassiveDict.Add(data.skillID, data);
            }
        }
        
        //어빌리티 스킬 딕셔너리 채우기
        AbilityDict = new Dictionary<AbilityID, AbilityInstance>();
        AbilityData[] abilityDatas = Managers.Resource.LoadAll<AbilityData>("Ability");
        if (abilityDatas != null)
        {
            foreach (var data in abilityDatas)
            {
                if (!AbilityDict.ContainsKey(data.abilityID))
                    AbilityDict.Add(data.abilityID, new AbilityInstance(data));
            }
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
            _saveDataIndex = 4;
            for (int i = 1; i <= 4; i++)
            {
                if (!HasSaveFile(i))
                {
                    _saveDataIndex = i;
                    break;
                } 
            }
                
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

        // 슬롯에 표시될 텍스트 예시: "마지막 접속 시간 : 2023-10-27 15:30 \n 플레이 시간 : 00:00:00 \n 현재 골드 : 100"
        return $" 마지막 접속 시간 : {data.settings.lastSaveDate}\n 플레이 시간 : {GetFormattedPlayTime()}\n " +
               $"현재 골드 : {data.player.gold}";
    }
    
    public string GetFormattedPlayTime()
    {
        // 누적 시간 + 현재 세션 진행 시간
        float totalSeconds = SaveData.settings.playTime;
        
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



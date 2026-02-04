using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.AI.Navigation;
using static Define;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    public event Action ExitRoom;
    public event Action EnterRoom;
    
    
    public int StageCount = 10;
    [Range(0, 1f)]
    public float MonsterMapPercent = 0.5f;

    public int TotalGold;
    public float PlayTime;
    public int TotalKill;
    
    private List<List<RoomNode>> stageMap = new();
    private RoomNode currentRoomNode;
    private RoomNode lobyNode;
    private int currentDepth;
    private NavMeshSurface  surface;
    private Room currentRoom;
    private EnemySpawner enemySpawner;
    private int killCount;
    private Door currentExitDoor;
    private int doorIndex;
    private UI_BattleScene _battleUI;
    
    private WaitForSeconds waitForOne = new WaitForSeconds(1f);
    private WaitForSeconds waitForTwo = new WaitForSeconds(2f);
    private WaitForSeconds waitForHalf = new WaitForSeconds(0.5f);
    
    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@Map_Root");
            if (root == null)
                root = new GameObject { name = "@Map_Root" };
            return root;
        }
    }
    public void Init()
    {
        killCount = 0;
        
        // 이벤트 초기화
        ExitRoom -= ExitToNextRoom;
        ExitRoom += ExitToNextRoom;
        EnterRoom -= EnterToNextRoom;
        EnterRoom += EnterToNextRoom;

        
        lobyNode = new RoomNode { 
            index = -1, 
            type = RoomType.Loby,
            address = GetAddressByType(RoomType.Loby)
        };
        GenerateMap();
        surface =GameObject.Find("NavMesh").GetComponent<NavMeshSurface>();
        _battleUI = Managers.UI.LoadScene<UI_BattleScene>();
    }

    
    
    public void GenerateMap()
    {
        currentDepth = 0;
        stageMap.Clear();
        // 1. 노드 생성 (총 10단계)
        for (int i = 0; i < StageCount; i++)
        {
            List<RoomNode> layer = new List<RoomNode>();
            int roomCount = (i==0||i == StageCount-1) ? 1 : Random.Range(1, 4); //0층,9층은 방 1개

            for (int j = 0; j < roomCount; j++)
            {
                RoomNode node = new RoomNode { index = i };
                
                // 타입 결정
                if (i == 0) node.type = RoomType.Monster;
                else if(i==StageCount-1) node.type = RoomType.Boss;
                else node.type = (Random.value > MonsterMapPercent) ? RoomType.Event : RoomType.Monster;
                
                layer.Add(node);
            }
            stageMap.Add(layer);
        }

        // 2. 노드 간 연결 (이전 층과 다음 층 연결)
        for (int i = 0; i < stageMap.Count - 1; i++)
        {
            foreach (var curr in stageMap[i])
            {
                //다음 층 방의 개수에 맞는 문 개수의 방 주소
                curr.address = GetAddressByType(curr.type,stageMap[i + 1].Count);
                // 다음 층의 방들 중 최소 하나는 연결
                foreach (var next in stageMap[i + 1])
                {
                    curr.nextNodes.Add(next);
                }
            }
        }
        //보스맵 주소 바인딩
        stageMap[stageMap.Count - 1][0].address = GetAddressByType(RoomType.Boss);
        //로비방 로드
        lobyNode.nextNodes.Add(stageMap[0][0]);
        currentRoomNode = lobyNode;
        currentRoom = Managers.Resource.Instantiate(lobyNode.address,Vector3.zero,default,Root.transform).GetComponent<Room>();
    }
    

    public void ChangeRoom()
    {
        
        Managers.Resource.Destroy(currentRoom.gameObject);
        currentRoomNode = currentRoomNode.nextNodes[doorIndex];
        currentDepth++;
        TotalGold += currentDepth * 15;
        currentRoom = Managers.Resource.Instantiate(currentRoomNode.address, Vector3.zero,default,Root.transform).GetComponent<Room>();
        currentRoom.CloseImmediately();
        if (currentRoomNode.type == RoomType.Monster)
        {
            var enemyroom = currentRoom as EnemyRoom;
            if(enemyroom!=null)
                enemySpawner = enemyroom.Spawner;
        }
        else if (currentRoomNode.type == RoomType.Boss)
        {
            enemySpawner = null;
        }
        
        if (surface != null)
        {
            // 실시간으로 맵 데이터에 맞춰 NavMesh를 다시 계산합니다.
            surface.BuildNavMesh();
        }
        else
        {
            surface = GameObject.Find("NavMesh").GetComponent<NavMeshSurface>();
            surface?.BuildNavMesh();
        }

    }
    private string GetAddressByType(RoomType type,int count=1)
    {
        // 몬스터방 주소 리스트, 이벤트방 주소 리스트 중 랜덤 반환
        return type switch
        {
            RoomType.Monster => $"Assets/Prefabs/Map/MonsterMap/{count}DoorMonsterRoom_{Random.Range(1, 4)}.prefab" ,
            RoomType.Event => $"Assets/Prefabs/Map/{count}DoorEventRoom_1.prefab",
            RoomType.Boss => "Assets/Prefabs/Map/BossRoom_Final.prefab",
            RoomType.Loby => "Assets/Prefabs/Map/LobyMap.prefab",
            _ => ""
        };
    }

    public void OnExitRoom(Door exitDoor)
    {
        currentExitDoor = exitDoor;
        doorIndex = currentRoom.doors.IndexOf(exitDoor);
        ExitRoom?.Invoke();
    }

    public void ExitToNextRoom()
    {
        StartCoroutine(ExitToNextRoomCoroutine());
    }

    IEnumerator ExitToNextRoomCoroutine()
    {
        // 연출 시작
        Managers.Camera.ChanageCamera();
        Managers.Player.FadeMoveFloat(0.5f);
        
        if (_battleUI == null) _battleUI = Managers.UI.LoadScene<UI_BattleScene>();
        _battleUI.AllUIActive(false);
        
        var player = Managers.Player.Trans;
        player.DOMove(currentExitDoor.ExitPos.position, 1f);
        player.DORotate(currentExitDoor.dir, 1f);
        yield return waitForOne;

        // 문 밖으로 나가는 연출
        Vector3 targetPosition = player.position + (player.forward * 4f);
        player.DOMove(targetPosition, 1f).SetEase(Ease.Linear);
        yield return waitForOne;
        
        //다음 방 동적 교체 ---
        ChangeRoom();
        yield return waitForOne;
        

        // 플레이어를 새 방의 스폰 포인트로 순간이동
        player.position = currentRoom.SpawnPos.position;
        // ------------------------------

        yield return waitForOne;
        EnterRoom?.Invoke();
    }
    
    public void CheckClear()
    {
        
        if (currentRoomNode.type == RoomType.Monster)
        {
            TotalGold+= currentDepth*2;
            TotalKill++;
            killCount++;
            if (enemySpawner != null && killCount >= enemySpawner.SpawnCount)
            {
                ClearRoom();
                killCount = 0;
            }
        }
        else if (currentRoomNode.type == RoomType.Boss)
        {
            TotalGold += 30;
            TotalKill++;
            PlayTime = Time.time-PlayTime;
            StartCoroutine(BossClear());
        }
    }
    
    IEnumerator BossClear()
    {
        Managers.Player.BossClearControl(false);
        Time.timeScale = 0.2f;
        yield return waitForHalf;
        Time.timeScale = 1f;
        
        yield return waitForOne;
        var ui = Managers.UI.ShowPopupUI<UI_StageEnding>();
        ui.SetText(true,PlayTime,TotalKill,TotalGold);

    }
    public void ClearRoom()
    {
        foreach (var door in currentRoom.doors)
        {
            door.ExitRoomOpen();
        }
    }

    #region 방입장 함수

    public void EnterToNextRoom()
    {
        StartCoroutine(EnterToNextRoomCoroutine());
    }
    
    IEnumerator EnterToNextRoomCoroutine()
    {
        // 1. 방 타입에 따른 초기 설정 (Spawn / Boss 생성 / Event)
        Enemy03 boss = InitializeRoomContent();

        // 2. 플레이어 이동 및 연출
        yield return StartPlayerEntranceSequence();

        // 3. 카메라 및 보스 등장 연출
        if (currentRoomNode.type == RoomType.Boss)
        {
            yield return StartBossEncounterSequence(boss);
        }
        else
        {
            Managers.Camera.ChanageCamera();
            yield return waitForTwo;
        }

        // 4. UI 설정 및 전투 개시
        SetupBattleUI();
        
        Managers.Player.EnterRoom();
        _battleUI.SetMap(currentRoomNode.nextNodes, currentDepth);
    }
    

    private Enemy03 InitializeRoomContent()
    {
        switch (currentRoomNode.type)
        {
            case RoomType.Monster:
                int spawnCount = currentDepth * Random.Range(0, 2) + 5; 
                enemySpawner.SpawnCount = spawnCount;
                enemySpawner.SpawnEnemys();
                break;

            case RoomType.Boss:
                var bossObj = Managers.Resource.Instantiate(Address.Boss, currentRoom.BossPos.position,Quaternion.Euler(0,180,0));
                var boss = bossObj.GetComponent<Enemy03>();
                boss.gameObject.SetLayerRecursively("Default");
                return boss;

            case RoomType.Event:
                if (currentRoom is EventRoom eventRoom)
                    eventRoom.EventInit();
                break;
        }
        return null;
    }

    private IEnumerator StartPlayerEntranceSequence()
    {
        var enterDoor = currentRoom.EnterDoor;
        enterDoor.EnterRoomOpen();

        var player = Managers.Player.Trans;
        player.rotation = Quaternion.identity;
        
        // 이동 연출
        yield return player.DOMove(enterDoor.ExitPos.position, 2f).SetEase(Ease.Linear).WaitForCompletion();
        
        enterDoor.Close();
        Managers.Player.FadeMoveFloat(0);
        yield return waitForHalf;
    }

    private IEnumerator StartBossEncounterSequence(Enemy03 boss)
    {
        if (boss == null) yield break;

        Managers.Camera.SetBossCam(true);
        yield return waitForOne;
        
        boss.Rage();
        yield return waitForTwo;
        
        Managers.Camera.SetBossCam(false);
        boss.Init(3);
    }

    #endregion

    

    private void SetupBattleUI()
    {
        if (_battleUI == null) 
            _battleUI = Managers.UI.LoadScene<UI_BattleScene>();

        if (currentDepth == 1)
        {
            _battleUI.BattleInit();
            TotalGold = 0;
            TotalKill = 0;
            PlayTime = Time.time;
        }

        switch (currentRoomNode.type )
        {
            case  RoomType.Monster:
                _battleUI.BattleUIActive();
                enemySpawner.StartBattle();
                break;
            case  RoomType.Boss:
                _battleUI.BossUIActive();
                break;
            case  RoomType.Event:
                _battleUI.BattleUIActive();
                break;
            
        }

    }
    public void ReturnToLoby()
    {
        StartCoroutine(ReturnToLobyCoroutine());
    }

    IEnumerator ReturnToLobyCoroutine()
    {
        _battleUI.AllUIActive(false);
        _battleUI.FadeOut(1);
        yield return waitForOne;
        Managers.Resource.Destroy(currentRoom.gameObject);
        GenerateMap();
        Managers.Player.PlayerInit();
        yield return waitForOne;
        Managers.Player.BossClearControl(true);
        _battleUI.FadeIn(1);
        
        yield return waitForOne;
        _battleUI.LobyUIActive();
        Managers.Player.AddGold(TotalGold);
    }
    
    public void AddGold(int amount)
    {
        TotalGold += amount;
        Managers.UI.ShowFloatingText(Managers.Player.Trans.position, $"+{amount}gold", Color.yellow, 1.5f);
    }
}
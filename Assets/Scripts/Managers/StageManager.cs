using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;
using DG.Tweening;
using Unity.AI.Navigation;
using UnityEngine.AddressableAssets;
using static Define;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    public event Action ExitRoom;
    public event Action EnterRoom;

    public bool CanEsc = true;
    
    [Range(0, 1f)]
    public float MonsterMapPercent = 0.5f;

    private int stageGold;
    public int TotalGold;
    public float PlayTime;
    public int TotalKill;
    private List<List<RoomNode>> stageMap = new();
    private List<Coin> coins = new();
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
    
    private CancellationTokenSource _cts;
    private AudioSource audioSource;
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
    public async UniTask Init()
    {
        //킬수 초기화
        killCount = 0;
        
        // 이벤트 초기화
        ExitRoom -= ExitToNextRoom;
        ExitRoom += ExitToNextRoom;
        EnterRoom -= EnterToNextRoom;
        EnterRoom += EnterToNextRoom;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        
        lobyNode = new RoomNode { 
            index = -1, 
            type = RoomType.Loby,
            address = GetAddressByType(RoomType.Loby)
        };
        await GenerateMap();
        surface =GameObject.Find("NavMesh").GetComponent<NavMeshSurface>();
        _battleUI = Managers.UI.LoadScene<UI_BattleScene>();
    }

    
    
    public async UniTask GenerateMap()
    {
        currentDepth = 0;
        stageMap.Clear();
        var stageCount = Managers.Data.SpawnDic.Count;
        // 1. 노드 생성 (총 10단계)
        for (int i = 0; i < stageCount; i++)
        {
            List<RoomNode> layer = new List<RoomNode>();
            int roomCount = (i==0||i == stageCount-1) ? 1 : Random.Range(1, 4); //0층,9층은 방 1개

            for (int j = 0; j < roomCount; j++)
            {
                RoomNode node = new RoomNode { index = i };
                
                // 타입 결정
                if (i == 0) node.type = RoomType.Monster;
                else if(i==stageCount-1) node.type = RoomType.Boss;
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
        var go = await Managers.Resource.InstantiateAsync(lobyNode.address, Vector3.zero, default, Root.transform);
        currentRoom =go.GetComponent<Room>();
    }
    

    public async UniTask ChangeRoom()
    {
        if(enemySpawner!=null)
            enemySpawner.EnemyClear();
        Managers.Resource.Destroy(currentRoom.gameObject);
        currentRoomNode = currentRoomNode.nextNodes[doorIndex];
        currentDepth++;
        TotalGold += currentDepth * 15;
        var go = await Managers.Resource.InstantiateAsync(currentRoomNode.address, Vector3.zero, default,
            Root.transform);
        currentRoom = go.GetComponent<Room>();
        currentRoom.CloseImmediately();
        if (currentRoomNode.type == RoomType.Monster)
        {
            var enemyroom = currentRoom as EnemyRoom;
            if(enemyroom!=null)
                enemySpawner = enemyroom.Spawner;
            stageGold = 0;
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
        CanEsc = false;
        currentExitDoor = exitDoor;
        doorIndex = currentRoom.doors.IndexOf(exitDoor);
        ExitRoom?.Invoke();
    }

    private void ExitToNextRoom()
    {
        ExitToNextRoomAsync().Forget();
    }

    private async UniTask ExitToNextRoomAsync()
    {
        var token = _cts.Token; // 토큰 가져오기
        audioSource = await Managers.Sound.PlayEffectLoop(Managers.Resource.Data.PlayerWalk);
        // 1. 연출 시작 및 UI 초기화
        Managers.Camera.ChanageCamera();
        Managers.Player.FadeMoveFloat(0.5f);
        

        if (_battleUI == null) 
            _battleUI = Managers.UI.LoadScene<UI_BattleScene>();
    
        _battleUI.AllUIActive(false);

        var player = Managers.Player.Trans;
        try
        {
            // 2. 문 앞으로 이동 및 회전 (두 연출을 동시에 시작하고 모두 끝날 때까지 대기)
            await UniTask.WhenAll(
                player.DOMove(currentExitDoor.ExitPos.position, 1f).ToUniTask(cancellationToken: token),
                player.DORotate(currentExitDoor.dir, 1f).ToUniTask(cancellationToken: token)
            );
            //bgm 종료
            Managers.Sound.StopFade(Sound.Bgm);

            // 3. 문 밖으로 나가는 연출
            Vector3 targetPosition = player.position + (player.forward * 4f);
            // 이동이 끝날 때까지 대기
            await player.DOMove(targetPosition, 1f).SetEase(Ease.Linear).ToUniTask(cancellationToken: token);
            Managers.Sound.StopLoop(audioSource);
            // 4. 다음 방 동적 교체
            await ChangeRoom();
            // 방 교체 후 짧은 대기 (기존 waitForOne 대체)
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);

            // 5. 플레이어를 새 방의 스폰 포인트로 순간이동
            player.position = currentRoom.SpawnPos.position;

            // 6. 추가 대기 및 이벤트 호출
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
            
            EnterRoom?.Invoke();
        }
        catch (Exception e)
        {
            Console.WriteLine("ExitToNextRoom error: " + e);
            throw;
        }
        
    }

    public void CheckClear(int gold, Coin coin = null)
    {
        if (currentRoomNode.type == RoomType.Monster)
        {
            TotalGold += gold;
            stageGold += gold;
            TotalKill++;
            killCount++;
            if(coin != null)
                coins.Add(coin);
            var totalCount = Managers.Data.SpawnDic[currentDepth].TotalCount;
            if (enemySpawner != null && killCount >= totalCount)
            {
                ClearRoom();
                foreach (var temp in coins)
                {
                    temp.Clear();
                }
                coins.Clear();
                Invoke(nameof(DelayGetCoin),1f);
                
                killCount = 0;
            }
        }
        else if (currentRoomNode.type == RoomType.Boss)
        {
            
            TotalKill++;
            PlayTime = Time.time-PlayTime;
            BossClearAsync(gold).Forget();
        }
    }

    void DelayGetCoin()
    {
        Managers.Player.AddGold(stageGold);
        stageGold = 0;
    }
    
    //플레이어가 죽었을 때 코인 파괴
    void ClearCoin()
    {
        foreach (var coin in coins)
        {
            Managers.Resource.Destroy(coin.gameObject);
        }
        coins.Clear();
    }
    private async UniTaskVoid BossClearAsync(int gold)
    {
        var token = _cts.Token; // 토큰 가져오기
        try
        {
            // 1. 보스 클리어 연출 시작 (슬로우 모션)
            Managers.Player.BossClearControl(false);
            Time.timeScale = 0.2f;
            
            await UniTask.Delay(TimeSpan.FromSeconds(2f), delayType: DelayType.Realtime, cancellationToken: token);
            // 2. 1초 대기 (이때는 정상 시간 흐름)
            Time.timeScale = 1f;
            Managers.Player.AddGold(gold);
            
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
        
            // 3. UI 팝업 로드 및 대기 
            UI_StageEnding ui = await Managers.UI.ShowPopupUI<UI_StageEnding>();

            // 4. 결과 UI 텍스트 설정
            ui.SetText(true, PlayTime, TotalKill, TotalGold);
        }
        catch (Exception e)
        {
            Debug.LogError($"BossClear Error: {e.Message}");
        }
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
         EnterToNextRoomAsync().Forget();
    }

    private async UniTaskVoid EnterToNextRoomAsync()
    {
        var token = _cts.Token; // 토큰 가져오기
        try
        {
            audioSource = await Managers.Sound.PlayEffectLoop(Managers.Resource.Data.PlayerWalk);
            // 1. 방 타입에 따른 초기 설정 
            Enemy03 boss = await InitializeRoomContent();

            // 2. 플레이어 이동 및 연출
            Managers.Sound.StopLoop(audioSource,2);
            await StartPlayerEntranceSequence();
            
            // 3. 카메라 및 보스 등장 연출
            if (currentRoomNode.type == RoomType.Boss)
            {
                await StartBossEncounterSequence(boss);
            }
            else
            {
                Managers.Camera.ChanageCamera();
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token); // waitForTwo 대체
            }

            var data = Managers.Resource.Data;
            AssetReference assetReference = currentRoomNode.type switch
            {
                RoomType.Monster => data.OnBattleBGM,
                RoomType.Boss => data.BossMapBGM,
                RoomType.Event => data.EventRoomBGM,
                _ => null
            };
            if(currentRoomNode.type!=RoomType.Boss)
                Managers.Sound.PlayBgm(assetReference).Forget();
            _battleUI.SetMap(currentRoomNode.nextNodes, currentDepth);
            Managers.Player.EnterRoom();
            SetupBattleUI();
            CanEsc = true;
            
            
        }
        catch (OperationCanceledException)
        {
            // 취소 처리 
        }
        catch (Exception e)
        {
            Debug.LogError($"Room Transition Error: {e}");
        }
    }
    

    private async UniTask<Enemy03> InitializeRoomContent()
    {
        switch (currentRoomNode.type)
        {
            case RoomType.Monster:
                var spawncCount = Managers.Data.SpawnDic[currentDepth];
                SpawnEnemy(spawncCount);
                break;

            case RoomType.Boss:
                var data = Managers.Resource.Data;
                var bossObj = await Managers.Resource.InstantiateAsync(data.Boss, currentRoom.BossPos.position,Quaternion.Euler(0,180,0));
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

    private async UniTask StartPlayerEntranceSequence()
    {
        var token = _cts.Token; // 토큰 가져오기
        var enterDoor = currentRoom.EnterDoor;
        enterDoor.EnterRoomOpen();

        var player = Managers.Player.Trans;
        player.rotation = Quaternion.identity;
    
        // 이동 연출 (DOTween + UniTask)
        // ToUniTask()를 붙여주면 해당 트윈이 끝날 때까지 await 합니다.
        await player.DOMove(enterDoor.ExitPos.position, 2f)
            .SetEase(Ease.Linear)
            .ToUniTask(cancellationToken: token);
    
        enterDoor.Close();
        Managers.Player.FadeMoveFloat(0);

        // yield return waitForHalf 대체 (0.5초 대기)
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
    }

    private async UniTask StartBossEncounterSequence(Enemy03 boss)
    {
        var token = _cts.Token; // 토큰 가져오기
        if (boss == null) return;
        Managers.Sound.PlayBgm(Managers.Resource.Data.BossMapBGM).Forget();
        Managers.Camera.SetBossCam(true);
        
        await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token); 
        
        Managers.Sound.PlayEffect(Managers.Resource.Data.Enemy03Roar).Forget();
        
        await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token); 
        
        boss.Rage();
        
        await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token); 
    
        Managers.Camera.SetBossCam(false);
        await boss.Init(3); 
    }

    public void SpawnEnemy(SpawnCount spawnCount)
    {
        
        int[] counts = {
            spawnCount.Enemy01Count,
            spawnCount.Enemy02Count,
            spawnCount.Enemy03Count,
            spawnCount.Enemy04Count,
            spawnCount.Enemy05Count
        };

        for (int i = 0; i < counts.Length; i++)
        {
            int count = counts[i];
            int enemyId = i + 1;

            for (int j = 0; j < count; j++)
            {
                enemySpawner.SpawnEnemy(enemyId).Forget();
            }
        }
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
        ReturnToLobyAsync().Forget();
    }

    private async UniTask ReturnToLobyAsync()
    {
        var token = _cts.Token; // 토큰 가져오기
        try
        {
            // 1. 연출 시작: BGM 정지 및 UI 비활성화
            Managers.Sound.StopFade(Sound.Bgm);
            _battleUI.AllUIActive(false);
        
            // 페이드 아웃 시작 (1초 동안 진행된다고 가정)
            _battleUI.FadeOut(1);
        
            // 페이드 아웃이 진행되는 동안 1초 대기
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
            ClearCoin();
            enemySpawner.ReSetEnemy();
            // 2. 데이터 및 맵 재구성
            if (currentRoom != null)
            {
                Managers.Resource.Destroy(currentRoom.gameObject);
            }
            
            await GenerateMap();// 맵 생성
        
            // 맵 생성 후 짧은 안정화 대기
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);

            // 3. 로비 진입 연출
            Managers.Player.BossClearControl(true);
            _battleUI.FadeIn(1);
            Managers.Sound.PlayBgm(Managers.Resource.Data.StoreMapBGM).Forget();
        
            // 페이드 인 완료 대기
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);

            // 4. 로비 전용 UI 활성화
            _battleUI.LobyUIActive();
        }
        catch (Exception e)
        {
            Debug.LogError($"ReturnToLoby Error: {e.Message}");
        }
    }
    
    public void AddGold(int amount)
    {
        TotalGold += amount;
        Managers.Player.AddGold(amount);
    }

    public void Clear()
    {
        // 1. 모든 UniTask 중단
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        // 2. 이벤트 구독 해제 (매우 중요)
        ExitRoom = null;
        EnterRoom = null;

        // 3. 리스트 및 노드 데이터 초기화
        foreach (var layer in stageMap)
        {
            foreach (var node in layer)
            {
                node.nextNodes.Clear();
            }

            layer.Clear();
        }

        stageMap.Clear();

        // 4. 컴포넌트 및 외부 객체 참조 제거
        currentRoom = null;
        currentRoomNode = null;
        lobyNode = null;
        enemySpawner = null;
        currentExitDoor = null;
        _battleUI = null;
        surface = null;
        
        
        currentDepth = 0;
        killCount = 0;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static Define;
public class StageManager : MonoBehaviour
{
    public event System.Action ExitRoom;
    public event System.Action EnterRoom;

    [Header("Settings")]
    [SerializeField] private string[] roomAddresses; // Addressables에 등록된 방 프리팹 주소들
    
    private List<List<RoomNode>> stageMap = new List<List<RoomNode>>();
    private RoomNode currentRoomNode;
    private int currentDepth = 0;
    
    private Room currentRoom;
    private EnemySpawner enemySpawner;
    private int killCount;
    private Door currentExitDoor;

    public void Init()
    {
        killCount = 0;
        
        // 이벤트 초기화
        ExitRoom -= ExitToNextRoom;
        ExitRoom += ExitToNextRoom;
        EnterRoom -= EnterToNextRoom;
        EnterRoom += EnterToNextRoom;

        // 첫 번째 스테이지 생성
        SpawnRandomRoom();
    }

    private void SpawnRandomRoom(string address = null)
    {
        // 1. 기존 방이 있다면 제거 (ResourceManager의 Destroy 활용)
        if (currentRoom != null)
        {
            Managers.Resource.Destroy(currentRoom.gameObject);
        }

        string randomAddress;
        // 2. 랜덤 주소 선택
        if (address != null)
            randomAddress=address;
        else 
            randomAddress = roomAddresses[Random.Range(0, roomAddresses.Length)];

        // 3. ResourceManager를 통한 동적 생성
        // 위치는 (0,0,0)으로 고정하거나 필요에 따라 오프셋을 줄 수 있습니다.
        GameObject roomObj = Managers.Resource.Instantiate(randomAddress, Vector3.zero, Quaternion.identity);
        currentRoom = roomObj.GetComponent<Room>();
        
        // 4. 컴포넌트 캐싱
        enemySpawner = currentRoom.GetComponentInChildren<EnemySpawner>();
    }
    
    public void GenerateMap()
    {
        stageMap.Clear();
        // 1. 노드 생성 (총 10단계)
        for (int i = 0; i <= 9; i++)
        {
            List<RoomNode> layer = new List<RoomNode>();
            int doorCount = (i==0||i == 9) ? 1 : Random.Range(1, 4); //0층(로비),9층은 문 1개

            for (int j = 0; j < doorCount; j++)
            {
                RoomNode node = new RoomNode { index = i };
                
                // 타입 결정
                if (i == 0) node.type = RoomType.Monster;
                else if(i==9) node.type = RoomType.Boss;
                else node.type = (Random.value > 0.8f) ? RoomType.Event : RoomType.Monster;
                
                // 주소 할당 (Addressables)
                node.address = GetAddressByType(node.type,doorCount);
                layer.Add(node);
            }
            stageMap.Add(layer);
        }

        // 2. 노드 간 연결 (이전 층과 다음 층 연결)
        for (int i = 0; i < stageMap.Count - 1; i++)
        {
            foreach (var curr in stageMap[i])
            {
                // 다음 층의 방들 중 최소 하나는 연결
                foreach (var next in stageMap[i + 1])
                {
                    curr.nextNodes.Add(next);
                }
            }
        }
        
        currentRoomNode = stageMap[0][0];
    }

    private string GetAddressByType(RoomType type,int count)
    {
        // 몬스터방 주소 리스트, 이벤트방 주소 리스트 중 랜덤 반환
        return type switch
        {
            RoomType.Monster => $"{count}DoorMonsterRoom_" + Random.Range(1, 3),
            RoomType.Event => $"{count}EventRoom_1",
            RoomType.Boss => "BossRoom_Final",
            _ => ""
        };
    }

    public void OnExitRoom(Door exitDoor)
    {
        currentExitDoor = exitDoor;
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
        Managers.Player.PlayerAnim.SetFloat("MOVE", 0.5f);
        
        var player = Managers.Player.PlayerTrans;
        player.DOMove(currentExitDoor.ExitPos.position, 1f);
        player.DORotate(currentExitDoor.dir, 1f);
        yield return new WaitForSeconds(1f);

        // 문 밖으로 나가는 연출
        Vector3 targetPosition = player.position + (player.forward * 4f);
        player.DOMove(targetPosition, 1f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(1.1f);

        // --- 핵심: 다음 방 동적 교체 ---
        SpawnRandomRoom();

        // 플레이어를 새 방의 스폰 포인트로 순간이동
        player.position = currentRoom.SpawnPos.position;
        // ------------------------------

        yield return new WaitForSeconds(1f);
        EnterRoom?.Invoke();
    }

    public void CheckClear()
    {
        killCount++;
        if (enemySpawner != null && killCount >= enemySpawner.spawnCount)
        {
            ClearRoom();
            killCount = 0;
        }
    }

    public void ClearRoom()
    {
        foreach (var door in currentRoom.doors)
        {
            door.ExitRoomOpen();
        }
    }

    public void EnterToNextRoom()
    {
        StartCoroutine(EnterToNextRoomCoroutine());
    }

    IEnumerator EnterToNextRoomCoroutine()
    {
        enemySpawner.SpawnEnemys();

        var enterDoor = currentRoom.EnterDoor;
        enterDoor.EnterRoomOpen();

        var player = Managers.Player.PlayerTrans;
        player.rotation = Quaternion.identity;
        
        // 새 방의 입구 안쪽으로 이동
        player.DOMove(enterDoor.ExitPos.position, 2f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(2f);

        enterDoor.Close();
        Managers.Player.PlayerAnim.SetFloat("MOVE", 0f);
        yield return new WaitForSeconds(0.5f);

        Managers.Camera.ChanageCamera();
        yield return new WaitForSeconds(2f);
        
        enemySpawner.StartBattle();
        Managers.Player.EnterRoom();
    }
}
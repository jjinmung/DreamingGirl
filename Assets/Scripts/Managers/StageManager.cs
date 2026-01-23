using System;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public event Action ExitRoom;
    public event Action EnterRoom;
    
    private Room[] rooms;
    private int currentRoomIndex;
    private EnemySpawner enemySpawner;

    private int KillCount;

    public void Init()
    {
        rooms = GameObject.Find("@Rooms").GetComponentsInChildren<Room>();
        foreach (var room in rooms)
        {
                room.gameObject.SetActive(false);
        }

        currentRoomIndex = 0;
        rooms[currentRoomIndex].gameObject.SetActive(true);
        
        ExitRoom -= ExitToNextRoom;
        ExitRoom += ExitToNextRoom;

        EnterRoom -= EnterToNetxtRoom;
        EnterRoom += EnterToNetxtRoom;
        KillCount = 0;
    }

    private Door exitDoor;
    public void  OnExitRoom(Door ExitDoor)
    {
        exitDoor = ExitDoor;
        ExitRoom?.Invoke();
    }
    
    public void ExitToNextRoom()
    {
        StartCoroutine(ExitToNextRoomCoroutine());
    }

    IEnumerator ExitToNextRoomCoroutine()
    {
        Managers.Camera.ChanageCamera();
        Managers.Player.PlayerAnim.SetFloat("MOVE",0.5f);
        var player = Managers.Player.PlayerTrans;
        player.DOMove(exitDoor.ExitPos.position, 1f);
        player.DORotate(exitDoor.dir, 1f);
        yield return new WaitForSeconds(1f); 
        
        
        Vector3 targetPosition = player.position + (player.forward * 4f);
        player.DOMove(targetPosition, 1f)
            .SetEase(Ease.Linear); 
        

        yield return new WaitForSeconds(1f); 
        
        rooms[currentRoomIndex].gameObject.SetActive(false);
        
        currentRoomIndex++;

        if (currentRoomIndex < rooms.Length)
        {
            
            var nextRoom = rooms[currentRoomIndex];
            nextRoom.gameObject.SetActive(true);
            enemySpawner = nextRoom.GetComponentInChildren<EnemySpawner>();
            
            Transform spawnPoint = nextRoom.SpawnPos;
            Managers.Player.PlayerTrans.position = spawnPoint.position;
        }
        yield return new WaitForSeconds(1f); 
        EnterRoom.Invoke();
    }

    public void checkClear()
    {
        KillCount++;
        if (KillCount == enemySpawner.spawnCount)
        {
            ClearRoom();
            KillCount = 0;
        }
            
    }
    
    public void ClearRoom()
    {
        foreach (var door in rooms[currentRoomIndex].doors)
        {
            door.ExitRoomOpen();
        }
    }

    public void EnterToNetxtRoom()
    {
        StartCoroutine(EnterToNetxtRoomCoroutine());
    }
    IEnumerator EnterToNetxtRoomCoroutine()
    {
        enemySpawner.SpawnEnemys();
       
        Vector3 targetPosition = rooms[currentRoomIndex].EnterDoor.ExitPos.position;
        var currentRoom = rooms[currentRoomIndex];
        currentRoom.EnterDoor.EnterRoomOpen();
       
        var player = Managers.Player.PlayerTrans;
        player.rotation = Quaternion.Euler(0, 0, 0);
        player.DOMove(targetPosition, 2f)
            .SetEase(Ease.Linear); 
        
        yield return new WaitForSeconds(2f); 
        
        currentRoom.EnterDoor.Close();
        Managers.Player.PlayerAnim.SetFloat("MOVE",0f);
        yield return new WaitForSeconds(0.5f);
        
        Managers.Camera.ChanageCamera();
        
        yield return new WaitForSeconds(2f);
        enemySpawner.StartBattle();
        Managers.Player.EnterRoom();
    }
}

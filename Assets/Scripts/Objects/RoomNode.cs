using System.Collections.Generic;
using static Define;
public class RoomNode
{
    public int index;               // 방 번호 (깊이)
    public RoomType type;           // 방 종류
    public string address;          // Addressables 주소
    public List<RoomNode> nextNodes = new List<RoomNode>(); // 이 방에서 갈 수 있는 다음 방들
}
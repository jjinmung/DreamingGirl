using UnityEngine;

public class LobyMap : MonoBehaviour
{
    public float speed = 1f; // 이동 속도
    private Vector3 pos;
    
    private float minZ = -16f; // 최소 지점
    private float maxZ = 8f;   // 시작 지점
    
    private Animator anim;

    void Start()
    {
        pos = transform.position;
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. posZ를 시간에 따라 감소시킴
        pos.z -= speed * Time.deltaTime;

        // 2. -16보다 작아지면 8로 보냄
        if (pos.z <= minZ+0.1f)
        {
            pos.z = maxZ;
            if(anim != null)
                anim.SetTrigger("MoveMap");
        }

        // 3. 변경된 값을 적용
        transform.position = pos;
    }
}
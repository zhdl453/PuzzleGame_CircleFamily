using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public bool isDrag;
    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>(); //동글이의 Rigidbody2D를 가지고 옴
    }
    void Update()
    {//ScreenToWorldPoint: 스크린좌표를 월드좌표로 변환 
        if (isDrag) //드래그 할때만 적용해야함.
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //Input.mousePosition:마우스 클릭할때의 위치
                                                                                    //x축 경제 설정
            float leftBorder = -3.9f + transform.localScale.x / 2f; //크기 말하는거임.
            float rightBorder = 3.9f - transform.localScale.x / 2f; //크기 말하는거임.

            if (mousePos.x < leftBorder)
            {
                mousePos.x = leftBorder;
            }
            else if (mousePos.x > rightBorder)
            {
                mousePos.x = rightBorder;
            }

            mousePos.z = 0;
            mousePos.y = 7.5f;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f); //자신의 위치에 변환된 마우스 위치를 적용, 살짝 느슨하게 부드럽게 이동시키는게 Lerp()
        }
    }

    public void Drag()
    {
        isDrag = true;
    }
    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true; //동글이 Rigidbody2D의 simulated를 켜줌(중력발동)
    }
}

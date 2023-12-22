using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public int level;
    public bool isDrag;
    Rigidbody2D rigid;
    Animator anim; //애니메이터 변수 선언 및 코드로 초기화

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>(); //동글이의 Rigidbody2D를 가지고 옴
        anim = GetComponent<Animator>(); //동글이의 Animator를 가지고 옴
    }
    void OnEnable() //OnEnble: 동글이가 딱 태어났을때 애니메이션 발동시키고 싶음
    {//애니메이션 변수가 int로 되어있으니까 애니의 레벨 변수 가지고 오고싶으면 Animator.SetInteger()
        anim.SetInteger("Level", level);
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

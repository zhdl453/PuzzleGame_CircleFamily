using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public int level;
    public bool isDrag;
    public bool isMerge;
    Rigidbody2D rigid;
    CircleCollider2D circle;
    Animator anim; //애니메이터 변수 선언 및 코드로 초기화

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>(); //동글이의 Rigidbody2D를 가지고 옴
        anim = GetComponent<Animator>(); //동글이의 Animator를 가지고 옴
        circle = GetComponent<CircleCollider2D>(); //동글이의 CircleCollider2D를 가지고 옴
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
    //물리적 충돌 중일때 계속 실행되는 함수
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();
            //동글이 합치기 로직: 1:1로만 합치게 해야함. 그래서 다른 동글이 개입하지 않도록 잠금 역할 해주는 변수 추가할거임
            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            { //나와 상대편 위치 가져오기
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                //1.내가 아래에 있을때 || 2.동일한 높이일때 내가 오른쪽에 있을때
                if (meY < otherY || (meY == otherY && meX > otherX))
                {//상대방은 숨기고, 
                    other.Hide(transform.position); //상대방은 내쪽으로 흡수되니까 내꺼 동글이의 위치를 인자값으로 넣어줌
                    //나는 레벨업
                }

            }
        }
    }
    public void Hide(Vector3 targetPos)
    {
        isMerge = true;
        //흡수 이동을 위해 물리효과 모두 비활성화
        rigid.simulated = false;
        circle.enabled = false;
        //이동하는거 애니안하고 로직으로 할거라, 이동을 위한 코루틴 돌릴거임
        StartCoroutine(_HideRoutine(targetPos)); //인자값 문자열만 받는줄 알았는데 IEnumerator인터페이스도 받는구나..
    }

    IEnumerator _HideRoutine(Vector3 targetPos) //성장하는 상대에게 이동하므로 Vector3 매개변수 추가
    {
        int frameCount = 0;
        while (frameCount < 20) //while문으로 마치 Update처럼 로직을 실행하여 이동
        {
            frameCount += 1; //frameCount++;
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            yield return null;
        }

        isMerge = false;
        //73번줄에 other.Hide()로 되어있으니 gameObject는 저 other를 뜻하는 거임.
        gameObject.SetActive(false); //while문 끝나면 잠금해제하면서 오브젝트 비활성화
    }

}

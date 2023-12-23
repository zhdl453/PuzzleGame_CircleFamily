using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager manager; //퍼블릭변수는 잊지말고 게임매니저에서 초기화 해줘야함
    public ParticleSystem effect; //얘도 게임매니저에서 초기화해줘야 동글이가 사용가능
    public int level;
    public bool isDrag;
    public bool isMerge;
    public bool isAttach;
    public Rigidbody2D rigid;
    CircleCollider2D circle; //C#에서 클래스의 멤버 변수에 대해 접근 제한자를 명시적으로 선언하지 않으면, 기본적으로 private으로 설정됩니다. 
    Animator anim; //애니메이터 변수 선언 및 코드로 초기화
    SpriteRenderer spriteRenderer;
    float deadTime;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>(); //동글이의 Rigidbody2D를 가지고 옴
        anim = GetComponent<Animator>(); //동글이의 Animator를 가지고 옴
        circle = GetComponent<CircleCollider2D>(); //동글이의 CircleCollider2D를 가지고 옴
        spriteRenderer = GetComponent<SpriteRenderer>(); //동글이의 CircleCollider2D를 가지고 옴
    }
    void OnEnable() //OnEnble: 동글이가 딱 태어났을때 애니메이션 발동시키고 싶음
    {//애니메이션 변수가 int로 되어있으니까 애니의 레벨 변수 가지고 오고싶으면 Animator.SetInteger()
        anim.SetInteger("Level", level);
    }
    void OnDisable() //객체가 비활성화되면 발동하는 함수
    {
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        //동글 트랜스폼 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity; //0값 주는거임
        transform.localScale = Vector3.zero;

        //동글 물리 초기화
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true; //꺼져있던거 다시 켜줌.

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

    void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine("_AttachRoutine");
    }
    //충돌음 시간제한을 위한 코루틴 선언
    IEnumerator _AttachRoutine()
    {
        if (isAttach) //1.처음 부딪칠땐 isAttach가 정의안됐으니, 여긴 그냥 통과할거임.
        {
            yield break; //코루틴을 탈출하는 키워드는 yield break;
        }
        isAttach = true;
        manager.sfxPlay(GameManager.sfx.Attach);
        yield return new WaitForSeconds(0.2f);
        isAttach = false;
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
                    //나는 레벨업 시켜야함
                    LevelUp();
                }

            }
        }
    }
    public void Hide(Vector3 targetPos) //게임오버에도 이펙트가 나오도록 Hide함수에 로직추가
    {
        isMerge = true;
        //흡수 이동을 위해 물리효과 모두 비활성화
        rigid.simulated = false;
        circle.enabled = false;
        //이동하는거 애니안하고 로직으로 할거라, 이동을 위한 코루틴 돌릴거임
        if (targetPos == Vector3.up * 100)
        {
            EffectPlay();
        }
        StartCoroutine(_HideRoutine(targetPos)); //인자값 문자열만 받는줄 알았는데 IEnumerator인터페이스도 받는구나..
    }

    IEnumerator _HideRoutine(Vector3 targetPos) //성장하는 상대에게 이동하므로 Vector3 매개변수 추가
    {
        int frameCount = 0;
        while (frameCount < 20) //while문으로 마치 Update처럼 로직을 실행하여 이동
        {
            frameCount += 1; //frameCount++;
            if (targetPos != Vector3.up * 100)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }
            else if (targetPos == Vector3.up * 100)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f); //크기 0으로 만들어서 사라지게 만듦
            }

            yield return null;
        }
        manager.score += (int)Mathf.Pow(2, level);//지정 숫자(왼쪽)에다가 오른쪽float값 더해서 반환해주는 함수

        isMerge = false; //73번줄에 other.Hide()로 되어있으니 gameObject는 저 other를 뜻하는 거임.
        gameObject.SetActive(false); //while문 끝나면 잠금해제하면서 오브젝트 비활성화
    }

    void LevelUp()
    {
        isMerge = true;
        //합쳐질때 물리효과로 이동하면 조금 모양세가 이상할수있으니, 물리속도를 제거해줄거임
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;//회전속도

        StartCoroutine(_LevelUpRoutine());
    }
    IEnumerator _LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f); //0.2초정도, 상대방이 나한테 막 오는 시간정도가 좋을거같음
        anim.SetInteger("Level", level + 1);
        EffectPlay();
        manager.sfxPlay(GameManager.sfx.LevelUp);
        yield return new WaitForSeconds(0.3f); //애니매이션으로 커지는 속도 맞춰서 기다려주기
        //실제 레벨 상승을 늦게 해주는 이유는 애니메이션 시간떄문이다. 애니메이션이 실행이 되기도 전에 옆에 붙어있던 1레벨 더 큰 동글이가 있다면 바로 또 합쳐질것이다. 그래서 약간의 시간차 두는거임
        level += 1; //level ++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);//인자값 중에 초대값을 반환하는 함수. 나의레벨과 게임매니저의 maxlevel를 비교해서 큰 인자값을 int로 반환

        isMerge = false;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;
            if (deadTime > 2)
            {
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f); //원하는 색깔 원하면 이렇게 new Color(r, g, b)
            }
            if (deadTime > 5)
            {
                manager.GameOver();
            }
        }
    }
    //경계선 탈출시 로직 작성
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Finish")
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }
    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
}

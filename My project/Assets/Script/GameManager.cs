using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefeb;
    public Transform dongleGroup; //동글 그룹 오브젝트를 담을 변수 선언 및 초기화

    void Awake()
    {
        Application.targetFrameRate = 60; //프레임(FPS) 설정 속성
    }
    void Start()
    {
        NextDongle();
    }
    Dongle GetDongle()
    {//오브젝트를 새로 생성해주는 함수. 그래서 이 메소드 호출할때마다 클론이 하나씩 하나씩 생기는 거임
        GameObject instant = Instantiate(donglePrefeb, dongleGroup);
        Dongle instantDongle = instant.GetComponent<Dongle>(); //donglePrefeb 오브젝트는 Dongle스크립트에 있으니 가지고와(Dongle클래스가 이 스크립트 안에 있음)
        return instantDongle; //이제 클론객체 생성될때 dongleGroup를 부모객체로 해서 그 아래의 자식객체로 생길꺼임,그러면 생길때의 위치도 부모 기준으로 될거임
    }
    void NextDongle() //자기자신을 호출하는 재귀함수 작성은 계속 돌기때문에 조심해야함.유니티가 멈춰버림.
    {
        Dongle newDongle = GetDongle(); //Instantiate()에 의해 새로운 동글이 객체 나옴
        lastDongle = newDongle;
        lastDongle.level = Random.Range(0, 8);
        lastDongle.gameObject.SetActive(true); //동글이.cs가 OnEnble()될때 애니가 실행이 되니, 동글이 객체를 활성화 켜줘야 애니도 발동됨
        //lastDongle이 비워질때까지 기다려주는 뭔가 필요한데, 이때 코루틴함수:로직 제어(진행정도 모두를)를 유니티에게 맡기는 함수
        StartCoroutine("_WaitNext"); //코루틴 제어를 시작하기 위한 함수
    }

    IEnumerator _WaitNext() //IEnumerator: 열거형 인터페이스. 동글이 비워질때까지 기다리는 코루틴 생성
    {//유니티가 코루틴을 제어하기 위한 키워드. null로 반환하면 한 프레임을 쉬는 그런 구도가 되버림
        while (lastDongle != null) //yield없이 돌리면 무한루프 빠져 유니티가 멈춤.
        {
            yield return null;
        }
        yield return new WaitForSeconds(2.5f); //WaitForSeconds:시간(초) 단위로 기다리는 타입. 2.5f초를 쉬고 아래 조식을 실어야함.
        NextDongle();
    }

    public void TouchDown()
    {
        if (lastDongle == null)
        {//return값이 null이란 뜻
            return; //밑에 있는 lastDongle.Drag() 함수 실행이 되지 않고 그냥 여기에서 함수를 탈출해버림;
        }
        lastDongle.Drag();
    }
    public void TouchUp()
    {
        if (lastDongle == null)
        {
            return; //밑에 있는 lastDongle.Drag() 함수 실행이 되지 않고 그냥 여기에서 함수를 탈출해버림;
        }
        lastDongle.Drop();
        lastDongle = null;
    }
}

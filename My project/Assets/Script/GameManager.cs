using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefeb;
    public Transform dongleGroup; //동글 그룹 오브젝트를 담을 변수 선언 및 초기화

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

        //lastDongle이 비워질때까지 기다려주는 뭔가 필요한데, 이때 코루틴함수:로직 제어(진행정도 모두를)를 유니티에게 맡기는 함수
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

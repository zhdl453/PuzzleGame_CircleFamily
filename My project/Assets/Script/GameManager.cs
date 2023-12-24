using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; //namespace추가

public class GameManager : MonoBehaviour
{
    [Header("-----------[core]")]
    public int score;
    public int maxLevel;
    public bool isOver;

    [Header("-----------[Object Pooling]")]
    public Dongle lastDongle;
    public GameObject donglePrefeb;
    public Transform dongleGroup; //동글 그룹 오브젝트를 담을 변수 선언 및 초기화
    public List<Dongle> donglePool;
    public GameObject effectPrefeb;
    public Transform effectGroup; //동글 그룹 오브젝트를 담을 변수 선언 및 초기화
    public List<ParticleSystem> effectPool;
    [Range(1, 30)]//OnDisable 함수에서 각종 변수, 트랜스폼, 물리 초기화
    public int poolSize;
    public int poolCursor;

    [Header("-----------[Audio]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum sfx { LevelUp, Next, Attach, Button, Over }
    int sfxCursor;

    [Header("-----------[UI]")]
    public GameObject endGroup;
    public GameObject startGroup;
    public TMP_Text scoreText;
    public TMP_Text maxScoreText;
    public TMP_Text subScoreText;


    [Header("-----------[ETC]")]
    public GameObject line;
    public GameObject bottom;


    void Awake()
    {
        Application.targetFrameRate = 60; //프레임(FPS) 설정 속성
        donglePool = new List<Dongle>(); // 초기화
        effectPool = new List<ParticleSystem>(); // 초기화
        for (int i = 0; i < poolSize; i++)
        {
            MakeDongle();
        }
        if (!PlayerPrefs.HasKey("MaxScore"))//HashKey:저장된 데이터가 있는지 확인하는 함수
        {
            PlayerPrefs.SetInt("MaxScore", 0); //첨엔 이 0값으로 설정되고, 그값이 밑에 maxScoreText.text에 저장이 되겠지요
        }

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString(); //데이터 저장을 담당하는 클래스
    }
    public void GameStart()
    {
        //오브젝트 활성화
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();
        sfxPlay(sfx.Button);
        Invoke("NextDongle", 1.5f); //일반적인 함수에다가 딜레이 주고 싶으면 Invoke()함수를 쓰면 됨. NextDongle()함수 호출함
    }
    Dongle MakeDongle() //새로운 동글이 만들어주고 pool에 담아주는 함수
    {
        GameObject instantEffectObj = Instantiate(effectPrefeb, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        //동글이 생성
        GameObject instantDongleObj = Instantiate(donglePrefeb, dongleGroup);
        instantDongleObj.name = "Effect " + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>(); //donglePrefeb 오브젝트는 Dongle스크립트에 있으니 가지고와(Dongle클래스가 이 스크립트 안에 있음)
        instantDongle.manager = this; //동글이 생성하면서 manager,effect 변수를 같이 초기화
        instantDongle.effect = instantEffect;//동글 생성하면서 바로 이펙트 변수를 생성했던 것으로 초기화
        donglePool.Add(instantDongle);

        return instantDongle; //이제 클론객체 생성될때 dongleGroup를 부모객체로 해서 그 아래의 자식객체로 생길꺼임,그러면 생길때의 위치도 부모 기준으로 될거임
    }

    //오브젝트를 새로 생성해주는 함수. 그래서 이 메소드 호출할때마다 클론이 하나씩 하나씩 생기는 거임
    Dongle GetDongle() //pool에 담아진 동글이 사용하는 함수, 없으면 MakeDongle()한테 만들어달라함
    {
        for (int i = 0; i < donglePool.Count; i++)
        {
            poolCursor = (poolCursor + 1) % donglePool.Count; //donglePool.Count을 넘지 않고 계속 반복될거임
            if (!donglePool[poolCursor].gameObject.activeSelf)
            {
                return donglePool[poolCursor];
            }
        }
        return MakeDongle(); //만약에 모든 동글이가 비활성화라 줄게 없다면 안되니, MakeDongle()함수 불러서 동글이 반환함
    }

    //자기자신을 호출하는 재귀함수 작성은 계속 돌기때문에 조심해야함.유니티가 멈춰버림.
    void NextDongle()
    {
        if (isOver)
        {
            return;
        }
        lastDongle = GetDongle();
        //이 코드를 지우고 실행하면, Dongle 객체는 GameManager에 대한 참조를 가지고 있지 않아서 GameManager의 메소드나 변수에 접근할 수 없게되서 참조 꼭 해줘야함
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true); //동글이.cs가 OnEnble()될때 애니가 실행이 되니, 동글이 객체를 활성화 켜줘야 애니도 발동됨
        //lastDongle이 비워질때까지 기다려주는 뭔가 필요한데, 이때 코루틴함수:로직 제어(진행정도 모두를)를 유니티에게 맡기는 함수
        sfxPlay(sfx.Next);
        StartCoroutine("_WaitNext"); //코루틴 제어를 시작하기 위한 함수
    }

    //IEnumerator: 열거형 인터페이스. 동글이 비워질때까지 기다리는 코루틴 생성
    IEnumerator _WaitNext()
    {//유니티가 코루틴을 제어하기 위한 키워드. null로 반환하면 한 프레임을 쉬는 그런 구도가 되버림
        while (lastDongle != null) //yield없이 돌리면 무한루프 빠져 유니티가 멈춤.
        {
            yield return null;
        }
        //lastDongle이 null되면서 NextDonlgle이 발생. 게임오버할때는 못들고오게 해야함.
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

    public void GameOver()
    {
        if (isOver)//2.이게 그다음 실행되면서 함수탈출
        {
            return;
        }
        isOver = true; //1.이게 먼저 실행되고


        StartCoroutine("_GameOverRoutine");
    }

    IEnumerator _GameOverRoutine()
    {
        //1.장면 안에 활성화 되어있는 모든 동글 가져오기
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        //2.지우기 전에 모든 동글의 물리효과 비활성화
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].rigid.simulated = false;//아래 동글이 사라지면 무너지면서(3번) 의도치않은 합치기 발생할수있으니 다 꺼줘야함
        }

        //3.1번의 목록을 하나씩 접근해서 지우기
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f); //시간 딜레이 주면서 동글이들 사라지게
        }
        bgmPlayer.Stop();
        //최고 점수 갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);
        //게임오버 UI표시
        yield return new WaitForSeconds(0.5f);
        endGroup.SetActive(true);
        subScoreText.text = "Score : " + scoreText.text;
        yield return new WaitForSeconds(0.5f);
        sfxPlay(sfx.Over);
    }
    public void Reset()
    {
        sfxPlay(sfx.Button);
        StartCoroutine("_ResetCoroutine");
    }
    IEnumerator _ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }

    public void sfxPlay(sfx type)
    {
        switch (type)
        {
            case sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }
        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length; //계속 0,1,2가 될거임
    }
    void Update() //모바일에 나가는 기능을 위해 Update에서 로직 추가
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }

    void LateUpdate() // Update종료 후 실행되는 생명주기 함수
    {
        scoreText.text = score.ToString();
    }


}



// # Systems
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;




// # Unity
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public enum TurnState
{
    None,
    Ready,
    PlayerCommanding,
    PlayerAttacking,
    EnemyCommanding,
    EnemyAttacking,
}

public class TickManager : MonoBehaviour
{
    // time
    private double currentTime = 0d; // 한박자씩 0으로 초기화됨
    private double realCurrentTime = 0d; // 반박자씩 0으로 초기화됨
    private double absoluteTime = 0d; // 오차 없는 절대 시간

    // music
    [SerializeField] private int bpm = 120;

    // 오차범위
    private double criticalTolerance = 0.05d; // 허용 오차 범위 0.05 == 50ms
    private double strikeTolerance = 0.125d;
    private double hitTolerance = 0.2d;

    // 원
    private int circleWaitTickOne = 2; // 원을 때리기 전이 몇 틱 전인지 정의
    private int circleWaitTickTwo = 4;
    private double circleWaitTimeOne; 
    private double circleWaitTimeTwo; // 원을 보여주기 전 시점과 타이밍의 차이를 계산
    private int attackWaitTime = 8; // 박자를 누르고 나서 얼마나 기다리는지의 값

    private double enemyCircleWaitTime;

    // 틱
    private int tickCount = 0; // 증가하는 틱 카운트
    public int TickCount => tickCount; // 증가하는 틱 카운트

    // 정박 타이밍들
    // 1, 3, 5, 7, 준비마디
    // 9, 11, 13, 15, 커맨드마디
    // 17, 19, 21, 23, 공격마디
    // 25, 27, 29, 31, 적 커맨드마디? ~
    // 33, 35, 37, 39 적 공격마디~

    // 턴
    private TurnState turnState = TurnState.None;
    private bool changeSkillCasterFlag = false;
    private int attackCharacterIndexCount = 0;

    private double aBeat;
    private double realbeat;

    private Skill castingSkill;

    private int attackCommandBeforeWaitCount;
    private int attackCommandAfterWaitCount;

    private Queue<Arrow> arrowQueue = new Queue<Arrow>();
    private Dictionary<int, Arrow> tickDict = new Dictionary<int, Arrow>();

    private void Start()
    {
        aBeat = (60d / bpm);
        realbeat = (30d / bpm); // 반박자를 세기위함

        circleWaitTimeTwo = aBeat * circleWaitTickTwo;
        circleWaitTimeOne = aBeat * circleWaitTickOne;
    }

    private void Update()
    {
        absoluteTime = Time.time;
        realCurrentTime = absoluteTime % realbeat;
        currentTime = absoluteTime % aBeat;

        // 박자가 끝나고 뿜!! 새로 시작하는 시점
        if (realCurrentTime < Time.deltaTime)
        {
            CalculationTurn();

            if(tickCount % 2 != 0)
            {
                // 정박이라면 (홀수가 정박)
                GameManager.Instance.BounceAnimation();
            }
        }
    }

    private void CalculationTurn()
    {
        if (tickCount == 16)
        {
            turnState = TurnState.PlayerAttacking;
            attackCharacterIndexCount = 0;
        }

        tickCount++;

        // 플레이어의 어택커맨드 타이밍인 5,6,7,8의 1초 전에 미리 서클애니메이션 진행
        if(tickCount == 5)
        {
            tickDict.Add(tickCount + circleWaitTickOne, Arrow.None);
            GameManager.Instance.PlayPartyTimingCircle(0, realCurrentTime, circleWaitTimeOne, Arrow.Up, TimingCircleType.Command, tickCount + circleWaitTickOne);
        }
        if (tickCount == 7)
        {
            tickDict.Add(tickCount + circleWaitTickOne, Arrow.None);
            GameManager.Instance.PlayPartyTimingCircle(1, realCurrentTime, circleWaitTimeOne, Arrow.Up, TimingCircleType.Command, tickCount + circleWaitTickOne);
        }
        if (tickCount == 9)
        {
            tickDict.Add(tickCount + circleWaitTickOne, Arrow.None);
            GameManager.Instance.PlayPartyTimingCircle(2, realCurrentTime, circleWaitTimeOne, Arrow.Up, TimingCircleType.Command, tickCount + circleWaitTickOne);
        }
        if (tickCount == 11)
        {
            tickDict.Add(tickCount + circleWaitTickOne, Arrow.None);
            GameManager.Instance.PlayPartyTimingCircle(3, realCurrentTime, circleWaitTimeOne, Arrow.Up, TimingCircleType.Command, tickCount + circleWaitTickOne);
        }

        if(turnState == TurnState.PlayerAttacking)
        {
            PartyCommandAttack();
        }
    }
   
    private void PartyCommandAttack()
    {
        if (attackCommandAfterWaitCount > 0)
        {
            attackCommandAfterWaitCount--;
            return;
        }

        if (!changeSkillCasterFlag)
        {
            GameManager.Instance.SetNowSkillCaster(ref castingSkill, attackCharacterIndexCount);

            if (GameManager.Instance.NowSkillCaster == null)
            {
                GameManager.Instance.ZoomOutCharacter();
                GameManager.Instance.ZoomOutTargets();
                turnState = TurnState.EnemyCommanding;
                return;
            }

            if(castingSkill != null)
            {
                GameManager.Instance.ZoomOutCharacter();
                GameManager.Instance.ZoomInCharacter(castingSkill);

                GameManager.Instance.ZoomOutTargets();
                GameManager.Instance.ZoomInTargets(castingSkill);

                castingSkill.GetSkillCommandList(ref arrowQueue);
            }

            changeSkillCasterFlag = true;
            attackCommandBeforeWaitCount = 4;
        }

        if(attackCommandBeforeWaitCount > 0)
        {
            attackCommandBeforeWaitCount--;
            return;
        }

        if (castingSkill != null)
        {
            Arrow arrow = arrowQueue.Dequeue();
            if (arrow != Arrow.None)
            {
                tickDict.Add(tickCount + circleWaitTickOne, arrow);
                GameManager.Instance.PlayPartyTimingCircle(attackCharacterIndexCount, realCurrentTime, circleWaitTimeOne, arrow, TimingCircleType.Attack, tickCount + circleWaitTickOne);
            }
        }
        else
        {
             GameManager.Instance.ShowCommandFailed(attackCharacterIndexCount);
        }

        if (arrowQueue.Count == 0)
        {
            changeSkillCasterFlag = false;
            attackCharacterIndexCount++;

            if(castingSkill == null)
                return;

            if(tickCount % 2 != 0)
            {
                attackCommandAfterWaitCount = 5;
            }
            else
            {
                attackCommandAfterWaitCount = 4;
            }
        }
    }

    public Accuracy GetAccuracy(int targetTick)
    {
        if (!tickDict.ContainsKey(targetTick))
        {
            return Accuracy.Miss;
        }
      
        if (tickCount < targetTick - 1)
        {
            return Accuracy.Miss;
        }

        double time = 0;
        double beat = 0;
        if(targetTick % 2 != 0)
        {
            time = currentTime;
            beat = aBeat;
        }
        else
        {
            time = realCurrentTime;
            beat = realbeat;
        }
        // 5/5가 다음 틱이라면
        // Mathf.Abs((float)(currentTime - 60d / bpm)) 다음 틱에 얼마나 근접햇는지 4/5
        // Mathf.Abs((float)currentTime) <= tolerance 이전틱에서 얼마나 지나쳤는지 6/5

        if (Mathf.Abs((float)time) <= criticalTolerance || Mathf.Abs((float)(time - beat)) <= criticalTolerance)
        {
            return Accuracy.Critical;
        }
        else if (Mathf.Abs((float)time) <= strikeTolerance || Mathf.Abs((float)(time - beat)) <= strikeTolerance)
        {
            return Accuracy.Strike;
        }
        else if (Mathf.Abs((float)time) <= hitTolerance || Mathf.Abs((float)(time - beat)) <= hitTolerance)
        {
            return Accuracy.Hit;
        }

        return Accuracy.Miss;
    }

    public Arrow GetNowArrowInTick(int targetTick)
    {
        if (!tickDict.ContainsKey(targetTick))
        {
            return Arrow.None;
        }
        else
        {
            return tickDict[targetTick];
        }
    }
}

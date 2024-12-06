// # Systems
using System.Collections;
using System.Collections.Generic;
using System.Linq;


// # Unity
using UnityEngine;
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
    private double currentTime = 0d;
    private double realCurrentTime = 0d;
    private double absoluteTime = 0d; // 오차 없는 절대 시간

    // music
    [SerializeField] private int bpm = 120;

    // 오차범위
    private double criticalTolerance = 0.05d; // 허용 오차 범위 0.05 == 50ms
    private double strikeTolerance = 0.15d;
    private double hitTolerance = 0.3d;

    // 원
    private int circleWaitTick = 2; // 원을 때리기 전이 몇 틱 전인지 정의
    private double circleWaitTime; // 원을 보여주기 전 시점과 타이밍의 차이를 계산
    private int attackWaitTime = 8; // 박자를 누르고 나서 얼마나 기다리는지의 값

    private double enemyCircleWaitTime;

    // 틱
    private int tickCount = 0; // 증가하는 틱 카운트
    public int TickCount => tickCount; // 증가하는 틱 카운트
    private int maxTickCount;

    // 정박 타이밍들
    // 1, 3, 5, 7, 준비마디
    // 9, 11, 13, 15, 커맨드마디
    // 17, 19, 21, 23, 공격마디
    // 25, 27, 29, 31, 적 커맨드마디? ~
    // 33, 35, 37, 39 적 공격마디~

    private int[] partyCommandTicks = { 9, 11, 13, 15 }; // 파티가 커맨드를 입력하는 시점(틱)

    private int[] enemyAttackTicks; // 무조건 29 이상부터 (그래야 반응이 쉬움)

    // 턴
    private TurnState turnState = TurnState.None;

    private double aBeat;
    private double realbeat;

    private void Start()
    {
        aBeat = (60d / bpm);
        realbeat = (30d / bpm); // 반박자를 세기위함

        circleWaitTime = aBeat * circleWaitTick;
        enemyCircleWaitTime = aBeat;
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
                // 정박이라면
                GameManager.Instance.BounceAnimation();
            }
        }
    }

    private void CalculationTurn()
    {
        //if (tickCount == enemyAttackTicks[enemyAttackTicks.Length - 1])
        //{
        //    // 이전 틱이 모든 틱의 마지막 틱이었다면?
        //    turnState = TurnState.None;
        //    GameManager.Instance.SortPartyMember();
        //    tickCount = 0;
        //}
        //else if (tickCount == partyCommandTicks[partyCommandTicks.Length - 1] + attackWaitTime) // 파티의 어택 틱들이 모두 끝났다면
        //{
        //    // 이전 틱이 마지막 공격 틱이었다면?
        //    // 적의 공격을 받을 준비
        //    turnState = TurnState.EnemyCommanding;
        //    //GameManager.Instance.SortEnemyMember(ref enemyAttackTicks);
        //}

        tickCount++;

        #region Command
        // 플레이어의 어택커맨드 타이밍인 5,6,7,8의 1초 전에 미리 서클애니메이션 진행
        if(tickCount == partyCommandTicks[0] - 4)
        {
            GameManager.Instance.PlayPartyTimingCircle(0, currentTime, circleWaitTime);
        }
        else if (tickCount == partyCommandTicks[1] - 4)
        {
            GameManager.Instance.PlayPartyTimingCircle(1, currentTime, circleWaitTime);
        }
        else if (tickCount == partyCommandTicks[2] - 4)
        {
            GameManager.Instance.PlayPartyTimingCircle(2, currentTime, circleWaitTime);
        }
        else if (tickCount == partyCommandTicks[3] - 4)
        {
            GameManager.Instance.PlayPartyTimingCircle(3, currentTime, circleWaitTime);
        }

        // 타이밍을 늦어서 못누른채 반박자가 넘어간다면 Miss가 떠야함
        if (tickCount == partyCommandTicks[0] +2 && GameManager.Instance.PressCount == 0)
        {
            GameManager.Instance.PressedKey(KeyIndexInArray.None);
        }
        else if (tickCount == partyCommandTicks[1] + 2 && GameManager.Instance.PressCount == 1)
        {
            GameManager.Instance.PressedKey(KeyIndexInArray.None);
        }
        else if (tickCount == partyCommandTicks[2] + 2 && GameManager.Instance.PressCount == 2)
        {
            GameManager.Instance.PressedKey(KeyIndexInArray.None);
        }
        else if (tickCount == partyCommandTicks[3] +2 && GameManager.Instance.PressCount == 3)
        {
            GameManager.Instance.PressedKey(KeyIndexInArray.None);
        }
        #endregion

        #region Attack
        if (tickCount == partyCommandTicks[0] + attackWaitTime)
        {
            GameManager.Instance.AttackPartyMember(0);
        }
        else if (tickCount == partyCommandTicks[1] + attackWaitTime)
        {
            GameManager.Instance.AttackPartyMember(1);
        }
        else if (tickCount == partyCommandTicks[2] + attackWaitTime)
        {
            GameManager.Instance.AttackPartyMember(2);
        }
        else if (tickCount == partyCommandTicks[3] + attackWaitTime)
        {
            GameManager.Instance.AttackPartyMember(3);
        }
        #endregion

        #region Guard
        // 플레이어의 가드커맨드 타이밍인 5,6,7,8의 1초 전에 미리 서클애니메이션 진행
        //if (tickCount == enemyAttackTicks[0] - circleWaitTick)
        //{
        //    GameManager.Instance.PlayPartyGuardCircle(0, currentTime, circleWaitTime);
        //}
        //else if (tickCount == enemyAttackTicks[1] - circleWaitTick)
        //{
        //    GameManager.Instance.PlayPartyGuardCircle(1, currentTime, circleWaitTime);
        //}
        //else if (tickCount == enemyAttackTicks[2] - circleWaitTick)
        //{
        //    GameManager.Instance.PlayPartyGuardCircle(2, currentTime, circleWaitTime);
        //}
        //else if (tickCount == enemyAttackTicks[3] - circleWaitTick)
        //{
        //    GameManager.Instance.PlayPartyGuardCircle(3, currentTime, circleWaitTime);
        //}
        #endregion
    }

    public Accuracy GetAccuracy(int targetTick)
    {
        if (tickCount < targetTick - 1) return Accuracy.Miss;
        // 5/5가 다음 틱이라면
        // Mathf.Abs((float)(currentTime - 60d / bpm)) 다음 틱에 얼마나 근접햇는지 4/5
        // Mathf.Abs((float)currentTime) <= tolerance 이전틱에서 얼마나 지나쳤는지 6/5

        if (Mathf.Abs((float)realCurrentTime) <= criticalTolerance || Mathf.Abs((float)(realCurrentTime - realbeat)) <= criticalTolerance)
        {
            return Accuracy.Critical;
        }
        else if (Mathf.Abs((float)realCurrentTime) <= strikeTolerance || Mathf.Abs((float)(realCurrentTime - realbeat)) <= strikeTolerance)
        {
            return Accuracy.Strike;
        }
        else if (Mathf.Abs((float)realCurrentTime) <= hitTolerance || Mathf.Abs((float)(realCurrentTime - realbeat)) <= hitTolerance)
        {
            return Accuracy.Hit;
        }

        return Accuracy.Miss;
    }
}

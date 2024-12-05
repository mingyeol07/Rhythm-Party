// # Systems
using System.Collections;
using System.Collections.Generic;
using System.Linq;


// # Unity
using UnityEngine;

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
    public double CurrentTime => currentTime;
    private double absoluteTime = 0d; // 오차 없는 절대 시간

    // music
    [SerializeField] private int bpm = 120;
    private int stageBeat = 4; // 4/4 박자라면 4,  6/8 박자라면 6

    // 오차범위
    private double criticalTolerance = 0.05d; // 허용 오차 범위 0.05 == 50ms
    private double strikeTolerance = 0.15d;
    private double hitTolerance = 0.3d;

    // 원
    private int circleWaitTick = 2; // 원을 보여주기 전이 몇 틱 전인지 정의
    private double circleWaitTime; // 원을 보여주기 전 시점과 타이밍의 차이를 계산

    // 틱
    private int tickCount = 0; // 증가하는 틱 카운트

    private int[] partyCommandTicks = { 5, 6, 7, 8 }; // 파티가 커맨드를 입력하는 시점(틱)
    private int[] partyAttackTicks = { 9, 10, 11, 12 }; // 파티가 커맨드를 입력하는 시점(틱)
    private int[] enemyCommandTicks = { 13, 14, 15, 16 };
    private int[] enemyAttackTicks = { 17, 18, 19, 20 };

    private int maxTickCount = 20; // 사이클의 마지막 틱

    // 턴
    private TurnState turnState = TurnState.None;

    private void Start()
    {
        circleWaitTime = (60d / bpm) * circleWaitTick;
    }

    private void Update()
    {
        absoluteTime = Time.time;
        currentTime = absoluteTime % (60d / bpm);

        // 박자가 끝나고 뿜!! 새로 시작하는 시점
        if (currentTime < Time.deltaTime)
        {
            GameManager.Instance.BounceAnimation();

            CalculationTurn();
        }
    }

    private void CalculationTurn()
    {
        // 한 루프의 끝까지 갔다면? 
        // 파티를 정비! (리스트를 스피드순서대로 정렬)
        if (tickCount == maxTickCount)
        {
            turnState = TurnState.None;
            attackCount = 0;
            GameManager.Instance.SortPartyMember();
            tickCount = 0;
        }
        else if (tickCount == partyAttackTicks[partyAttackTicks.Length - 1]) // 파티의 어택 틱들이 모두 끝났다면
        {
            turnState = TurnState.EnemyCommanding;
            //GameManager.Instance.SortEnemyMember();
        }

        tickCount++;

        #region Command
        // 플레이어의 어택커맨드 타이밍인 5,6,7,8의 1초 전에 미리 서클애니메이션 진행
        if (tickCount == partyCommandTicks[0] - circleWaitTick)
        {
            GameManager.Instance.PlayPartyTimingCircle(0, currentTime, circleWaitTime, 5);
        }
        else if (tickCount == partyCommandTicks[1] - circleWaitTick)
        {
            GameManager.Instance.PlayPartyTimingCircle(1, currentTime, circleWaitTime, 6);
        }
        else if (tickCount == partyCommandTicks[2] - circleWaitTick)
        {
            GameManager.Instance.PlayPartyTimingCircle(2, currentTime, circleWaitTime, 7);
        }
        else if (tickCount == partyCommandTicks[3] - circleWaitTick)
        {
            GameManager.Instance.PlayPartyTimingCircle(3, currentTime, circleWaitTime, 8);
        }
        #endregion


        #region Attack
        // 플레이어의 어택커맨드 타이밍인 5,6,7,8의 1초 전에 미리 서클애니메이션 진행

        else if (tickCount == partyAttackTicks[3] + 1)
        {
            GameManager.Instance.PlayPartyReBounce();
        }
        #endregion

        #region Guard
        // 플레이어의 가드커맨드 타이밍인 5,6,7,8의 1초 전에 미리 서클애니메이션 진행
        if (tickCount == enemyAttackTicks[0] - circleWaitTick)
        {
            GameManager.Instance.PlayPartyGuardCircle(0, currentTime, circleWaitTime);
        }
        else if (tickCount == enemyAttackTicks[1] - circleWaitTick)
        {
            GameManager.Instance.PlayPartyGuardCircle(1, currentTime, circleWaitTime);
        }
        else if (tickCount == enemyAttackTicks[2] - circleWaitTick)
        {
            GameManager.Instance.PlayPartyGuardCircle(2, currentTime, circleWaitTime);
        }
        else if (tickCount == enemyAttackTicks[3] - circleWaitTick)
        {
            GameManager.Instance.PlayPartyGuardCircle(3, currentTime, circleWaitTime);
        }
        #endregion
    }

    public Accuracy GetAccuracy(int targetTick)
    {
        if(tickCount < targetTick - 1) return Accuracy.Miss;
        // 5/5가 다음 틱이라면
        // Mathf.Abs((float)(currentTime - 60d / bpm)) 다음 틱에 얼마나 근접햇는지 4/5
        // Mathf.Abs((float)currentTime) <= tolerance 이전틱에서 얼마나 지나쳤는지 6/5

        if (Mathf.Abs((float)currentTime) <= criticalTolerance || Mathf.Abs((float)(currentTime - 60d / bpm)) <= criticalTolerance)
        {
            return Accuracy.Critical;
        }
        else if (Mathf.Abs((float)currentTime) <= strikeTolerance || Mathf.Abs((float)(currentTime - 60d / bpm)) <= strikeTolerance)
        {
            return Accuracy.Strike;
        }
        else if (Mathf.Abs((float)currentTime) <= hitTolerance || Mathf.Abs((float)(currentTime - 60d / bpm)) <= hitTolerance)
        {
            return Accuracy.Hit;
        }

        return Accuracy.Miss;
    }
}

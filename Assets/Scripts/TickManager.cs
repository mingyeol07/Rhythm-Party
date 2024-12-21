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

    private int[] partyCommandTicks = { 9, 11, 13, 15 }; // 파티가 커맨드를 입력하는 시점(틱)

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
    private Dictionary<int, bool> tickDict = new Dictionary<int, bool>();

    private void Start()
    {
        aBeat = (60d / bpm);
        realbeat = (30d / bpm); // 반박자를 세기위함

        circleWaitTimeTwo = aBeat * circleWaitTickTwo;
        circleWaitTimeOne = aBeat * circleWaitTickOne;
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
        //if(tickCount == 24)
        //{
        //    // 적의 공격을 준비함
        //    GameManager.Instance.PlayPartyReBounce();
        //    GameManager.Instance.SortEnemyMember(ref enemyAttackTicks);
        //}
        if (tickCount == 16)
        {
            turnState = TurnState.PlayerAttacking;
            attackCharacterIndexCount = 0;
        }

        tickCount++;

        #region Command
        // 플레이어의 어택커맨드 타이밍인 5,6,7,8의 1초 전에 미리 서클애니메이션 진행
        if(tickCount == 5)
        {
            tickDict.Add(tickCount + circleWaitTickOne, true);
            GameManager.Instance.PlayPartyTimingCircle(0, realCurrentTime, circleWaitTimeOne, Arrow.Up, tickCount + circleWaitTickOne);
        }
        if (tickCount == 7)
        {
            tickDict.Add(tickCount + circleWaitTickOne, true);
            GameManager.Instance.PlayPartyTimingCircle(1, realCurrentTime, circleWaitTimeOne, Arrow.Up, tickCount + circleWaitTickOne);
        }
        if (tickCount == 9)
        {
            tickDict.Add(tickCount + circleWaitTickOne, true);
            GameManager.Instance.PlayPartyTimingCircle(2, realCurrentTime, circleWaitTimeOne, Arrow.Up, tickCount + circleWaitTickOne);
        }
        if (tickCount == 11)
        {
            tickDict.Add(tickCount + circleWaitTickOne, true);
            GameManager.Instance.PlayPartyTimingCircle(3, realCurrentTime, circleWaitTimeOne, Arrow.Up, tickCount + circleWaitTickOne);
        }

        if(turnState == TurnState.PlayerAttacking)
        {
            PartyCommandAttack();
        }

        // 타이밍을 늦어서 못누른채 반박자가 넘어간다면 Miss가 떠야함
        //if (tickCount == partyCommandTicks[0] +2 && GameManager.Instance.PressCount == 0)
        //{
        //    GameManager.Instance.PressedKey(KeyIndexInArray.None);
        //}
        //else if (tickCount == partyCommandTicks[1] + 2 && GameManager.Instance.PressCount == 1)
        //{
        //    GameManager.Instance.PressedKey(KeyIndexInArray.None);
        //}
        //else if (tickCount == partyCommandTicks[2] + 2 && GameManager.Instance.PressCount == 2)
        //{
        //    GameManager.Instance.PressedKey(KeyIndexInArray.None);
        //}
        //else if (tickCount == partyCommandTicks[3] +2 && GameManager.Instance.PressCount == 3)
        //{
        //    GameManager.Instance.PressedKey(KeyIndexInArray.None);
        //}
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

        if(tickCount > 24)
        {
            CheckEnemyAttackTick();
        }

        //if (tickCount == enemyAttackTicks[0] + attackWaitTime)
        //{
        //    GameManager.Instance.AttackPartyMember(0);
        //}
        //else if (tickCount == partyCommandTicks[1] + attackWaitTime)
        //{
        //    GameManager.Instance.AttackPartyMember(1);
        //}
        //else if (tickCount == partyCommandTicks[2] + attackWaitTime)
        //{
        //    GameManager.Instance.AttackPartyMember(2);
        //}
        //else if (tickCount == partyCommandTicks[3] + attackWaitTime)
        //{
        //    GameManager.Instance.AttackPartyMember(3);
        //}
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

            GameManager.Instance.ZoomOutCharacter();
            GameManager.Instance.ZoomInCharacter(castingSkill);

            GameManager.Instance.ZoomOutTargets();
            GameManager.Instance.ZoomInTargets(castingSkill);

            castingSkill.GetSkillCommandList(ref arrowQueue);

            changeSkillCasterFlag = true;
            attackCommandBeforeWaitCount = 4;
        }

        if(attackCommandBeforeWaitCount > 0)
        {
            attackCommandBeforeWaitCount--;
            return;
        }

        Arrow arrow = arrowQueue.Dequeue();
        if(arrow != Arrow.None)
        {
            tickDict.Add(tickCount + circleWaitTickOne, true);
            GameManager.Instance.PlayPartyTimingCircle(attackCharacterIndexCount, realCurrentTime, circleWaitTimeOne, arrow, tickCount + circleWaitTickOne);
        }

        if (arrowQueue.Count == 0)
        {
            changeSkillCasterFlag = false;
            attackCharacterIndexCount++;

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

    private void CheckEnemyAttackTick()
    {

    }

    public Accuracy GetAccuracy(int targetTick)
    {
        Debug.Log(targetTick);

        if (!tickDict.ContainsKey(targetTick))
        {
            return Accuracy.Miss;
        }
      
        if (tickCount < targetTick - 1)
        {
            return Accuracy.Miss;
        }

        // 5/5가 다음 틱이라면
        // Mathf.Abs((float)(currentTime - 60d / bpm)) 다음 틱에 얼마나 근접햇는지 4/5
        // Mathf.Abs((float)currentTime) <= tolerance 이전틱에서 얼마나 지나쳤는지 6/5

        if (Mathf.Abs((float)currentTime) <= criticalTolerance || Mathf.Abs((float)(currentTime - aBeat)) <= criticalTolerance)
        {
            return Accuracy.Critical;
        }
        else if (Mathf.Abs((float)currentTime) <= strikeTolerance || Mathf.Abs((float)(currentTime - aBeat)) <= strikeTolerance)
        {
            return Accuracy.Strike;
        }
        else if (Mathf.Abs((float)currentTime) <= hitTolerance || Mathf.Abs((float)(currentTime - aBeat)) <= hitTolerance)
        {
            return Accuracy.Hit;
        }

        return Accuracy.Miss;
    }
}

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
    private double absoluteTime = 0d; // ���� ���� ���� �ð�

    // music
    [SerializeField] private int bpm = 120;

    // ��������
    private double criticalTolerance = 0.05d; // ��� ���� ���� 0.05 == 50ms
    private double strikeTolerance = 0.15d;
    private double hitTolerance = 0.3d;

    // ��
    private int circleWaitTickOne = 2; // ���� ������ ���� �� ƽ ������ ����
    private int circleWaitTickTwo = 4;
    private double circleWaitTimeOne; 
    private double circleWaitTimeTwo; // ���� �����ֱ� �� ������ Ÿ�̹��� ���̸� ���
    private int attackWaitTime = 8; // ���ڸ� ������ ���� �󸶳� ��ٸ������� ��

    private double enemyCircleWaitTime;

    // ƽ
    private int tickCount = 0; // �����ϴ� ƽ ī��Ʈ
    public int TickCount => tickCount; // �����ϴ� ƽ ī��Ʈ

    // ���� Ÿ�ֵ̹�
    // 1, 3, 5, 7, �غ񸶵�
    // 9, 11, 13, 15, Ŀ�ǵ帶��
    // 17, 19, 21, 23, ���ݸ���
    // 25, 27, 29, 31, �� Ŀ�ǵ帶��? ~
    // 33, 35, 37, 39 �� ���ݸ���~

    private int[] partyCommandTicks = { 9, 11, 13, 15 }; // ��Ƽ�� Ŀ�ǵ带 �Է��ϴ� ����(ƽ)

    // ��
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
        realbeat = (30d / bpm); // �ݹ��ڸ� ��������

        circleWaitTimeTwo = aBeat * circleWaitTickTwo;
        circleWaitTimeOne = aBeat * circleWaitTickOne;
        enemyCircleWaitTime = aBeat;
    }

    private void Update()
    {
        absoluteTime = Time.time;
        realCurrentTime = absoluteTime % realbeat;
        currentTime = absoluteTime % aBeat;

        // ���ڰ� ������ ��!! ���� �����ϴ� ����
        if (realCurrentTime < Time.deltaTime)
        {
            CalculationTurn();

            if(tickCount % 2 != 0)
            {
                // �����̶��
                GameManager.Instance.BounceAnimation();
            }
        }
    }

    private void CalculationTurn()
    {
        //if (tickCount == enemyAttackTicks[enemyAttackTicks.Length - 1])
        //{
        //    // ���� ƽ�� ��� ƽ�� ������ ƽ�̾��ٸ�?
        //    turnState = TurnState.None;
        //    GameManager.Instance.SortPartyMember();
        //    tickCount = 0;
        //}
        //else if (tickCount == partyCommandTicks[partyCommandTicks.Length - 1] + attackWaitTime) // ��Ƽ�� ���� ƽ���� ��� �����ٸ�
        //{
        //    // ���� ƽ�� ������ ���� ƽ�̾��ٸ�?
        //    // ���� ������ ���� �غ�
        //    turnState = TurnState.EnemyCommanding;
        //    //GameManager.Instance.SortEnemyMember(ref enemyAttackTicks);
        //}
        //if(tickCount == 24)
        //{
        //    // ���� ������ �غ���
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
        // �÷��̾��� ����Ŀ�ǵ� Ÿ�̹��� 5,6,7,8�� 1�� ���� �̸� ��Ŭ�ִϸ��̼� ����
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

        // Ÿ�̹��� �ʾ ������ä �ݹ��ڰ� �Ѿ�ٸ� Miss�� ������
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

        // 5/5�� ���� ƽ�̶��
        // Mathf.Abs((float)(currentTime - 60d / bpm)) ���� ƽ�� �󸶳� �����޴��� 4/5
        // Mathf.Abs((float)currentTime) <= tolerance ����ƽ���� �󸶳� �����ƴ��� 6/5

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

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
    private double absoluteTime = 0d; // ���� ���� ���� �ð�

    // music
    [SerializeField] private int bpm = 120;

    // ��������
    private double criticalTolerance = 0.05d; // ��� ���� ���� 0.05 == 50ms
    private double strikeTolerance = 0.15d;
    private double hitTolerance = 0.3d;

    // ��
    private int circleWaitTick = 2; // ���� ������ ���� �� ƽ ������ ����
    private double circleWaitTime; // ���� �����ֱ� �� ������ Ÿ�̹��� ���̸� ���
    private int attackWaitTime = 8; // ���ڸ� ������ ���� �󸶳� ��ٸ������� ��

    private double enemyCircleWaitTime;

    // ƽ
    private int tickCount = 0; // �����ϴ� ƽ ī��Ʈ
    public int TickCount => tickCount; // �����ϴ� ƽ ī��Ʈ
    private int maxTickCount;

    // ���� Ÿ�ֵ̹�
    // 1, 3, 5, 7, �غ񸶵�
    // 9, 11, 13, 15, Ŀ�ǵ帶��
    // 17, 19, 21, 23, ���ݸ���
    // 25, 27, 29, 31, �� Ŀ�ǵ帶��? ~
    // 33, 35, 37, 39 �� ���ݸ���~

    private int[] partyCommandTicks = { 9, 11, 13, 15 }; // ��Ƽ�� Ŀ�ǵ带 �Է��ϴ� ����(ƽ)

    private int[] enemyAttackTicks; // ������ 29 �̻���� (�׷��� ������ ����)

    // ��
    private TurnState turnState = TurnState.None;

    private double aBeat;
    private double realbeat;

    private void Start()
    {
        aBeat = (60d / bpm);
        realbeat = (30d / bpm); // �ݹ��ڸ� ��������

        circleWaitTime = aBeat * circleWaitTick;
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

        tickCount++;

        #region Command
        // �÷��̾��� ����Ŀ�ǵ� Ÿ�̹��� 5,6,7,8�� 1�� ���� �̸� ��Ŭ�ִϸ��̼� ����
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

        // Ÿ�̹��� �ʾ ������ä �ݹ��ڰ� �Ѿ�ٸ� Miss�� ������
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
        // �÷��̾��� ����Ŀ�ǵ� Ÿ�̹��� 5,6,7,8�� 1�� ���� �̸� ��Ŭ�ִϸ��̼� ����
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
        // 5/5�� ���� ƽ�̶��
        // Mathf.Abs((float)(currentTime - 60d / bpm)) ���� ƽ�� �󸶳� �����޴��� 4/5
        // Mathf.Abs((float)currentTime) <= tolerance ����ƽ���� �󸶳� �����ƴ��� 6/5

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

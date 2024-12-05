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
    private double absoluteTime = 0d; // ���� ���� ���� �ð�

    // music
    [SerializeField] private int bpm = 120;
    private int stageBeat = 4; // 4/4 ���ڶ�� 4,  6/8 ���ڶ�� 6

    // ��������
    private double criticalTolerance = 0.05d; // ��� ���� ���� 0.05 == 50ms
    private double strikeTolerance = 0.15d;
    private double hitTolerance = 0.3d;

    // ��
    private int circleWaitTick = 2; // ���� �����ֱ� ���� �� ƽ ������ ����
    private double circleWaitTime; // ���� �����ֱ� �� ������ Ÿ�̹��� ���̸� ���

    // ƽ
    private int tickCount = 0; // �����ϴ� ƽ ī��Ʈ

    private int[] partyCommandTicks = { 5, 6, 7, 8 }; // ��Ƽ�� Ŀ�ǵ带 �Է��ϴ� ����(ƽ)
    private int[] partyAttackTicks = { 9, 10, 11, 12 }; // ��Ƽ�� Ŀ�ǵ带 �Է��ϴ� ����(ƽ)
    private int[] enemyCommandTicks = { 13, 14, 15, 16 };
    private int[] enemyAttackTicks = { 17, 18, 19, 20 };

    private int maxTickCount = 20; // ����Ŭ�� ������ ƽ

    // ��
    private TurnState turnState = TurnState.None;

    private void Start()
    {
        circleWaitTime = (60d / bpm) * circleWaitTick;
    }

    private void Update()
    {
        absoluteTime = Time.time;
        currentTime = absoluteTime % (60d / bpm);

        // ���ڰ� ������ ��!! ���� �����ϴ� ����
        if (currentTime < Time.deltaTime)
        {
            GameManager.Instance.BounceAnimation();

            CalculationTurn();
        }
    }

    private void CalculationTurn()
    {
        // �� ������ ������ ���ٸ�? 
        // ��Ƽ�� ����! (����Ʈ�� ���ǵ������� ����)
        if (tickCount == maxTickCount)
        {
            turnState = TurnState.None;
            attackCount = 0;
            GameManager.Instance.SortPartyMember();
            tickCount = 0;
        }
        else if (tickCount == partyAttackTicks[partyAttackTicks.Length - 1]) // ��Ƽ�� ���� ƽ���� ��� �����ٸ�
        {
            turnState = TurnState.EnemyCommanding;
            //GameManager.Instance.SortEnemyMember();
        }

        tickCount++;

        #region Command
        // �÷��̾��� ����Ŀ�ǵ� Ÿ�̹��� 5,6,7,8�� 1�� ���� �̸� ��Ŭ�ִϸ��̼� ����
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
        // �÷��̾��� ����Ŀ�ǵ� Ÿ�̹��� 5,6,7,8�� 1�� ���� �̸� ��Ŭ�ִϸ��̼� ����

        else if (tickCount == partyAttackTicks[3] + 1)
        {
            GameManager.Instance.PlayPartyReBounce();
        }
        #endregion

        #region Guard
        // �÷��̾��� ����Ŀ�ǵ� Ÿ�̹��� 5,6,7,8�� 1�� ���� �̸� ��Ŭ�ִϸ��̼� ����
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
        // 5/5�� ���� ƽ�̶��
        // Mathf.Abs((float)(currentTime - 60d / bpm)) ���� ƽ�� �󸶳� �����޴��� 4/5
        // Mathf.Abs((float)currentTime) <= tolerance ����ƽ���� �󸶳� �����ƴ��� 6/5

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

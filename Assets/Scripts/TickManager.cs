// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public enum TurnState
{
    None,
    Commanding,
    Attacking
}

public class TickManager : MonoBehaviour
{
    private int stageBeat = 4; // 4/4 ���ڶ�� 4,  6/8 ���ڶ�� 6

    [SerializeField] private int bpm = 120;
    private double currentTime = 0d;
    public double CurrentTime => currentTime;
    private double absoluteTime = 0d; // ���� ���� ���� �ð�

    private double tolerance = 0.1d; // ��� ���� ���� 0.05 == 50ms

    private int turnCount;
    private int maxTurnCount = 5;

    private TurnState turnState = TurnState.None;

    private void Start()
    {
        turnState = TurnState.Commanding;
    }

    private void Update()
    {
        absoluteTime = Time.time;
        currentTime = absoluteTime % (60d / bpm);

        if (currentTime < Time.deltaTime)
        {
            GameManager.Instance.BounceAnimation();

            if (turnCount == maxTurnCount)
            {
                turnCount = 0;

                turnState = (turnState == TurnState.Commanding)
                    ? TurnState.Attacking
                    : TurnState.Commanding;
            }
            turnCount++;
        }
    }

    public bool IsPerfect()
    {
        if (Mathf.Abs((float)currentTime) <= tolerance || Mathf.Abs((float)(currentTime - 60d / bpm)) <= tolerance)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 1, 2, 3, 4 �����߿� �ϳ��� �Ű������� �ް� �� ������ �ش��ϴ� ���� ������ Ÿ���� ��ȯ
    /// </summary>
    public double GetSequenceTime(int sequence)
    {
        double nextTime = 0;

        return nextTime;
    }
}

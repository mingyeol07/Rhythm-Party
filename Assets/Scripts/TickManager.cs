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
    private int stageBeat = 4; // 4/4 박자라면 4,  6/8 박자라면 6

    [SerializeField] private int bpm = 120;
    private double currentTime = 0d;
    public double CurrentTime => currentTime;
    private double absoluteTime = 0d; // 오차 없는 절대 시간

    private double tolerance = 0.1d; // 허용 오차 범위 0.05 == 50ms

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
    /// 1, 2, 3, 4 순서중에 하나를 매개변수로 받고 그 순서에 해당하는 다음 박자의 타임을 반환
    /// </summary>
    public double GetSequenceTime(int sequence)
    {
        double nextTime = 0;

        return nextTime;
    }
}

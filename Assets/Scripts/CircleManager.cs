using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public enum Accuracy
{
    None,
    Critical,
    Strike,
    Hit,
    Miss,
}

public class CircleManager : MonoBehaviour
{
    [Header("ReduceCircle")]
    [SerializeField] private CircleSpawner[] circleSpawners;
    private Queue<ReduceCircle> skillCircleQueue = new Queue<ReduceCircle>();
    public Queue<ReduceCircle> SkillCircleQueue => skillCircleQueue;

    private void Awake()
    {
        for(int i =0; i < circleSpawners.Length; i++)
        {
            circleSpawners[i].SetManager(this);
        }
    }

    public CircleSpawner GetCircleSpawner(Arrow arrow)
    {
        return circleSpawners[(int)arrow];
    }

    public void SpawnReduceCircle(double currentTime, double nextTime, Arrow arrow, CircleType type, int targetTick)
    {
        circleSpawners[(int)arrow].SpawnCircle(currentTime, nextTime, arrow, type, targetTick);
    }

    public void PressedCommand(Accuracy accuracy)
    {
        StartCoroutine(circleSpawners[(int)Arrow.Up].PressedCircle());
        circleSpawners[(int)Arrow.Up].ShowText(accuracy);
    }

    public void PressedAttackCommand(Accuracy accuracy, Arrow arrow)
    {
        CircleSpawner spawner = circleSpawners[(int)arrow];
        if (spawner.ReduceCircleQueue.Count > 0)
        {
            StartCoroutine(spawner.PressedCircle());
            spawner.ShowText(accuracy);
        }
        else
        {
            spawner.ShowText(Accuracy.Miss);
        }
    }
}
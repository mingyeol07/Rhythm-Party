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

    public void SpawnReduceCircle(double currentTime, double nextTime, Arrow arrow, bool isGuardTiming = false )
    {
        circleSpawners[(int)arrow].SpawnCircle(currentTime, nextTime, isGuardTiming);
    }

    public void PressedCommand(Accuracy accuracy)
    {
        circleSpawners[(int)Arrow.Up].PressedCircle();
        circleSpawners[(int)Arrow.Up].ShowText(accuracy);
    }

    public void PressedAttack(Accuracy accuracy, Arrow arrow)
    {
        CircleSpawner spawner = circleSpawners[(int)arrow];
        if (spawner.ReduceCircleQueue.Count > 0)
        {
            spawner.PressedCircle();
            spawner.ShowText(accuracy);
        }
        else
        {
            spawner.ShowText(Accuracy.Miss);
        }
    }
}
// # Systems
using System.Collections;
using System.Collections.Generic;
using TMPro;


// # Unity
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CircleSpawner : MonoBehaviour
{
    private Queue<ReduceCircle> reduceCircleQueue = new Queue<ReduceCircle>();
    public Queue<ReduceCircle> ReduceCircleQueue => reduceCircleQueue;

    [SerializeField] private GameObject reduceCirclePrefab;
    [SerializeField] private ReduceCircle timingCircle;
    private Transform reduceCircleParent;

    [Header("UI")]
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private Animator accuracyAnimator;

    private void Awake()
    {
        reduceCircleParent = transform.GetChild(1).transform;
    }

    public void SpawnCircle(double currentTime, double endTime, bool isGuardTiming, int targetTick)
    {
        if(reduceCircleQueue.Count ==0)
        {
            AppearTimingCircle(isGuardTiming);
        }

        // 줄어드는 원 생성
        GameObject circle = Instantiate(reduceCirclePrefab, reduceCircleParent);
        ReduceCircle reduceCircle = circle.GetComponent<ReduceCircle>();
        reduceCircle.SetTargetTick(targetTick);
        reduceCircleQueue.Enqueue(reduceCircle);
        reduceCircle.SetSpawner(this);

        // 원 줄어들기 코루틴
        StartCoroutine(reduceCircle.Co_StartReduce(currentTime, endTime));
    }

    private void AppearTimingCircle(bool isGuardTiming)
    {
        if(isGuardTiming)
        {
            timingCircle.CircleMaterial.color = Color.cyan;
        }
        else
        {
            timingCircle.CircleMaterial.color = Color.red;
        }
        StartCoroutine(timingCircle.Co_Appear());
    }

    public void VanishTimingCircle()
    {
        StartCoroutine(timingCircle.Co_Vanish());
    }

    public void PressedCircle()
    {
        ReduceCircle circle = reduceCircleQueue.Dequeue();
        if (reduceCircleQueue.Count == 0)
        {
            VanishTimingCircle();
        }
        StartCoroutine(circle.Co_Vanish());
    }

    public void ShowText(Accuracy accuracy)
    {
        switch (accuracy)
        {
            case Accuracy.Critical:
                accuracyText.text = "CRITICAL!";
                break;
            case Accuracy.Strike:
                accuracyText.text = "STRIKE";
                break;
            case Accuracy.Hit:
                accuracyText.text = "HIT";
                break;
            case Accuracy.Miss:
                accuracyText.text = "MISS";
                break;
        }
        accuracyAnimator.SetTrigger("Play");
    }
}

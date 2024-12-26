// # Systems
using System.Collections;
using System.Collections.Generic;
using TMPro;


// # Unity
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public enum TimingType
{
    None,
    Command,
    Attack,
    AttackCharge,
    Guard,
    GuardCharge,
}

public class CircleSpawner : MonoBehaviour
{
    private Queue<ReduceCircle> reduceCircleQueue = new Queue<ReduceCircle>();
    public Queue<ReduceCircle> ReduceCircleQueue => reduceCircleQueue;

    private CircleManager circleManager;
    public CircleManager CircleManager => circleManager;

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

    public void SetManager(CircleManager manager)
    {
        circleManager = manager;
    }    

    public void SpawnCircle(double currentTime, double endTime, Arrow arrow, TimingType type, int targetTick, int circleSpawnCount)
    {
        if(reduceCircleQueue.Count ==0)
        {
            AppearTimingCircle(type);
        }

        // 줄어드는 원 생성
        GameObject circle = Instantiate(reduceCirclePrefab, reduceCircleParent);
        ReduceCircle reduceCircle = circle.GetComponent<ReduceCircle>();

        StartCoroutine(reduceCircle.Co_Appear());
        reduceCircle.Init(this, targetTick, arrow, circleSpawnCount);

        //circleManager.SkillCircleQueue.Enqueue(reduceCircle);
        reduceCircleQueue.Enqueue(reduceCircle);

        // 원 줄어들기 코루틴
        StartCoroutine(reduceCircle.Co_StartReduce(currentTime, endTime));
    }

    private void AppearTimingCircle(TimingType type)
    {
        switch(type)
        {
            case TimingType.Command:
                timingCircle.CircleMaterial.color = Color.yellow;
                break;
            case TimingType.Attack:
                timingCircle.CircleMaterial.color = Color.red;
                break;
            case TimingType.AttackCharge:
                timingCircle.CircleMaterial.color = Color.gray;
                break;
            case TimingType.Guard:
                timingCircle.CircleMaterial.color = Color.cyan;
                break;
            case TimingType.GuardCharge:
                timingCircle.CircleMaterial.color = Color.gray;
                break;
        }

        StartCoroutine(timingCircle.Co_Appear());
    }

    public void VanishTimingCircle()
    {
        StartCoroutine(timingCircle.Co_Vanish());
    }

    public void PressedCircle()
    {
        //circleManager.SkillCircleQueue.Dequeue();
        ReduceCircle circle = reduceCircleQueue.Dequeue();

        if (reduceCircleQueue.Count == 0)
        {
            VanishTimingCircle();
        }

        StartCoroutine(circle.Clicked());
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

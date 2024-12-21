// # Systems
using System.Collections;
using System.Collections.Generic;
using TMPro;


// # Unity
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public enum CircleType
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

    public void SpawnCircle(double currentTime, double endTime, Arrow arrow, CircleType type, int targetTick)
    {
        if(reduceCircleQueue.Count ==0)
        {
            AppearTimingCircle(type);
        }

        // 줄어드는 원 생성
        GameObject circle = Instantiate(reduceCirclePrefab, reduceCircleParent);
        ReduceCircle reduceCircle = circle.GetComponent<ReduceCircle>();

        reduceCircle.Init(this, targetTick, arrow);

        circleManager.SkillCircleQueue.Enqueue(reduceCircle);
        reduceCircleQueue.Enqueue(reduceCircle);

        // 원 줄어들기 코루틴
        StartCoroutine(reduceCircle.Co_StartReduce(currentTime, endTime));
    }

    private void AppearTimingCircle(CircleType type)
    {
        switch(type)
        {
            case CircleType.Command:
                timingCircle.CircleMaterial.color = Color.yellow;
                break;
            case CircleType.Attack:
                timingCircle.CircleMaterial.color = Color.red;
                break;
            case CircleType.AttackCharge:
                timingCircle.CircleMaterial.color = Color.gray;
                break;
            case CircleType.Guard:
                timingCircle.CircleMaterial.color = Color.cyan;
                break;
            case CircleType.GuardCharge:
                timingCircle.CircleMaterial.color = Color.gray;
                break;
        }

        StartCoroutine(timingCircle.Co_Appear());
    }

    public void VanishTimingCircle()
    {
        StartCoroutine(timingCircle.Co_Vanish());
    }

    public IEnumerator PressedCircle()
    {
        circleManager.SkillCircleQueue.Dequeue();
        ReduceCircle circle = reduceCircleQueue.Dequeue();
        if (reduceCircleQueue.Count == 0)
        {
            VanishTimingCircle();
        }

        yield return StartCoroutine(circle.Co_Vanish());
        Destroy(circle.gameObject);
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

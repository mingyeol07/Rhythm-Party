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

public class TimingCircleSpawner : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Character character;

    [Header("ReduceCircle")]
    [SerializeField] private GameObject reduceCirclePrefab;
    [SerializeField] private Transform reduceCircleParent;
    [SerializeField] private GameObject timingCirclePrefab;
    [SerializeField] private Transform timingCircleParent;

    // 커맨드로 입력해야할 줄어드는 서클들이 차례대로 들어왔다가 나가는 큐
    private Queue<ReduceCircle> reduceCricleQueue = new Queue<ReduceCircle>();
    public Queue<ReduceCircle> ReduceCricleQueue => reduceCricleQueue;

    [Header("UI")]
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private Animator accuracyAnimator;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        character = GetComponentInParent<Character>();
    }

    private void OnDestroy()
    {
    }

    public void SpawnReduceCircle(double currentTime, double nextTime, bool isGuardTiming = false, Arrow arrow = Arrow.Up)
    {
        GameObject timingCricle = Instantiate(timingCirclePrefab, timingCircleParent);
        Material timingCircleMaterial = timingCricle.GetComponent<Image>().material;
        // 가드타이밍이라면 timingCircle의 색깔은 파랑, 아니라면 빨강
        timingCircleMaterial.color = isGuardTiming ? Color.cyan : Color.red;

        switch(arrow)
        {
            case Arrow.Up:
                transform.localPosition = Vector3.up * 2;
                break;
            case Arrow.Down:
                transform.localPosition = Vector3.down * 2;
                break;
            case Arrow.Left:
                transform.localPosition = Vector3.left * 2;
                break;
            case Arrow.Right:
                transform.localPosition = Vector3.right * 2;
                break;
        }

        // 줄어드는 원 생성
        GameObject circle = Instantiate(reduceCirclePrefab, reduceCircleParent);
        ReduceCircle reduceCircle = circle.GetComponent<ReduceCircle>();
        reduceCricleQueue.Enqueue(reduceCircle);

        // 원 줄어들기 코루틴
        StartCoroutine(reduceCircle.Co_StartReduce(currentTime, nextTime));
    }

    public void PressedCommanded(Accuracy accuracy, Skill skill)
    {
        StartCoroutine(reduceCricleQueue.Peek().Co_Vanish());
        reduceCricleQueue.Dequeue();
        character.SetNextSkill(skill);

        switch(accuracy)
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
        // 서클이 사라지는 애니메이션?과 퍼펙트나 미스 이런게 떠야함
    }
}
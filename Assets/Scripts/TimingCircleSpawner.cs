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

    // Ŀ�ǵ�� �Է��ؾ��� �پ��� ��Ŭ���� ���ʴ�� ���Դٰ� ������ ť
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
        // ����Ÿ�̹��̶�� timingCircle�� ������ �Ķ�, �ƴ϶�� ����
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

        // �پ��� �� ����
        GameObject circle = Instantiate(reduceCirclePrefab, reduceCircleParent);
        ReduceCircle reduceCircle = circle.GetComponent<ReduceCircle>();
        reduceCricleQueue.Enqueue(reduceCircle);

        // �� �پ��� �ڷ�ƾ
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
        // ��Ŭ�� ������� �ִϸ��̼�?�� ����Ʈ�� �̽� �̷��� ������
    }
}
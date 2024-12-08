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
    
    // Ŀ�ǵ�� �Է��ؾ��� �پ��� ��Ŭ���� ���ʴ�� ���Դٰ� ������ ť
    private Queue<ReduceCircle> reduceCricleQueue = new Queue<ReduceCircle>();
    public Queue<ReduceCircle> ReduceCricleQueue => reduceCricleQueue;

    [Header("TimingCircle")]
    [SerializeField] private Image img_timingCircle;
    private Material timingCircleMaterial;

    [Header("UI")]
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private Animator accuracyAnimator;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        character = GetComponentInParent<Character>();

        // img_timingCircle�� ��Ƽ������ �ϳ��� �ν��Ͻ�ȭ�Ͽ� ����
        timingCircleMaterial = new Material(img_timingCircle.material);
        img_timingCircle.material = timingCircleMaterial;
    }

    private void OnDestroy()
    {
        // �� ��ȯ �� ��Ȱ�� ��Ƽ���� �޸� ����
        if (timingCircleMaterial != null)
        {
            Destroy(timingCircleMaterial);
        }
    }

    public IEnumerator Co_PlayReduceCircle(double currentTime, double nextTime, bool isGuardTiming = false)
    {
        // ����Ÿ�̹��̶�� timingCircle�� ������ �Ķ�, �ƴ϶�� ����
        timingCircleMaterial.color = isGuardTiming ? Color.cyan : Color.red;

        // �پ��� �� ����
        GameObject circle = Instantiate(reduceCirclePrefab, reduceCircleParent);
        ReduceCircle reduceCircle = circle.GetComponent<ReduceCircle>();
        reduceCricleQueue.Enqueue(reduceCircle);

        // �� �پ��� �ڷ�ƾ
        StartCoroutine(reduceCircle.Co_StartReduce(currentTime, nextTime));

        // Ÿ�̹� �� ��Ÿ����
        yield return StartCoroutine(Co_AppearTimingCircle());
    }

    private IEnumerator Co_AppearTimingCircle()
    {
        Color reduceCircleColor = timingCircleMaterial.color;

        while (timingCircleMaterial.color.a < 0)
        {
            reduceCircleColor.a += Time.deltaTime * 3;
            timingCircleMaterial.SetColor("_Color", reduceCircleColor);

            yield return null;
        }

        reduceCircleColor.a = 1;
        timingCircleMaterial.SetColor("_Color", reduceCircleColor);
    }

    public IEnumerator Co_VanishTimingCircle()
    {
        Color reduceCircleColor = timingCircleMaterial.color;

        while (timingCircleMaterial.color.a > 0)
        {
            reduceCircleColor.a -= Time.deltaTime * 3;
            timingCircleMaterial.SetColor("_Color", reduceCircleColor);

            yield return null;
        }

        reduceCircleColor.a = 0;
        timingCircleMaterial.SetColor("_Color", reduceCircleColor);
    }

    public void PressedCommanded(Accuracy accuracy, Skill skill)
    {
        StartCoroutine(reduceCricleQueue.Peek().Co_Vanish());
        reduceCricleQueue.Dequeue();
        if (reduceCricleQueue.Count == 0)
        {
            StartCoroutine(Co_VanishTimingCircle());
        }
        character.SkillQueue.Enqueue(skill);

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
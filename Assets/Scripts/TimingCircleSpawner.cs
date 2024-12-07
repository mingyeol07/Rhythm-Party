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
    
    // 커맨드로 입력해야할 줄어드는 서클들이 차례대로 들어왔다가 나가는 큐
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

        // img_timingCircle의 머티리얼은 하나만 인스턴스화하여 재사용
        timingCircleMaterial = new Material(img_timingCircle.material);
        img_timingCircle.material = timingCircleMaterial;
    }

    private void OnDestroy()
    {
        // 씬 전환 시 재활용 머티리얼 메모리 해제
        if (timingCircleMaterial != null)
        {
            Destroy(timingCircleMaterial);
        }
    }

    public IEnumerator Co_PlayReduceCircle(double currentTime, double nextTime, bool isGuardTiming = false)
    {
        // 가드타이밍이라면 timingCircle의 색깔은 파랑, 아니라면 빨강
        timingCircleMaterial.color = isGuardTiming ? Color.cyan : Color.red;

        // 줄어드는 원 생성
        GameObject circle = Instantiate(reduceCirclePrefab, reduceCircleParent);
        ReduceCircle reduceCircle = circle.GetComponent<ReduceCircle>();
        reduceCricleQueue.Enqueue(reduceCircle);

        // 원 줄어들기 코루틴
        StartCoroutine(reduceCircle.Co_StartReduce(currentTime, nextTime));

        // 타이밍 원 나타내기
        yield return StartCoroutine(Co_AppearCanvasGroup());
    }

    private IEnumerator Co_AppearCanvasGroup()
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha + Time.deltaTime * 5);
            yield return new WaitForSeconds(0.01f); // 프레임 간격 조정
        }
        canvasGroup.alpha = 1;
    }

    public IEnumerator Co_VanishCanvasGroup()
    {
        while (canvasGroup.alpha > 0)
        {
            Debug.Log("DDD");
            canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha - Time.deltaTime * 3);
            yield return new WaitForSeconds(0.01f); // 프레임 간격 조정
        }
        canvasGroup.alpha = 0;
    }

    public void PressedCommanded(Accuracy accuracy, Skill skill)
    {
        StartCoroutine(reduceCricleQueue.Peek().Co_Vanish());
        reduceCricleQueue.Peek().ExitCircleQueue();
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
        // 서클이 사라지는 애니메이션?과 퍼펙트나 미스 이런게 떠야함
    }
}
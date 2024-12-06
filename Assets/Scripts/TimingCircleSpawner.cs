using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
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
    [SerializeField] private GameObject reduceCirclePrefab;
    [SerializeField] private Transform reduceCircleParent;
    [SerializeField] private Image img_timingCircle;
    [SerializeField] private Material m_reduceCircleMaterial;

    private Material reusableTimingMaterial;
    private readonly string materialColorName = "_Color";

    // 클릭 준비 상태
    private bool isReadied = false;
    public bool IsReadied => isReadied;

    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private Animator accuracyAnimator;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // img_timingCircle의 머티리얼은 하나만 인스턴스화하여 재사용
        reusableTimingMaterial = new Material(img_timingCircle.material);
        img_timingCircle.material = reusableTimingMaterial;
    }

    private void OnDestroy()
    {
        // 씬 전환 시 재활용 머티리얼 메모리 해제
        if (reusableTimingMaterial != null)
        {
            Destroy(reusableTimingMaterial);
        }
    }

    public IEnumerator Co_PlayReduceCircle(double currentTime, double nextTime, bool isGuardTiming = false)
    {
        isReadied = true;
        // 줄어드는 원 생성
        GameObject reduceCircle = Instantiate(reduceCirclePrefab, reduceCircleParent);

        // 줄어드는 원의 머테리얼 재활용 또는 인스턴스화
        Image circleImage = reduceCircle.GetComponent<Image>();
        Material reduceCircleMaterial = new Material(m_reduceCircleMaterial);
        circleImage.material = reduceCircleMaterial;

        reusableTimingMaterial.color = isGuardTiming ? Color.cyan : Color.red;

        StartCoroutine(Co_AppearCanvasGroup(reduceCircleMaterial));

        float startRadius = 0.5f;
        float minRadius = 0.12f;

        double startTime = Time.time;
        double duration = nextTime - currentTime;

        while (Time.time - startTime < duration)
        {
            double t = (Time.time - startTime) / duration;
            reduceCircleMaterial.SetFloat("_Radius", Mathf.Lerp(startRadius, minRadius, (float)t));

            if (!isReadied) break;
            yield return null;
        }

        Destroy(reduceCircle);
        Destroy(reduceCircleMaterial); // 파괴 시점 명확화

        yield return StartCoroutine(Co_VanishCanvasGroup());
        isReadied = false;
    }

    private IEnumerator Co_AppearCanvasGroup(Material reduceCircleMaterial)
    {
        if (reduceCircleMaterial == null || reusableTimingMaterial == null) yield break;

        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * 3;

            Color timingCircleColor = reusableTimingMaterial.color;
            timingCircleColor.a = Mathf.Clamp01(canvasGroup.alpha);
            reusableTimingMaterial.SetColor(materialColorName, timingCircleColor);

            Color reduceCircleColor = reduceCircleMaterial.color;
            reduceCircleColor.a = Mathf.Clamp01(canvasGroup.alpha);
            reduceCircleMaterial.SetColor(materialColorName, reduceCircleColor);

            if (!isReadied) break;
            yield return null;
        }

        canvasGroup.alpha = 1;
    }

    private IEnumerator Co_VanishCanvasGroup()
    {
        if (reusableTimingMaterial == null) yield break;

        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * 3;

            Color timingCircleColor = reusableTimingMaterial.color;
            timingCircleColor.a = Mathf.Clamp01(canvasGroup.alpha);
            reusableTimingMaterial.SetColor(materialColorName, timingCircleColor);

            if (!isReadied) break;
            yield return null;
        }

        canvasGroup.alpha = 0;
    }

    public void PressedCommanded(Accuracy accuracy)
    {
        isReadied = false;
        
        switch(accuracy)
        {
            case Accuracy.Critical:
                accuracyText.text = "Critical!";
                break;
            case Accuracy.Strike:
                accuracyText.text = "Strike";
                break;
            case Accuracy.Hit:
                accuracyText.text = "Hit";
                break;
            case Accuracy.Miss:
                accuracyText.text = "Miss";
                break;
        }

        accuracyAnimator.SetTrigger("Play");
        // 서클이 사라지는 애니메이션?과 퍼펙트나 미스 이런게 떠야함
    }
}
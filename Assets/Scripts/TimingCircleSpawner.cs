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

    // Ŭ�� �غ� ����
    private bool isReadied = false;
    public bool IsReadied => isReadied;

    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private Animator accuracyAnimator;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // img_timingCircle�� ��Ƽ������ �ϳ��� �ν��Ͻ�ȭ�Ͽ� ����
        reusableTimingMaterial = new Material(img_timingCircle.material);
        img_timingCircle.material = reusableTimingMaterial;
    }

    private void OnDestroy()
    {
        // �� ��ȯ �� ��Ȱ�� ��Ƽ���� �޸� ����
        if (reusableTimingMaterial != null)
        {
            Destroy(reusableTimingMaterial);
        }
    }

    public IEnumerator Co_PlayReduceCircle(double currentTime, double nextTime, bool isGuardTiming = false)
    {
        isReadied = true;
        // �پ��� �� ����
        GameObject reduceCircle = Instantiate(reduceCirclePrefab, reduceCircleParent);

        // �پ��� ���� ���׸��� ��Ȱ�� �Ǵ� �ν��Ͻ�ȭ
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
        Destroy(reduceCircleMaterial); // �ı� ���� ��Ȯȭ

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
        // ��Ŭ�� ������� �ִϸ��̼�?�� ����Ʈ�� �̽� �̷��� ������
    }
}
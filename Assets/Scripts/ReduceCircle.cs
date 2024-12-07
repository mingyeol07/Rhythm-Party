// # Systems
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;


// # Unity
using UnityEngine;
using UnityEngine.UI;

public class ReduceCircle : MonoBehaviour
{
    private Material circleMaterial;
    private Image img;
    private CanvasGroup canvasGroup;
    private TimingCircleSpawner circleSpawner;
    private readonly string materialColorName = "_Color";
    private Color reduceCircleColor;

    private bool isVanish;

    private void Awake()
    {
        canvasGroup = GetComponentInParent<CanvasGroup>();
        circleSpawner = GetComponentInParent<TimingCircleSpawner>();

        img = GetComponent<Image>();
        circleMaterial = new Material(img.material);
        img.material = circleMaterial;

        reduceCircleColor = circleMaterial.color;
    }

    private void Update()
    {
        if (isVanish) return;
        reduceCircleColor.a = Mathf.Clamp01(canvasGroup.alpha);
        circleMaterial.SetColor(materialColorName, reduceCircleColor);
    }

    public IEnumerator Co_StartReduce(double currentTime, double nextTime)
    {
        float startRadius = 0.5f;
        float minRadius = 0.12f;

        double startTime = Time.time;
        double duration = nextTime - currentTime;

        while (Time.time - startTime < duration)
        {
            double t = (Time.time - startTime) / duration;
            circleMaterial.SetFloat("_Radius", Mathf.Lerp(startRadius, minRadius, (float)t));

            if (isVanish) yield break;
            yield return null;
        }

        yield return StartCoroutine(Co_Vanish());
        ExitCircleQueue();
    }

    public IEnumerator Co_Vanish()
    {
        if (isVanish) yield break;
        isVanish = true;

        // 알파값 조이고
        Color reduceCircleColor = circleMaterial.color;

        while (circleMaterial.color.a > 0)
        {
            reduceCircleColor.a -= Time.deltaTime * 3;
            circleMaterial.SetColor(materialColorName, reduceCircleColor);

            yield return null;
        }

        reduceCircleColor.a = 0;
        circleMaterial.SetColor(materialColorName, reduceCircleColor);
        Destroy(this.gameObject);
    }

    public void ExitCircleQueue()
    {
        if (isVanish) return;

        circleSpawner.ReduceCricleQueue.Dequeue();
        if (circleSpawner.ReduceCricleQueue.Count == 0)
        {
            StartCoroutine(circleSpawner.Co_VanishCanvasGroup());
        }
    }

    private void OnDestroy()
    {
        if (circleMaterial != null)
        {
            Destroy(circleMaterial);
        }
    }
}

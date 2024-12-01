// # Systems
using System;
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class TimingCircle : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    [SerializeField] private GameObject reduceCirclePrefab;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {

    }

    public IEnumerator Co_PlayCircleReduce(double currentTime, double nextTime)
    {
        GameObject circle = Instantiate(reduceCirclePrefab, canvasGroup.transform);
        Material material = circle.GetComponent<Material>();

        StartCoroutine(Co_AppearCanvasGroup(material));

        float startRadius = 0.5f;
        float minRadius = 0.12f;

        while(currentTime < nextTime)
        {
            currentTime += Time.deltaTime;
            double t = currentTime / nextTime;

            material.SetFloat("_Radius", Mathf.Lerp(startRadius, minRadius, (float)t));

            yield return null;
        }
    }

    private IEnumerator Co_AppearCanvasGroup(Material material)
    {
        if (canvasGroup.alpha == 1) yield break;

        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * 10;

            if (material != null && canvasGroup != null)
            {
                Color color = material.GetColor("_Color");
                color.a = canvasGroup.alpha; // CanvasGroup Alpha 값을 쉐이더에 전달
                material.SetColor("_Color", color);
            }

            yield return null;
        }

        canvasGroup.alpha = 1;
    }
}

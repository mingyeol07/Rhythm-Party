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
    public Material CircleMaterial => circleMaterial;
    private Image img;
    private readonly string materialColorName = "_Color";

    private bool isVanish;

    private CircleSpawner mySpawner;

    private int targetTick;
    public int TargetTick => targetTick;

    private Arrow arrowType;
    public Arrow ArrowType => arrowType;

    private void Awake()
    {
        img = GetComponent<Image>();
        circleMaterial = new Material(img.material);
        img.material = circleMaterial;
    }

    public void Init(CircleSpawner spawner, int tick, Arrow arrow)
    {
        mySpawner = spawner;
        targetTick = tick;
        arrowType = arrow;
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

        if (mySpawner.ReduceCircleQueue.Count <= 1)
        {
            mySpawner.VanishTimingCircle();
        }
        yield return StartCoroutine(Co_Vanish());
        ExitCircleQueue();
    }

    public IEnumerator Co_Appear()
    {
        isVanish = false;

        Color reduceCircleColor = circleMaterial.color;

        while (circleMaterial.color.a < 0)
        {
            reduceCircleColor.a += Time.deltaTime * 3;
            circleMaterial.SetColor(materialColorName, reduceCircleColor);

            yield return null;
        }

        reduceCircleColor.a = 1;
        circleMaterial.SetColor(materialColorName, reduceCircleColor);
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
    }

    public void ExitCircleQueue()
    {
        if(mySpawner.ReduceCircleQueue.Count > 0)
        {
            mySpawner.ReduceCircleQueue.Dequeue();
            mySpawner.CircleManager.SkillCircleQueue.Dequeue();
            Destroy(this.gameObject);
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

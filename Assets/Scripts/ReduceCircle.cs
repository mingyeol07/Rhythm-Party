// # Systems
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;


// # Unity
using UnityEngine;
using UnityEngine.UI;

public class ReduceCircle : MonoBehaviour
{
    [SerializeField] private TMP_Text txt_count;
    [SerializeField] private Image img_arrow;
    [SerializeField] private Sprite sprite_arrow_left;
    [SerializeField] private Sprite sprite_arrow_up;
    [SerializeField] private Sprite sprite_arrow_down;
    [SerializeField] private Sprite sprite_arrow_right;

    private Material circleMaterial;
    public Material CircleMaterial => circleMaterial;
    private Image img_circle;
    private readonly string materialColorName = "_Color";

    private bool isVanish;

    private CircleSpawner mySpawner;

    private int targetTick;
    public int TargetTick => targetTick;

    private Arrow arrowType;
    public Arrow ArrowType => arrowType;

    private bool isClicked;

    private void Awake()
    {
        img_circle = GetComponent<Image>();
        circleMaterial = new Material(img_circle.material);
        img_circle.material = circleMaterial;
    }

    public void Init(CircleSpawner spawner, int tick, Arrow arrow, int count)
    {
        mySpawner = spawner;
        targetTick = tick;

        if (arrow != Arrow.None)
        {
            arrowType = arrow;

            switch (arrowType)
            {
                case Arrow.Left:
                    img_arrow.sprite = sprite_arrow_left;
                    break;
                case Arrow.Up:
                    img_arrow.sprite = sprite_arrow_up;
                    break;
                case Arrow.Down:
                    img_arrow.sprite = sprite_arrow_down;
                    break;
                case Arrow.Right:
                    img_arrow.sprite = sprite_arrow_left;
                    break;
            }
        }
        else
        {
            img_arrow = null;
        }

        if (count == 0) txt_count = null;
        else txt_count.text = count.ToString();
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
        reduceCircleColor.a = 0f;

        Color arrowColor = new();

        if (img_arrow != null)
        {
            arrowColor = img_arrow.color;
            arrowColor.a = 0f;
        }

        Color countColor = new();

        if (txt_count != null)
        {
            countColor = img_arrow.color;
            countColor.a = 0f;
        }

        while (reduceCircleColor.a < 1f)
        {
            reduceCircleColor.a += Time.deltaTime * 2f;
            circleMaterial.SetColor(materialColorName, reduceCircleColor);
            
            if(img_arrow != null)
            {
                arrowColor.a += Time.deltaTime * 2f; 
                img_arrow.color = arrowColor;
            }
            if(txt_count != null)
            {
                countColor.a += Time.deltaTime * 2f;
                txt_count.color = countColor;
            }

            if (isVanish) yield break;
            yield return null;
        }

        reduceCircleColor.a = 1f;
        circleMaterial.SetColor(materialColorName, reduceCircleColor);
    }

    public IEnumerator Co_Vanish()
    {
        if (isVanish) yield break;
        isVanish = true;

        // 알파값 조이고
        Color reduceCircleColor = circleMaterial.color;
        reduceCircleColor.a = 1f;

        Color arrowColor = new();

        if (img_arrow != null)
        {
            arrowColor = img_arrow.color;
            arrowColor.a = 1f;
        }

        Color countColor = new();

        if (txt_count != null)
        {
            countColor = img_arrow.color;
            countColor.a = 1f;
        }

        while (reduceCircleColor.a > 0f)
        {
            reduceCircleColor.a -= Time.deltaTime * 4.5f;
            circleMaterial.SetColor(materialColorName, reduceCircleColor);

            if (img_arrow != null)
            {
                arrowColor.a -= Time.deltaTime * 4.5f;
                img_arrow.color = arrowColor;
            }
            if (txt_count != null)
            {
                countColor.a -= Time.deltaTime * 4.5f;
                txt_count.color = countColor;
            }

            if (!isVanish) yield break;
            yield return null;
        }

        reduceCircleColor.a = 0f;
        circleMaterial.SetColor(materialColorName, reduceCircleColor);
    }

    public void ExitCircleQueue()
    {
        if (isClicked) return;
        // 미처 선택되지 못하고 도태되는 서클의 최후
        if (mySpawner.ReduceCircleQueue.Count > 0)
        {
            mySpawner.ReduceCircleQueue.Dequeue();
            //mySpawner.CircleManager.SkillCircleQueue.Dequeue();
            mySpawner.ShowText(Accuracy.Miss);
            Destroy(this.gameObject);
        }
    }

    public IEnumerator Clicked()
    {
        isClicked = true;
        yield return StartCoroutine(Co_Vanish());
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        if (circleMaterial != null)
        {
            Destroy(circleMaterial);
        }
    }
}

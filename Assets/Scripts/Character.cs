// # Systems
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;


// # Unity
using UnityEngine;

public class Character : MonoBehaviour
{
    private Skill[] skills = new Skill[4];

    // 스탯
    private int hp = 999;
    [SerializeField] private int speed;
    public int Speed => speed;

    private Skill nextSkill;
    public Skill NextSkill => nextSkill;

    [Header("Component")]
    [SerializeField] private CircleManager circleManager;
    public CircleManager CircleManager => circleManager;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator bounceAnimator;

    [Header("UI")]
    [SerializeField] private DamageText damageText;
    [SerializeField] private Animator commandFailedAnimator;

    private Vector3 returnPosition;
    private bool isMoveToCam;

    private void Awake()
    {
        skills = new Skill[4];

        skills[0] = new ElectricStorm(this);
        skills[1] = new ElectricStorm(this);
        skills[2] = new ElectricStorm(this);
        skills[3] = new ElectricStorm(this);
    }

    private void Start()
    {
        returnPosition = transform.position;
    }

    public void Damaged(int damage)
    {
        damageText.TextPlay(damageText.transform, damage);

        hp -= damage;
        if (hp < 0)
        {
            Die();
        }
    }

    private void Die()
    {

    }

    public void Attack(Accuracy accuracy)
    {
        if(nextSkill == null)
        {
            return;
        }

        animator.SetBool("Attacked", true);

        nextSkill.Activate(nextSkill.DamageCalculation(accuracy));
    }

    public void BounceAnimation()
    {
        bounceAnimator.SetTrigger("Bounce");
    }

    public void ReBounce()
    {
        animator.SetBool("Attacked", false);
        animator.SetBool("Commanded", false);
    }

    public ReduceCircle GetFirstCircleInSpawner(Arrow arrow)
    {
        if(circleManager.GetCircleSpawner(arrow).ReduceCircleQueue.Count > 0)
        {
            return circleManager.GetCircleSpawner(arrow).ReduceCircleQueue.Peek();
        }

        return null;
    }

    public void AttackCommand(Accuracy accuracy, Arrow arrow)
    {
        circleManager.PressedAttackCommand(accuracy, arrow);
        Attack(accuracy);
    }

    public void SkillCommand(Accuracy accuracy, int skillIndex)
    {
        animator.SetBool("Commanded", true);
        circleManager.PressedCommand(accuracy);

        // 스킬 세팅
        nextSkill = skills[skillIndex];
    }

    public void CommandFailedAnimation()
    {
        commandFailedAnimator.SetTrigger("Failed");
    }

    public IEnumerator MoveToCameraFront(float x)
    {
        isMoveToCam = true;

        Vector3 endPos = new Vector3(x, 0.5f, -10f);
        float maxTime = 0.2f;
        float time = 0;

        while(time < maxTime)
        {
            time += Time.deltaTime;
            float t = time / maxTime;
            transform.position = Vector3.Lerp(returnPosition, endPos, t);
            yield return null;
        }

        transform.position = endPos;
        isMoveToCam = false;
    }

    public IEnumerator ReturnToInPlace()
    {
        Vector3 nowPos = transform.position;

        float maxTime = 0.2f;
        float time = 0;

        while (time < maxTime)
        {
            if (isMoveToCam)
            {
                isMoveToCam = false;
                yield break;
            }
            time += Time.deltaTime;
            float t = time / maxTime;
            transform.position = Vector3.Lerp(nowPos, returnPosition, t);
            yield return null;
        }

        transform.position = returnPosition;
    }
}

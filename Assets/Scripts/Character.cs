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

    private readonly int hashTrigCommandFailed = Animator.StringToHash("Failed");
    private readonly int hashTrigBounce = Animator.StringToHash("Bounce");

    private readonly int hashTrigExit = Animator.StringToHash("Exit");
    private readonly int hashTrigCommand = Animator.StringToHash("Command");
    private readonly int hashTrigDamage= Animator.StringToHash("Damage");

    private readonly int hashTrigAtkLeft = Animator.StringToHash("AttackLeft");
    private readonly int hashTrigAtkUp = Animator.StringToHash("AttackUp");
    private readonly int hashTrigAtkDown = Animator.StringToHash("AttackDown");
    private readonly int hashTrigAtkRight = Animator.StringToHash("AttackRight");

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
        //animator.SetTrigger(hashTrigDamage);

        hp -= damage;
        if (hp < 0)
        {
            Die();
        }
    }

    private void Die()
    {

    }

    private void Attack(Accuracy accuracy)
    {
        if(nextSkill == null)
        {
            return;
        }

        int calculationDamage = nextSkill.DamageCalculation(accuracy);
        nextSkill.Activate(calculationDamage);
    }

    public void BounceAnimation()
    {
        bounceAnimator.SetTrigger(hashTrigBounce);
    }

    public void ReBounce()
    {
        animator.SetTrigger(hashTrigExit);
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

        switch (arrow)
        {
            case Arrow.Left:
                animator.SetTrigger(hashTrigAtkLeft);
                break;
            case Arrow.Up:
                animator.SetTrigger(hashTrigAtkUp);
                break;
            case Arrow.Down:
                animator.SetTrigger(hashTrigAtkDown);
                break;
            case Arrow.Right:
                animator.SetTrigger(hashTrigAtkRight);
                break;
        }

        Attack(accuracy);
    }

    public void SkillCommand(Accuracy accuracy, int skillIndex)
    {
        animator.SetTrigger(hashTrigCommand);
        circleManager.PressedCommand(accuracy);

        // 스킬 세팅
        nextSkill = skills[skillIndex];
    }

    public void CommandFailedAnimation()
    {
        commandFailedAnimator.SetTrigger(hashTrigCommandFailed);
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

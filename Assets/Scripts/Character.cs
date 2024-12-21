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

    // 공격 기회를 알려주는 써클 
    [SerializeField] private CircleManager circleManager;
    public CircleManager CircleManager => circleManager;

    private Skill nextSkill;
    public Skill NextSkill => nextSkill;

    // 컴포넌트들
    [SerializeField] private Animator animator;

    // UI들
    [SerializeField] private DamageText damageText;

    private Vector3 returnPosition;
    private bool isMoveToCam;

    private void Start()
    {
        skills = new Skill[4];

        skills[0] = new ElectricStorm(this);
        skills[1] = new ElectricStorm(this);
        skills[2] = new ElectricStorm(this);
        skills[3] = new ElectricStorm(this);

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

    public void Attack()
    {
        if(nextSkill == null)
        {
            // 스킬 실패 애니메이션
            return;
        }

        animator.SetBool("Attacked", true);

        Skill skill = nextSkill;
        nextSkill = null;

        skill.Activate();
    }

    public void BounceAnimation()
    {
        if (animator.GetBool("Commanded")) return;

        animator.SetTrigger("Bounce");
    }

    public void ReBounce()
    {
        animator.SetBool("Attacked", false);
        animator.SetBool("Commanded", false);
        animator.SetTrigger("Bounce");
    }

    public int GetFirstCircleTick(Arrow arrow)
    {
        if(circleManager.GetCircleSpawner(arrow).ReduceCircleQueue.Count > 0)
        {
            return circleManager.GetCircleSpawner(arrow).ReduceCircleQueue.Peek().TargetTick;
        }

        return 0;
    }

    public void AttackCommand(Accuracy accuracy, Arrow arrow)
    {
        circleManager.PressedAttackCommand(accuracy, arrow);
    }

    public void SkillCommand(Accuracy accuracy, int skillIndex)
    {
        //animator.SetBool("Commanded", true);
        circleManager.PressedCommand(accuracy);

        // 스킬 세팅
        nextSkill = skills[skillIndex];
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

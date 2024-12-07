// # Systems
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
    [SerializeField] private TimingCircleSpawner circleSpawner;
    public TimingCircleSpawner CricleSpawner => circleSpawner;

    // 입력된 커맨드에 따라 스킬들이 순서대로 저장되는 큐
    private Queue<Skill> skillQueue = new Queue<Skill>();
    public Queue<Skill> SkillQueue => skillQueue;

    // 컴포넌트들
    [SerializeField] private Animator animator;

    // UI들
    [SerializeField] private TMP_Text damageText;

    private void Start()
    {
        skills = new Skill[4];

        skills[0] = new Skill();
        skills[1] = new Skill();
        skills[2] = new Skill();
        skills[3] = new Skill();
    }

    public void Damaged(int damage)
    {
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
        if(skillQueue.Count == 0)
        {
            // 스킬 실패 애니메이션
            return;
        }

        animator.SetBool("Attacked", true);

        Skill skill = skillQueue.Dequeue();

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

    public void Commanded(Accuracy accuracy, int skillIndex)
    {
        animator.SetBool("Commanded", true);
        circleSpawner.PressedCommanded(accuracy, skills[skillIndex]);
        // 스킬 예약
    }
}

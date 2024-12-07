// # Systems
using System.Collections;
using System.Collections.Generic;
using TMPro;


// # Unity
using UnityEngine;

public class Character : MonoBehaviour
{
    private Skill[] skills = new Skill[4];

    // ����
    private int hp = 999;
    [SerializeField] private int speed;
    public int Speed => speed;

    // ���� ��ȸ�� �˷��ִ� ��Ŭ 
    [SerializeField] private TimingCircleSpawner circleSpawner;
    public TimingCircleSpawner CricleSpawner => circleSpawner;

    // �Էµ� Ŀ�ǵ忡 ���� ��ų���� ������� ����Ǵ� ť
    private Queue<Skill> skillQueue = new Queue<Skill>();
    public Queue<Skill> SkillQueue => skillQueue;

    // ������Ʈ��
    [SerializeField] private Animator animator;

    // UI��
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
            // ��ų ���� �ִϸ��̼�
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
        // ��ų ����
    }
}

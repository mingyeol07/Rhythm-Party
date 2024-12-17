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
    [SerializeField] private CircleManager circleManager;
    public CircleManager CircleManager => circleManager;

    private Skill nextSkill;
    public Skill NextSkill => nextSkill;

    // ������Ʈ��
    [SerializeField] private Animator animator;

    // UI��
    [SerializeField] private DamageText damageText;

    private Vector3 returnPosition;

    private void Start()
    {
        skills = new Skill[4];

        skills[0] = new ElectricStorm(this);
        skills[1] = new ElectricStorm(this);
        skills[2] = new ElectricStorm(this);
        skills[3] = new ElectricStorm(this);
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
            // ��ų ���� �ִϸ��̼�
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

    public void AttackCommand(Accuracy accuracy, Arrow arrow)
    {
        circleManager.PressedAttack(accuracy, arrow);
    }

    public void SkillCommand(Accuracy accuracy, int skillIndex)
    {
        animator.SetBool("Commanded", true);
        circleManager.PressedCommand(accuracy);

        nextSkill = skills[skillIndex];
    }

    public IEnumerator MoveToCameraFront(float x)
    {
        returnPosition = transform.position;

        Vector3 endPos = new Vector3(x, 0.5f, -10f);
        float maxTime = 0.5f;
        float time = 0;

        while(time < maxTime)
        {
            time += Time.deltaTime;
            float t = time / maxTime;
            transform.position = Vector3.Lerp(returnPosition, endPos, t);
            yield return null;
        }

        transform.position = endPos;
    }

    public IEnumerator ReturnToInPlace()
    {
        Vector3 nowPos = transform.position;

        float maxTime = 0.5f;
        float time = 0;

        while (time < maxTime)
        {
            time += Time.deltaTime;
            float t = time / maxTime;
            transform.position = Vector3.Lerp(nowPos, returnPosition, t);
            yield return null;
        }

        transform.position = returnPosition;
    }
}

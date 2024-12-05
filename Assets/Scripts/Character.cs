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
    [SerializeField] private TimingCircleSpawner timingCircle;
    public TimingCircleSpawner TimingCircle => timingCircle;

    // 컴포넌트들
    [SerializeField] private Animator animator;

    // UI들
    [SerializeField] private TMP_Text damageText;

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
        animator.SetBool("Attacked", true);
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
        timingCircle.PressedCommanded(accuracy);

        // 스킬 예약
    }
}

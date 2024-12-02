// # Systems
using System.Collections;
using System.Collections.Generic;
using TMPro;


// # Unity
using UnityEngine;

public class Character : MonoBehaviour
{
    // stats
    private int hp = 999;
    [SerializeField] private int speed;
    public int Speed => speed;

    [SerializeField] private TimingCircleSpawner timingCircle;
    public TimingCircleSpawner TimingCircle => timingCircle;

    [SerializeField] private Animator animator;

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

    public void Commanded(Accuracy accuracy)
    {
        animator.SetBool("Commanded", true);
        timingCircle.PressedCommanded(accuracy);
    }
}

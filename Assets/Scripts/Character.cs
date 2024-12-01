// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class Character : MonoBehaviour
{
    // stats
    private int hp = 0;
    [SerializeField] private int speed;
    public int Speed => speed;

    [SerializeField] private TimingCircle myCircle;
    public TimingCircle MyCircle => myCircle;

    public void DownHp(int damage)
    {
        hp -= damage;
        if(hp < 0)
        {
            Die();
        }
    }

    private void Die()
    {

    }
}

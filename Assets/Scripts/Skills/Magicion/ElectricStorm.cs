// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class ElectricStorm : Skill
{
    public override void Activate()
    {
        for(int i = 0; i< GameManager.Instance.EnemyMembers.Count; i++)
        {
            GameManager.Instance.EnemyMembers[i].Damaged(40);
        }
    }
}

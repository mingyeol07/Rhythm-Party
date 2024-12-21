// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class ElectricStorm : Skill
{
    public ElectricStorm(Character caster) : base(caster) { }

    public override void Activate(int damage)
    {
        for(int i = 0; i< GameManager.Instance.EnemyMembers.Count; i++)
        {
            GameManager.Instance.EnemyMembers[i].Damaged(damage);
        }
    }

    public override void SetCommand()
    {
        damage = 40;

        skillCommandList = new Arrow[7];
        skillCommandList[0] = Arrow.Down;
        skillCommandList[1] = Arrow.None;
        skillCommandList[2] = Arrow.Left;
        skillCommandList[3] = Arrow.None;
        skillCommandList[4] = Arrow.Right;
        skillCommandList[5] = Arrow.None;
        skillCommandList[6] = Arrow.Up;

        targetIndex = new int[4];
        targetIndex[0] = 0;
        targetIndex[1] = 1;
        targetIndex[2] = 2;
        targetIndex[3] = 3;
    }
}

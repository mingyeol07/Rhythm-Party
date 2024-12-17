// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class ElectricStorm : Skill
{
    public ElectricStorm(Character caster) : base(caster) { }

    public override void Activate()
    {
        for(int i = 0; i< GameManager.Instance.EnemyMembers.Count; i++)
        {
            GameManager.Instance.EnemyMembers[i].Damaged(40);
        }
    }

    public override void SetCommand()
    {
        skillCommandList = new Arrow[5];
        skillCommandList[0] = Arrow.Down;
        skillCommandList[1] = Arrow.Up;
        skillCommandList[2] = Arrow.Left;
        skillCommandList[3] = Arrow.None;
        skillCommandList[4] = Arrow.Right;

        targetIndex = new int[4];
        targetIndex[0] = 0;
        targetIndex[1] = 1;
        targetIndex[2] = 2;
        targetIndex[3] = 3;
    }
}

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

        skillCommandList = new Note[]
        {
             new Note(Arrow.Down, NoteType.Short, 2),
             new Note(Arrow.Down, NoteType.Long, 10),
        };

        targetIndex = new int[]
        {
            0,1,2,3,
        };
    }
}

// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class Axe : Skill
{
    public Axe(Character caster) : base(caster) { }

    public override void Activate(int damage)
    {
        for (int i = 0; i < GameManager.Instance.PartyMembers.Count; i++)
        {
            GameManager.Instance.PartyMembers[i].Damaged(damage);
        }
    }

    public override void SetCommand()
    {
        isEnemySkill = true;

        isPartyTarget = true;

        damage = 20;

        targetIndex = new int[] { 0, 2, 3 };

        skillCommandList = new Note[]
{
             new Note(Arrow.Down, NoteType.Short, 2),
             new Note(Arrow.Down, NoteType.Short, 10),
};
    }
}

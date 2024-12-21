// # Systems
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;


// # Unity
using UnityEngine;

public abstract class Skill
{
    protected int damage;
    protected int[] targetIndex; // 0, 1, 2, 3?
    protected Arrow[] skillCommandList;
    protected Character caster;
    protected bool isPartyTarget = false;
    public Character Caster => caster;

    /// <summary>
    /// 스킬 구현
    /// </summary>
    public abstract void Activate(int damage);
    public abstract void SetCommand();

    public void GetSkillCommandList(ref Queue<Arrow> queue)
    {
        for(int i =0; i < skillCommandList.Length; i++)
        {
            queue.Enqueue(skillCommandList[i]);
        }
    }
    public void GetTargetIndex(out int[] arr, out bool isParty)
    {
        arr = targetIndex;
        isParty = isPartyTarget;
    }
    public Skill(Character caster)
    {
        SetCommand();
        this.caster = caster;
    }
    public int DamageCalculation(Accuracy accuracy)
    {
        int dam = damage;

        switch (accuracy)
        {
            case Accuracy.Critical:
                dam = damage + damage / 2;
                break;
            case Accuracy.Strike:
                break;
            case Accuracy.Hit:
                dam /= 2;
                break;
            case Accuracy.Miss:
                dam = 0;
                break;
        }

        return dam;
    }
}

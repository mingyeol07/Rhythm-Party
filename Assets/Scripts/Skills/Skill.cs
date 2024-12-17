// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public abstract class Skill : MonoBehaviour
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
    public abstract void Activate();
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
}

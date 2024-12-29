// # Systems
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;


// # Unity
using UnityEngine;

public abstract class Skill
{
    protected int damage;
    protected int[] targetIndex; // 0, 1, 2, 3?

    protected bool isPartyTarget = false;
    protected bool isEnemySkill = false;
    public bool IsEnemySkill => isEnemySkill;

    protected Note[] skillCommandList;

    protected Character caster;
    public Character Caster => caster;

    /// <summary>
    /// 스킬 구현
    /// </summary>
    public abstract void Activate(int damage);

    /// <summary>
    /// 타겟인덱스와 커맨드리스트, 데미지를 초기화해주세요
    /// </summary>
    public abstract void SetCommand();

    public void GetSkillCommandList(ref Queue<Note> queue)
    {
        for(int i =0; i < skillCommandList.Length; i++)
        {
            queue.Enqueue(skillCommandList[i]);
        }
    }
    public void GetTargetIndex(out int[] arr, out bool isPartyTarget)
    {
        arr = targetIndex;
        isPartyTarget = this.isPartyTarget;
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

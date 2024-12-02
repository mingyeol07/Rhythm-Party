// # Systems
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


// # Unity
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Managers")]
    [SerializeField] private TickManager tickManager;

    [SerializeField] private List<Character> partyMembers = new List<Character>();
    private List<Character> sortedParty = new List<Character>();

    [SerializeField] private List<Character> enemyMembers = new List<Character>();
    private List<Character> sortedEnemy = new List<Character>();

    [SerializeField] private Animator bounceAnimator;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SortPartyMember();
    }

    public void BounceAnimation()
    {
        bounceAnimator.SetTrigger("Bounce");
        for (int i = 0; i < partyMembers.Count; i++)
        {
            partyMembers[i].BounceAnimation();
        }
        for (int i = 0; i < enemyMembers.Count; i++)
        {
            enemyMembers[i].BounceAnimation();
        }
    }

    #region Party
    public void SortPartyMember()
    {
        sortedParty = new List<Character>(partyMembers);

        sortedParty.Sort((a, b) => b.Speed.CompareTo(a.Speed));
    }

    public void PlayPartyTimingCircle(int index, double startTime, double endTime, int targetTick)
    {
        if (sortedParty.Count <= index) return;

        StartCoroutine(sortedParty[index].TimingCircle.Co_PlayCircleReduce(startTime, endTime, targetTick, false));
    }
    public void PlayPartyGuardCircle(int index, double startTime, double endTime, int targetTick)
    {
        if (sortedParty.Count <= index) return;

        StartCoroutine(sortedParty[index].TimingCircle.Co_PlayCircleReduce(startTime, endTime, targetTick, true));
    }
    public void PlayPartyAttack(int index)
    {
        sortedParty[index].Attack();
    }

    public void PlayPartyReBounce()
    {
        for(int i =0; i < partyMembers.Count; i++)
        {
            partyMembers[i].ReBounce();
        }
    }
    #endregion

    #region Enemy

    public void SortEnemyMember()
    {
        sortedEnemy = new List<Character>(enemyMembers);

        sortedEnemy.Sort((a, b) => b.Speed.CompareTo(a.Speed));
    }
    public void PlayEnemyTimingCircle(int index, double startTime, double endTime)
    {
        if (sortedEnemy.Count <= index) return;

        StartCoroutine(sortedEnemy[index].TimingCircle.Co_PlayCircleReduce(startTime, endTime, 0));
    }
    #endregion Enemy

    #region Input
    public void PressedKey()
    {
        Character member = null;

        for(int i =0; i < sortedParty.Count; i++)
        {
            if (sortedParty[i].TimingCircle.IsReadied)
            {
                member = sortedParty[i];
                break;
            }
        }

        member?.Commanded(tickManager.GetAccuracy(member.TimingCircle.NextTargetTick));
    }
    #endregion
}

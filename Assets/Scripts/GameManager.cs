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

    [SerializeField] private Animator bounceAnimator;

    private void Awake()
    {
        Instance = this;
    }

    //private void Start()
    //{
    //    for(int i = 0; i < partyMembers.Count; i++)
    //    {
    //        double nextTime = tickManager.GetSequenceTime(i + 1);
    //        StartCoroutine(partyMembers[i].MyCircle.Co_PlayCircleReduce(tickManager.CurrentTime, nextTime));
    //    }
    //}

    public void BounceAnimation()
    {
        bounceAnimator.SetTrigger("Bounce");
    }

    public void PressedKey()
    {
        if(tickManager.IsPerfect())
        {
            Debug.Log("Yes!");
        }
    }

    private List<Character> GetPartySortToSpeed()
    {
        List<Character> members = new List<Character>(partyMembers);

        members.Sort((a, b) => b.Speed.CompareTo(a.Speed));

        for(int i = 0;i < members.Count; i++)
        {
            Debug.Log(members[i].Speed);
        }

        return members;
    }
}

// # Systems
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;


// # Unity
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Managers")]
    [SerializeField] private TickManager tickManager;

    [SerializeField] private List<Character> partyMembers = new List<Character>();
    private List<Character> sortedPartyWithSpeed = new List<Character>();
    private List<List<Character>> sortedPartyAttackSequence = new List<List<Character>>();
    public event Action ItemAdditionalAttackChance; // 아이템에 의한 추가 공격 기회
    // 캐릭터는 공격 기회를 가진다. 공격 기회는 파티에서의 스피드를 비교하여 정해지며 모두에게 무조건 한번은 주어진다.
    // 만약 아이템이나 능력 중, "1번째 리듬에 공격 기회 추가"라는 아이템이 있다면
    //  한 박자에 동시에 커맨드를 입력해야하는? or 동료끼리의 시너지 공격이 가능하다.
    // 그렇기에 한 박자에 여러 캐릭터가 공격하는 시스템을 넣고 싶다!! List안에 Queue를 모두 꺼내어 박자에 투입!!

    // 이걸 정하는 기준..? =>
    // 1. 스피드가 높은 순서로 일단 리스트의 0,1,2,3에 캐릭터별로 공격 기회를 넣는다
    // 2. 캐릭터의 아이템이 자체적으로 Action에 함수를 전달, Action을 발동하여 리스트에 캐릭터 공격 기회 추가
    private List<List<Character>> sortedPartyGuardSequence = new List<List<Character>>();

    [SerializeField] private List<Character> enemyMembers = new List<Character>();
    private List<Enemy> sortedEnemyWithSpeed = new List<Enemy>();
    private List<List<Enemy>> sortedEnemyAttackSequence = new List<List<Enemy>>();
    public event Action EnemyAdditionalAttackChance; // 적 기믹에 의한 추가 공격 기회

    [SerializeField] private Animator bounceAnimator;

    private int pressCount;
    public int PressCount => pressCount;

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
        pressCount = 0;
        sortedPartyAttackSequence.Clear();

        // 스피드 순서대로 정렬된 파티
        sortedPartyWithSpeed = new List<Character>(partyMembers);
        sortedPartyWithSpeed.Sort((a, b) => b.Speed.CompareTo(a.Speed));

        for (int i=0; i < 4; i++)
        {
            List<Character> characterList = new List<Character> { sortedPartyWithSpeed[i] };
            sortedPartyAttackSequence.Add(characterList);
        }

        // 아이템에 의한 공격 기회 추가
        EnemyAdditionalAttackChance?.Invoke();
        EnemyAdditionalAttackChance = null;

        Character character = partyMembers[3];

        for (int i = 0; i < 4; i++)
        {
            sortedPartyAttackSequence[i].Add(character);
        }
    }

    public void PlayPartyTimingCircle(int index, double startTime, double endTime)
    {
        if (index >= sortedPartyAttackSequence.Count) return;

        foreach(Character character in sortedPartyAttackSequence[index])
        {
            StartCoroutine(character.CricleSpawner.Co_PlayReduceCircle(startTime, endTime, false));
        }
    }
    public void PlayPartyGuardCircle(int index, double startTime, double endTime)
    {
        if (index >= sortedPartyGuardSequence.Count) return;

        foreach (Character character in sortedPartyGuardSequence[index])
        {
            StartCoroutine(character.CricleSpawner.Co_PlayReduceCircle(startTime, endTime, true));
        }
    }

    public void PlayPartyReBounce()
    {
        for(int i =0; i < partyMembers.Count; i++)
        {
            partyMembers[i].ReBounce();
        }
    }
    public void AttackPartyMember(int index)
    {
        for(int i=0; i < sortedPartyAttackSequence[index].Count;i++)
        {
            sortedPartyAttackSequence[index][i].Attack();
        }
    }
    #endregion

    #region Enemy
    public void SortEnemyMember(ref int[] enemyTicks)
    {
        //pressCount = 0;
        //sortedEnemyAttackSequence.Clear();
        //sortedEnemyWithSpeed.Clear();

        //// 스피드 순서대로 정렬된 파티
        //for (int i = 0; i < enemyMembers.Count; i++)
        //{
        //    sortedEnemyWithSpeed.Add(enemyMembers[i].GetComponent<Enemy>());
        //}
        //sortedEnemyWithSpeed.Sort((a, b) => b.Speed.CompareTo(a.Speed));

        //for (int i = 0; i < 4; i++)
        //{
        //    List<Enemy> characterList = new List<Enemy>();
        //    sortedEnemyAttackSequence.Add(characterList);
        //}

        //// Enemy는 자기맘대로 여러번 공격도 가능함.
        //// Enemy들의 공격 순서를 어떻게 해야할지가 문제
        //// 일단 스피드가 빠른 순서대로 정렬하고
        //// 순회하며 각자 예약된 스킬Queue를 가져와 하나만 빼서 실행하여 
        //// 빠른순서대로 번갈아가며실행

        //for (int i = 0; i < sortedEnemyWithSpeed.Count; i++)
        //{
        //    for(int j =0; j < sortedEnemyWithSpeed[i].PreparedSkills.Count; j++)
        //    {
        //       // sortedEnemyAttackSequence[sortedEnemyWithSpeed[i].PreparedSkills[j].Turn].Add(sortedEnemyWithSpeed[i]);
        //    }
        //}

        //// 아이템에 의한 공격 기회 추가
        //EnemyAdditionalAttackChance?.Invoke();
        //EnemyAdditionalAttackChance = null;

        ////sortedPartyGuardSequence
        ////sortedEnemyAttackSequence
        //enemyTicks = new int[sortedEnemyWithSpeed.Count];
    }
    #endregion Enemy

    #region Input
    public void PressedKey(KeyIndexInArray key)
    {
        if(tickManager.TickCount > 16)
        {
            if (pressCount >= sortedPartyGuardSequence.Count) 
                return;

            if (sortedPartyGuardSequence[pressCount][0].CricleSpawner.ReduceCricleQueue.Count == 0)
            {
                // 서클이 아무것도 없는경우
                return;
            }

            Guard();
        }
        else
        {
            if (pressCount >= sortedPartyAttackSequence.Count)
                return;

            if (sortedPartyAttackSequence[pressCount][0].CricleSpawner.ReduceCricleQueue.Count == 0)
            {
                // 서클이 아무것도 없는경우
                return;
            }

            int skillIndex = (int)key;

            Command(skillIndex);
        }

        pressCount++;
    }

    private void Command(int skillIndex)
    {
        for (int i = 0; i < sortedPartyAttackSequence[pressCount].Count; i++)
        {
            Character member = sortedPartyAttackSequence[pressCount][i];
            Accuracy accuracy = tickManager.GetAccuracy(9 + (2 * pressCount));
            member?.Commanded(accuracy, skillIndex);
        }
    }

    private void Guard()
    {
        for (int i = 0; i < sortedPartyGuardSequence[pressCount].Count; i++)
        {
            Character member = sortedPartyAttackSequence[pressCount][i];
            Accuracy accuracy = tickManager.GetAccuracy(9 + (2 * pressCount));
            //member?.Guard(accuracy);
        }
    }
    #endregion
}

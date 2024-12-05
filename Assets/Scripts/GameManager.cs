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

    

    [SerializeField] private List<Character> enemyMembers = new List<Character>();
    private List<Queue<Character>> sortedEnemyAttackSequence = new List<Queue<Character>>();

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
        sortedPartyAttackSequence.Clear();

        // 스피드 순서대로 정렬된 파티
        sortedPartyWithSpeed = new List<Character>(partyMembers);
        sortedPartyWithSpeed.Sort((a, b) => b.Speed.CompareTo(a.Speed));

        // 스피드가 빠른순서로 Queue에 담긴 후 List에 추가
        for (int i=0; i < 4; i++)
        {
            List<Character> characterQueue = new List<Character> { sortedPartyWithSpeed[i] };
            sortedPartyAttackSequence.Add(characterQueue);
        }

        // 아이템에 의한 공격 기회 추가
        ItemAdditionalAttackChance?.Invoke();
        ItemAdditionalAttackChance = null;
    }

    public void PlayPartyTimingCircle(int index, double startTime, double endTime)
    {
        if (sortedPartyAttackSequence[index].Count == 0) return;

        foreach(Character character in sortedPartyAttackSequence[index])
        {
            StartCoroutine(character.TimingCircle.Co_PlayReduceCircle(startTime, endTime, false));
        }
    }
    public void PlayPartyGuardCircle(int index, double startTime, double endTime)
    {
        if (sortedPartyAttackSequence[index].Count == 0) return;

        foreach (Character character in sortedPartyAttackSequence[index])
        {
            StartCoroutine(character.TimingCircle.Co_PlayReduceCircle(startTime, endTime, true));
        }
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
    #endregion Enemy

    #region Input
    public void PressedKey(KeyIndexInArray key)
    {
        // 지금 틱에 공격하는 파티멤버의 인덱스를 받아옴
        int skillIndex = (int)key;
        int attackSequenceIndex = 0;

        for(int i =0; i < sortedPartyAttackSequence[attackSequenceIndex].Count; i++)
        {
            Character member = sortedPartyAttackSequence[attackSequenceIndex][i];
            Accuracy accuracy = tickManager.GetAccuracy(1);
            if (accuracy != Accuracy.Miss)
            {
                member?.Commanded(accuracy, skillIndex);
            }
            else
            {
                // miss
            }
        }
    }
    #endregion
}

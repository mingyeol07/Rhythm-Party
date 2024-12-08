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
    public event Action ItemAdditionalAttackChance; // �����ۿ� ���� �߰� ���� ��ȸ
    // ĳ���ʹ� ���� ��ȸ�� ������. ���� ��ȸ�� ��Ƽ������ ���ǵ带 ���Ͽ� �������� ��ο��� ������ �ѹ��� �־�����.
    // ���� �������̳� �ɷ� ��, "1��° ���뿡 ���� ��ȸ �߰�"��� �������� �ִٸ�
    //  �� ���ڿ� ���ÿ� Ŀ�ǵ带 �Է��ؾ��ϴ�? or ���᳢���� �ó��� ������ �����ϴ�.
    // �׷��⿡ �� ���ڿ� ���� ĳ���Ͱ� �����ϴ� �ý����� �ְ� �ʹ�!! List�ȿ� Queue�� ��� ������ ���ڿ� ����!!

    // �̰� ���ϴ� ����..? =>
    // 1. ���ǵ尡 ���� ������ �ϴ� ����Ʈ�� 0,1,2,3�� ĳ���ͺ��� ���� ��ȸ�� �ִ´�
    // 2. ĳ������ �������� ��ü������ Action�� �Լ��� ����, Action�� �ߵ��Ͽ� ����Ʈ�� ĳ���� ���� ��ȸ �߰�
    private List<List<Character>> sortedPartyGuardSequence = new List<List<Character>>();

    [SerializeField] private List<Character> enemyMembers = new List<Character>();
    private List<Enemy> sortedEnemyWithSpeed = new List<Enemy>();
    private List<List<Enemy>> sortedEnemyAttackSequence = new List<List<Enemy>>();
    public event Action EnemyAdditionalAttackChance; // �� ��Ϳ� ���� �߰� ���� ��ȸ

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

        // ���ǵ� ������� ���ĵ� ��Ƽ
        sortedPartyWithSpeed = new List<Character>(partyMembers);
        sortedPartyWithSpeed.Sort((a, b) => b.Speed.CompareTo(a.Speed));

        for (int i=0; i < 4; i++)
        {
            List<Character> characterList = new List<Character> { sortedPartyWithSpeed[i] };
            sortedPartyAttackSequence.Add(characterList);
        }

        // �����ۿ� ���� ���� ��ȸ �߰�
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

        //// ���ǵ� ������� ���ĵ� ��Ƽ
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

        //// Enemy�� �ڱ⸾��� ������ ���ݵ� ������.
        //// Enemy���� ���� ������ ��� �ؾ������� ����
        //// �ϴ� ���ǵ尡 ���� ������� �����ϰ�
        //// ��ȸ�ϸ� ���� ����� ��ųQueue�� ������ �ϳ��� ���� �����Ͽ� 
        //// ����������� �����ư������

        //for (int i = 0; i < sortedEnemyWithSpeed.Count; i++)
        //{
        //    for(int j =0; j < sortedEnemyWithSpeed[i].PreparedSkills.Count; j++)
        //    {
        //       // sortedEnemyAttackSequence[sortedEnemyWithSpeed[i].PreparedSkills[j].Turn].Add(sortedEnemyWithSpeed[i]);
        //    }
        //}

        //// �����ۿ� ���� ���� ��ȸ �߰�
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
                // ��Ŭ�� �ƹ��͵� ���°��
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
                // ��Ŭ�� �ƹ��͵� ���°��
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

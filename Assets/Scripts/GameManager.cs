// # Systems
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;



// # Unity
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Managers")]
    [SerializeField] private TickManager tickManager;
    private float zoomInCamCenter = 1.7f;

    #region Party
    [Header("Party")]
    [SerializeField] private List<Character> partyMembers = new List<Character>(); // ��Ƽ���
    public List<Character> PartyMembers => partyMembers;

    private List<Character> sortedPartyAttackSequence = new List<Character>();
    private List<List<Character>> sortedPartyGuardSequence = new List<List<Character>>();

    private Queue<Skill> partyAttackCommandQueue = new Queue<Skill>();
    public Queue<Skill> PartyAttackCommandQueue => partyAttackCommandQueue;

    private int previousZoomInPartyCharacterIndex;

    [Header("Enemy")]
    [SerializeField] private List<Character> enemyMembers = new List<Character>();
    public List<Character> EnemyMembers => enemyMembers;

    private List<List<Character>> sortedEnemyAttackSequence = new List<List<Character>>();

    private int[] previousZoomInEnemyCharactersIndex;
    #endregion

    [SerializeField] private Animator bounceAnimator;

    private int pressCount;
    public int PressCount => pressCount;

    private Camera mainCam;

    private void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
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

    public void ZoomInCharacter(int partyCharIndex, int[] enemyCharIndex, bool isParty = false)
    {
        previousZoomInPartyCharacterIndex = partyCharIndex;
        previousZoomInEnemyCharactersIndex = enemyCharIndex;

        StartCoroutine(sortedPartyAttackSequence[partyCharIndex].MoveToCameraFront(-zoomInCamCenter));

        if(enemyCharIndex.Length > 2)
        {
            zoomInCamCenter = 1;
        }
        else
        {
            zoomInCamCenter = 1.7f;
        }

        for (int i = 0; i < enemyCharIndex.Length; i++)
        {
            StartCoroutine(enemyMembers[enemyCharIndex[i]].MoveToCameraFront(zoomInCamCenter + i * 1));
        }

        zoomInCamCenter = 1.7f;
    }

    public void ZoomOutCharacter()
    {
        if (previousZoomInPartyCharacterIndex == 0) return;

        StartCoroutine(sortedPartyAttackSequence[previousZoomInPartyCharacterIndex].ReturnToInPlace());
        for (int i = 0; i < previousZoomInEnemyCharactersIndex.Length; i++)
        {
            StartCoroutine(enemyMembers[previousZoomInEnemyCharactersIndex[i]].ReturnToInPlace());
        }
    }

    #region Party
    public void SortPartyMember()
    {
        pressCount = 0;
        sortedPartyAttackSequence.Clear();

        // ���ǵ� ������� ���ĵ� ��Ƽ
        sortedPartyAttackSequence = new List<Character>(partyMembers);
        sortedPartyAttackSequence.Sort((a, b) => b.Speed.CompareTo(a.Speed));
    }

    public void SetPartyCommandList()
    {
        for(int i =0; i < sortedPartyAttackSequence.Count; i++)
        {
            partyAttackCommandQueue.Enqueue(sortedPartyAttackSequence[i].NextSkill);
        }
    }

    public void PlayPartyTimingCircle(int index, double startTime, double endTime, Arrow arrow = Arrow.Up)
    {
        if (index >= sortedPartyAttackSequence.Count) return;

        sortedPartyAttackSequence[index].CricleSpawner.SpawnReduceCircle(startTime, endTime, false, arrow);
    }
    public void PlayPartyGuardCircle(int index, double startTime, double endTime)
    {
        if (index >= sortedPartyGuardSequence.Count) return;

        foreach (Character character in sortedPartyGuardSequence[index])
        {
            sortedPartyAttackSequence[index].CricleSpawner.SpawnReduceCircle(startTime, endTime, true);
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
        sortedPartyAttackSequence[index].Attack();
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
    public void PressedKey(Arrow key)
    {
        if(tickManager.TickCount > 16)
        {

        }
        else
        {
            if (pressCount >= sortedPartyAttackSequence.Count)
                return;

            int skillIndex = (int)key;

            Command(skillIndex);
        }

        pressCount++;
    }

    private void Command(int skillIndex)
    {
        Character member = sortedPartyAttackSequence[pressCount];
        Accuracy accuracy = tickManager.GetAccuracy(9 + (2 * pressCount));
        member?.Commanded(accuracy, skillIndex);
    }

    private void Guard()
    {

    }
    #endregion
}

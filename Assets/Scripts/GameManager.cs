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

        // ���ǵ� ������� ���ĵ� ��Ƽ
        sortedPartyWithSpeed = new List<Character>(partyMembers);
        sortedPartyWithSpeed.Sort((a, b) => b.Speed.CompareTo(a.Speed));

        // ���ǵ尡 ���������� Queue�� ��� �� List�� �߰�
        for (int i=0; i < 4; i++)
        {
            List<Character> characterQueue = new List<Character> { sortedPartyWithSpeed[i] };
            sortedPartyAttackSequence.Add(characterQueue);
        }

        // �����ۿ� ���� ���� ��ȸ �߰�
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
        // ���� ƽ�� �����ϴ� ��Ƽ����� �ε����� �޾ƿ�
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

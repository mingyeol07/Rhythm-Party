// # Systems
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;



// # Unity
using UnityEngine;
using UnityEngine.TextCore.Text;

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

    private Character nowSkillCaster = null;
    public Character NowSkillCaster => nowSkillCaster;


    [Header("Enemy")]
    [SerializeField] private List<Character> enemyMembers = new List<Character>();
    public List<Character> EnemyMembers => enemyMembers;

    private List<List<Character>> sortedEnemyAttackSequence = new List<List<Character>>();

    private Character previousZoomInCaster;
    private List<Character> previousZoomInTargets = new List<Character>();
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

    public void ZoomInCharacter(Skill skill)
    {
        skill.GetTargetIndex(out int[] targetArray, out bool isParty);
        Character caster = sortedPartyAttackSequence.Find(c => c == skill.Caster);
        previousZoomInCaster = caster;

        StartCoroutine(caster.MoveToCameraFront(-zoomInCamCenter));

        if(targetArray.Length > 2)
        {
            zoomInCamCenter = 1;
        }
        else
        {
            zoomInCamCenter = 1.7f;
        }

        for (int i = 0; i < targetArray.Length; i++)
        {
            Character character;
            if (!isParty)
            {
                character = enemyMembers[targetArray[i]];
            }
            else
            {
                character = partyMembers[targetArray[i]];
            }
            previousZoomInTargets.Add(character);
            StartCoroutine(character.MoveToCameraFront(zoomInCamCenter + i * 1));
        }

        zoomInCamCenter = 1.7f;
    }

    public void ZoomOutCharacter()
    {
        if (previousZoomInCaster == null) return;

        StartCoroutine(previousZoomInCaster.ReturnToInPlace());
        previousZoomInCaster = null;

        for (int i = 0; i < previousZoomInTargets.Count; i++)
        {
            StartCoroutine(previousZoomInTargets[i].ReturnToInPlace());
        }
        previousZoomInTargets.Clear();
    }

    #region Party
    public void SetNowSkillCaster(ref Skill skill, int skillCasterIndex)
    {
        if(skillCasterIndex > 3)
        {
            nowSkillCaster = null;
            skill = null;
            return;
        }
        else
        {
            nowSkillCaster = sortedPartyAttackSequence[skillCasterIndex];
            skill = nowSkillCaster.NextSkill;
        }
    }

    public void SortPartyMember()
    {
        pressCount = 0;
        sortedPartyAttackSequence.Clear();

        // ���ǵ� ������� ���ĵ� ��Ƽ
        sortedPartyAttackSequence = new List<Character>(partyMembers);
        sortedPartyAttackSequence.Sort((a, b) => b.Speed.CompareTo(a.Speed));
    }

    public void PlayPartyTimingCircle(int sortedPartyIndex, double startTime, double endTime, Arrow arrow)
    {
        if (sortedPartyIndex >= sortedPartyAttackSequence.Count) return;

        sortedPartyAttackSequence[sortedPartyIndex].CircleManager.SpawnReduceCircle(startTime, endTime, arrow, false);
    }
    public void PlayPartyGuardCircle(int index, double startTime, double endTime)
    {
        if (index >= sortedPartyGuardSequence.Count) return;

        foreach (Character character in sortedPartyGuardSequence[index])
        {
            sortedPartyAttackSequence[index].CircleManager.SpawnReduceCircle(startTime, endTime, Arrow.Up, true);
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
    public void PressedKey(Arrow arrow)
    {
        Character member; 
        Accuracy accuracy = tickManager.GetAccuracy(9 + (2 * pressCount));

        if (tickManager.TickCount > 16)
        {
            
        }
        else
        {
            if (pressCount >= sortedPartyAttackSequence.Count)
                return;

            int skillIndex = (int)arrow;
            member = sortedPartyAttackSequence[pressCount];
            member.SkillCommand(accuracy, skillIndex);
        }

        pressCount++;
    }
    #endregion
}

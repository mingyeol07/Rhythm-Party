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

    private Character previousZoomInCaster;
    private List<Character> previousZoomInTargets = new List<Character>();

    #region Party
    [Header("Party")]
    [SerializeField] private List<Character> partyMembers = new List<Character>(); // ��Ƽ���
    public List<Character> PartyMembers => partyMembers;

    private List<Character> sortedPartyAttackSequence = new List<Character>();

    private Character nowSkillCaster = null;
    public Character NowSkillCaster => nowSkillCaster;


    [Header("Enemy")]
    [SerializeField] private List<Character> enemyMembers = new List<Character>();
    public List<Character> EnemyMembers => enemyMembers;

    #endregion

    [SerializeField] private Animator bounceAnimator;

    private int pressCount;
    public int PressCount => pressCount;

    [SerializeField] private CamMoving camMove;

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

    public void ZoomInCam()
    {
        StartCoroutine(camMove.Co_MoveAttackAngle());
    }

    public void ZoomOutCam()
    {
        StartCoroutine(camMove.Co_MoveDefaultAngle());
    }

    public void ZoomInCharacter(Skill skill)
    {
        Character caster = skill.Caster;
        previousZoomInCaster = caster;

        StartCoroutine(caster.Co_MoveToCameraFront(-zoomInCamCenter));
    }

    public void ZoomInTargets(Skill skill)
    {
        previousZoomInTargets.Clear();
        skill.GetTargetIndex(out int[] targetArray, out bool isParty);
        if (targetArray.Length > 2)
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
            StartCoroutine(character.Co_MoveToCameraFront(zoomInCamCenter + i * 1.5f));
        }

        zoomInCamCenter = 1.7f;
    }

    public void ZoomOutCharacter()
    {
        if (previousZoomInCaster == null) return;
        StartCoroutine(previousZoomInCaster.Co_ReturnToInPlace());
        previousZoomInCaster.ReBounce();
    }

    public void ZoomOutTargets()
    {
        for (int i = 0; i < previousZoomInTargets.Count; i++)
        {
            StartCoroutine(previousZoomInTargets[i].Co_ReturnToInPlace());
            previousZoomInTargets[i].ReBounce();
        }
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

        nowSkillCaster = sortedPartyAttackSequence[skillCasterIndex];
        skill = nowSkillCaster.NextSkill;
    }

    public void SortPartyMember()
    {
        pressCount = 0;
        sortedPartyAttackSequence.Clear();

        // ���ǵ� ������� ���ĵ� ��Ƽ
        sortedPartyAttackSequence = new List<Character>(partyMembers);
        sortedPartyAttackSequence.Sort((a, b) => b.Speed.CompareTo(a.Speed));
    }

    public void PlayPartyTimingCircle(int sortedPartyIndex, double startTime, double endTime, Arrow arrow, TimingType timingType, NoteType noteType, int targetTick)
    {
        if (sortedPartyIndex >= sortedPartyAttackSequence.Count) return;

        sortedPartyAttackSequence[sortedPartyIndex].CircleManager.SpawnReduceCircle(startTime, endTime, arrow, timingType, targetTick);
    }

    public void ShowCommandFailed(int sortedPartyIndex)
    {
        sortedPartyAttackSequence[sortedPartyIndex].CommandFailedAnimation();
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
    public void PressedKey(Arrow myInputArrow)
    {
        Character member;
        Accuracy accuracy;

        if (tickManager.TickCount > 16 && nowSkillCaster != null)
        {
            member = nowSkillCaster;
            CircleSpawner spawner = member.CircleManager.GetCircleSpawner(myInputArrow);

            if (spawner.ReduceCircleQueue.TryPeek(out ReduceCircle circle))
            {
                if (myInputArrow != circle.ArrowType)
                {
                    accuracy = Accuracy.Miss;
                }
                else
                {
                    accuracy = tickManager.GetAccuracy(circle.TargetTick);
                }

                member.AttackCommand(accuracy, circle.ArrowType);
            }
            else
            {
                member.AttackCommand(Accuracy.Miss, myInputArrow);
            }
        }
        else
        {
            if (pressCount >= sortedPartyAttackSequence.Count)
                return;
            int skillIndex = (int)myInputArrow;

            member = sortedPartyAttackSequence[pressCount];
            while(member.GetFirstCircleInSpawner(Arrow.Up) == null)
            {
                pressCount++;
                if (pressCount > 3) return;
                member = sortedPartyAttackSequence[pressCount];
            }
            accuracy = tickManager.GetAccuracy(member.GetFirstCircleInSpawner(Arrow.Up).TargetTick);
            member.SkillCommand(accuracy, skillIndex);

            pressCount++;
        }
    }
    #endregion
}

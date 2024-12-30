// # Systems
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Managers")]
    [SerializeField] private TickManager tickManager;
    private float zoomInCamCenter = 1.7f;
    private float zoomInCharPadding = 1.5f;

    private Character previousZoomInCaster;
    private List<Character> previousZoomInTargets = new List<Character>();

    #region Party
    [Header("Party")]
    [SerializeField] private List<Character> partyMembers = new List<Character>(); // 파티멤버
    public List<Character> PartyMembers => partyMembers;

    private List<Character> sortedPartyAttackSequence = new List<Character>();

    private Character nowSkillCaster = null;
    public Character NowSkillCaster => nowSkillCaster;

    [Header("Enemy")]
    [SerializeField] private List<Character> enemyMembers = new List<Character>();
    public List<Character> EnemyMembers => enemyMembers;

    private List<Character> sortedEnemyAttackSequence = new List<Character>();
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

    #region Cam
    public void ZoomInCam(int sign)
    {
        StartCoroutine(camMove.Co_MoveAttackAngle(sign));
    }

    public void ZoomOutCam(int sign)
    {
        StartCoroutine(camMove.Co_MoveDefaultAngle(sign));
    }

    public void ZoomInCharacter(Skill skill)
    {
        Character caster = skill.Caster;
        previousZoomInCaster = caster;

        if(skill.IsEnemySkill)
        {
            StartCoroutine(caster.Co_MoveToCameraFront(zoomInCamCenter));
        }
        else
        {
            StartCoroutine(caster.Co_MoveToCameraFront(-zoomInCamCenter));
        }
    }

    public void ZoomInTargets(Skill skill)
    {
        previousZoomInTargets.Clear();
        skill.GetTargetIndex(out int[] targetArray, out bool isParty);
        float centerPos = zoomInCamCenter;

        if (targetArray.Length > 2)
        {
            centerPos = 1;
        }

        if (skill.IsEnemySkill)
        {
            for (int i = 0; i < targetArray.Length; i++)
            {
                Character character;
                int pm = 1;

                if (!isParty)
                {
                    character = partyMembers[targetArray[i]];
                    pm = -1;
                }
                else
                {
                    character = enemyMembers[targetArray[i]];
                    pm = 1;
                }
                previousZoomInTargets.Add(character);
                StartCoroutine(character.Co_MoveToCameraFront(pm *( centerPos + (i * zoomInCharPadding))));
            }
        }
        else
        {
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
                StartCoroutine(character.Co_MoveToCameraFront(centerPos + (i * zoomInCharPadding)));
            }
        }
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
    #endregion

    #region Party
    public void SetPartySkillCaster(ref Skill skill, int skillCasterIndex)
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

        // 스피드 순서대로 정렬된 파티
        sortedPartyAttackSequence = new List<Character>(partyMembers);
        sortedPartyAttackSequence.Sort((a, b) => b.Speed.CompareTo(a.Speed));
    }

    public void PlaySortedPartyTimingCircle(int sortedPartyIndex, double startTime, double endTime, Arrow arrow, TimingType timingType, NoteType noteType, int targetTick)
    {
        if (sortedPartyIndex >= sortedPartyAttackSequence.Count) return;

        sortedPartyAttackSequence[sortedPartyIndex].CircleManager.SpawnReduceCircle(startTime, endTime, arrow, timingType, targetTick);
    }

    public void PlayPartyTimingCircle(int sortedPartyIndex, double startTime, double endTime, Arrow arrow, TimingType timingType, NoteType noteType, int targetTick)
    {
        if (sortedPartyIndex >= partyMembers.Count) return;

        partyMembers[sortedPartyIndex].CircleManager.SpawnReduceCircle(startTime, endTime, arrow, timingType, targetTick);
    }

    public void ShowCommandFailed(int sortedPartyIndex)
    {
        sortedPartyAttackSequence[sortedPartyIndex].CommandFailedAnimation();
    }
    #endregion

    #region Enemy
    public void SetEnemySkillCaster(ref Skill skill, int skillCasterIndex)
    {
        if (skillCasterIndex > 3)
        {
            nowSkillCaster = null;
            skill = null;
            return;
        }

        nowSkillCaster = sortedEnemyAttackSequence[skillCasterIndex];
        skill = nowSkillCaster.NextSkill;
    }

    public void SortEnemyMember()
    {
        sortedEnemyAttackSequence.Clear();
        sortedEnemyAttackSequence = new List<Character>(enemyMembers);
        sortedEnemyAttackSequence.Sort((a, b) => b.Speed.CompareTo(a.Speed));

        for(int i =0; i < sortedEnemyAttackSequence.Count; i++)
        {
            sortedEnemyAttackSequence[i].SetNextSkill(0);
        }
    }
    #endregion Enemy

    #region Input
    public void PressedKey(Arrow myInputArrow)
    {
        Character member;
        Accuracy accuracy = Accuracy.None;

        if (tickManager.TurnState == TurnState.PlayerCommanding)
        {
            if (pressCount >= sortedPartyAttackSequence.Count)
                return;
            int skillIndex = (int)myInputArrow;

            member = sortedPartyAttackSequence[pressCount];
            while (member.GetFirstCircleInSpawner(Arrow.Up) == null)
            {
                pressCount++;
                if (pressCount > 3) return;
                member = sortedPartyAttackSequence[pressCount];
            }
            accuracy = tickManager.GetAccuracy(member.GetFirstCircleInSpawner(Arrow.Up).TargetTick);
            member.SkillCommand(accuracy, skillIndex);

            pressCount++;
        }
        else if (tickManager.TurnState == TurnState.PlayerAttacking)
        {
            member = nowSkillCaster;
            CircleSpawner spawner = member.CircleManager.GetCircleSpawner(myInputArrow);

            if (spawner.ReduceCircleQueue.TryPeek(out ReduceCircle circle))
            {
                accuracy = tickManager.GetAccuracy(circle.TargetTick);

                member.AttackCommand(accuracy, circle.ArrowType);
            }
            else
            {
                circle = member.CircleManager.TryPeekSpawnerCircle();

                member.AttackCommand(Accuracy.Miss, circle.ArrowType);
            }
        }
        else if (tickManager.TurnState == TurnState.EnemyCommanding)
        {
            nowSkillCaster.NextSkill.GetTargetIndex(out int[] arr, out bool isPartyTarget);

            if(isPartyTarget)
            {

            }
            else
            {
                member = partyMembers[arr[0]];
                CircleSpawner spawner = member.CircleManager.GetCircleSpawner(myInputArrow);

                if (spawner.ReduceCircleQueue.TryPeek(out ReduceCircle circle))
                {
                    
                    accuracy = tickManager.GetAccuracy(circle.TargetTick);
                    partyMembers[arr[0]].GuardCommand(accuracy, circle.ArrowType);

                    for (int i = 1; i < arr.Length; i++)
                    {
                        partyMembers[arr[i]].GuardAnim();
                    }
                }
                else
                {
                    circle = member.CircleManager.TryPeekSpawnerCircle();

                    member.GuardCommand(Accuracy.Miss, circle.ArrowType);
                }
            }

            nowSkillCaster.Attack(accuracy);
        }
    }
    #endregion
}

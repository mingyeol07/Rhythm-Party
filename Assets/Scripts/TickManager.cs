// # Systems
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;




// # Unity
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public enum TurnState
{
    None,
    Ready,
    PlayerCommanding,
    PlayerAttacking,
    EnemyCommanding,
    EnemyAttacking,
}

public class TickManager : MonoBehaviour
{
    // time
    private double currentTime = 0d; // �ѹ��ھ� 0���� �ʱ�ȭ��
    private double realCurrentTime = 0d; // �ݹ��ھ� 0���� �ʱ�ȭ��
    private double absoluteTime = 0d; // ���� ���� ���� �ð�

    // music
    [SerializeField] private int bpm = 120;

    // ��������
    private double criticalTolerance = 0.05d; // ��� ���� ���� 0.05 == 50ms
    private double strikeTolerance = 0.125d;
    private double hitTolerance = 0.2d;

    // ��
    private int circleWaitTickOne = 2; // ���� ������ ���� �� ƽ ������ ����
    private int circleWaitTickTwo = 4;
    private double circleWaitTimeOne; 
    private double circleWaitTimeTwo; // ���� �����ֱ� �� ������ Ÿ�̹��� ���̸� ���
    private int attackWaitTime = 8; // ���ڸ� ������ ���� �󸶳� ��ٸ������� ��

    private double enemyCircleWaitTime;

    // ƽ
    private int tickCount = 0; // �����ϴ� ƽ ī��Ʈ
    public int TickCount => tickCount; // �����ϴ� ƽ ī��Ʈ

    // ���� Ÿ�ֵ̹�
    // 1, 3, 5, 7, �غ񸶵�
    // 9, 11, 13, 15, Ŀ�ǵ帶��
    // 17, 19, 21, 23, ���ݸ���
    // 25, 27, 29, 31, �� Ŀ�ǵ帶��? ~
    // 33, 35, 37, 39 �� ���ݸ���~

    // ��
    private TurnState turnState = TurnState.None;
    public TurnState TurnState => turnState;
    private bool changeSkillCasterFlag = false;
    private int attackCharacterIndexCount = 0;

    private double aBeat;
    private double realbeat;

    private Skill castingSkill;

    private int attackCommandBeforeWaitCount;
    private int attackCommandAfterWaitCount;

    private Queue<Note> noteQueue = new Queue<Note>();
    private Dictionary<int, Arrow> noteDict = new Dictionary<int, Arrow>();
    private Dictionary<int, Arrow> longNoteExitDict = new Dictionary<int, Arrow>();

    private GameManager gameManager;

    private void Start()
    {
        aBeat = (60d / bpm);
        realbeat = (30d / bpm); // �ݹ��ڸ� ��������

        circleWaitTimeTwo = aBeat * circleWaitTickTwo;
        circleWaitTimeOne = aBeat * circleWaitTickOne;

        gameManager = GameManager.Instance;

        // test

        turnState = TurnState.PlayerCommanding;
    }

    private void Update()
    {
        absoluteTime = Time.time;
        realCurrentTime = absoluteTime % realbeat;
        currentTime = absoluteTime % aBeat;

        // ���ڰ� ������ ��!! ���� �����ϴ� ����
        if (realCurrentTime < Time.deltaTime)
        {
            CalculationTurn();

            if(tickCount % 2 != 0)
            {
                // �����̶�� (Ȧ���� ����)
                gameManager.BounceAnimation();
            }
        }
    }

    private void CalculationTurn()
    {
        if (tickCount == 16)
        {
            turnState = TurnState.PlayerAttacking;
            attackCharacterIndexCount = 0;
        }

        tickCount++;

        if(turnState == TurnState.PlayerCommanding)
        {
            PartyCommand();
        }

        if (turnState == TurnState.EnemyCommanding)
        {
            EnemyAttack();
        }

        if (turnState == TurnState.PlayerAttacking)
        {
            PartyAttack();
        }
    }
   
    private void PartyCommand()
    {
        // �÷��̾��� ����Ŀ�ǵ� Ÿ�̹��� 5,6,7,8�� 1�� ���� �̸� ��Ŭ�ִϸ��̼� ����
        if (tickCount == 5)
        {
            noteDict.Add(tickCount + circleWaitTickOne, Arrow.None);
            gameManager.PlayPartyTimingCircle(0, realCurrentTime, circleWaitTimeOne, Arrow.Up, TimingType.Command, NoteType.Short, tickCount + circleWaitTickOne);
        }
        if (tickCount == 7)
        {
            noteDict.Add(tickCount + circleWaitTickOne, Arrow.None);
            gameManager.PlayPartyTimingCircle(1, realCurrentTime, circleWaitTimeOne, Arrow.Up, TimingType.Command, NoteType.Short, tickCount + circleWaitTickOne);
        }
        if (tickCount == 9)
        {
            noteDict.Add(tickCount + circleWaitTickOne, Arrow.None);
            gameManager.PlayPartyTimingCircle(2, realCurrentTime, circleWaitTimeOne, Arrow.Up, TimingType.Command, NoteType.Short, tickCount + circleWaitTickOne);
        }
        if (tickCount == 11)
        {
            noteDict.Add(tickCount + circleWaitTickOne, Arrow.None);
            gameManager.PlayPartyTimingCircle(3, realCurrentTime, circleWaitTimeOne, Arrow.Up, TimingType.Command, NoteType.Short, tickCount + circleWaitTickOne);
        }
    }

    private void PartyAttack()
    {
        if (attackCommandAfterWaitCount > 0)
        {
            attackCommandAfterWaitCount--;
            return;
        }

        if (!changeSkillCasterFlag)
        {
            gameManager.SetPartySkillCaster(ref castingSkill, attackCharacterIndexCount);

            if (gameManager.NowSkillCaster == null)
            {
                gameManager.ZoomOutCharacter();
                gameManager.ZoomOutTargets();
                gameManager.ZoomOutCam();

                gameManager.SortEnemyMember();
                turnState = TurnState.EnemyCommanding;
                attackCharacterIndexCount = 0;
                changeSkillCasterFlag = false;

                if (tickCount % 2 != 0)
                {
                    attackCommandAfterWaitCount = 5;
                }
                else
                {
                    attackCommandAfterWaitCount = 4;
                }
                return;
            }

            if(castingSkill != null)
            {
                gameManager.ZoomOutCharacter();
                gameManager.ZoomInCharacter(castingSkill);

                gameManager.ZoomOutTargets();
                gameManager.ZoomInTargets(castingSkill);

                GameManager.Instance.ZoomInCam();
                castingSkill.GetSkillCommandList(ref noteQueue);
            }
            else
            {
                gameManager.ZoomOutCam();
            }

            changeSkillCasterFlag = true;
            attackCommandBeforeWaitCount = 4;
        }

        if(attackCommandBeforeWaitCount > 0)
        {
            attackCommandBeforeWaitCount--;
            return;
        }

        if (castingSkill != null)
        {
            Note note = noteQueue.Dequeue();
            if (note.Dir != Arrow.None)
            {
                if(note.Type == NoteType.Long)
                {
                    longNoteExitDict.Add(tickCount + circleWaitTickOne + note.NoteTime, note.Dir);
                }
                else
                {
                    noteDict.Add(tickCount + circleWaitTickOne, note.Dir);
                }

                gameManager.PlayPartyTimingCircle (
                    attackCharacterIndexCount, 
                    realCurrentTime, circleWaitTimeOne, 
                    note.Dir, TimingType.Attack, note.Type, 
                    tickCount + circleWaitTickOne);
            }
        }
        else
        {
             gameManager.ShowCommandFailed(attackCharacterIndexCount);
        }

        if (noteQueue.Count == 0)
        {
            changeSkillCasterFlag = false;
            attackCharacterIndexCount++;

            if(castingSkill == null)
                return;

            if(tickCount % 2 != 0)
            {
                attackCommandAfterWaitCount = 5;
            }
            else
            {
                attackCommandAfterWaitCount = 4;
            }
        }
    }

    private void EnemyAttack()
    {
        if (attackCommandAfterWaitCount > 0)
        {
            attackCommandAfterWaitCount--;
            return;
        }

        if (!changeSkillCasterFlag)
        {
            gameManager.SetEnemySkillCaster(ref castingSkill, attackCharacterIndexCount);

            if (gameManager.NowSkillCaster == null)
            {
                gameManager.ZoomOutCharacter();
                gameManager.ZoomOutTargets();
                gameManager.ZoomOutCam();

                turnState = TurnState.Ready;
                return;
            }

            if (castingSkill != null)
            {
                gameManager.ZoomOutCharacter();
                gameManager.ZoomInCharacter(castingSkill);

                gameManager.ZoomOutTargets();
                gameManager.ZoomInTargets(castingSkill);

                gameManager.ZoomInCam();

                castingSkill.GetSkillCommandList(ref noteQueue);
            }
            else
            {
                gameManager.ZoomOutCam();
            }

            changeSkillCasterFlag = true;
            attackCommandBeforeWaitCount = 4;
        }

        if (attackCommandBeforeWaitCount > 0)
        {
            attackCommandBeforeWaitCount--;
            return;
        }

        if (castingSkill != null)
        {
            Note note = noteQueue.Dequeue();

            castingSkill.GetTargetIndex(out int[] arr, out bool isPartyTarget);

            if (isPartyTarget) return;

            if (note.Dir != Arrow.None)
            {
                if (note.Type == NoteType.Long)
                {
                    longNoteExitDict.Add(tickCount + circleWaitTickOne + note.NoteTime, note.Dir);
                }
                else
                {
                    noteDict.Add(tickCount + circleWaitTickOne, note.Dir);
                }

                gameManager.PlayPartyTimingCircle(
                    arr[0],
                    realCurrentTime, circleWaitTimeOne,
                    note.Dir, TimingType.Attack, note.Type,
                    tickCount + circleWaitTickOne);
            }
        }

        if (noteQueue.Count == 0)
        {
            changeSkillCasterFlag = false;
            attackCharacterIndexCount++;

            if (castingSkill == null)
                return;

            if (tickCount % 2 != 0)
            {
                attackCommandAfterWaitCount = 5;
            }
            else
            {
                attackCommandAfterWaitCount = 4;
            }
        }
    }

    public Accuracy GetAccuracy(int targetTick)
    {
        if (!noteDict.ContainsKey(targetTick))
        {
            return Accuracy.Miss;
        }

        if (tickCount < targetTick)
        {
            return Accuracy.Miss;
        }

        double time = 0;
        double beat = 0;
        if (targetTick % 2 != 0)
        {
            time = currentTime;
            beat = aBeat;
        }
        else
        {
            time = realCurrentTime;
            beat = realbeat;
        }

        if (tickCount == targetTick + 1)
        {
            if (Mathf.Abs((float)(time - beat)) <= criticalTolerance)
            {
                return Accuracy.Critical;
            }
            else if (Mathf.Abs((float)(time - beat)) <= strikeTolerance)
            {
                return Accuracy.Strike;
            }
            else if (Mathf.Abs((float)(time - beat)) <= hitTolerance)
            {
                return Accuracy.Hit;
            }
        }
        else if(tickCount == targetTick + 2)
        {
            if (Mathf.Abs((float)time) <= criticalTolerance)
            {
                return Accuracy.Critical;
            }
            else if (Mathf.Abs((float)time) <= strikeTolerance)
            {
                return Accuracy.Strike;
            }
            else if (Mathf.Abs((float)time) <= hitTolerance)
            {
                return Accuracy.Hit;
            }
        }

        // 5/5�� ���� ƽ�̶��
        // Mathf.Abs((float)(currentTime - 60d / bpm)) ���� ƽ�� �󸶳� �����޴��� 4/5
        // Mathf.Abs((float)currentTime) <= tolerance ����ƽ���� �󸶳� �����ƴ��� 6/5

        return Accuracy.Miss;
    }
}

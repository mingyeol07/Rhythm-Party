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
    private bool changeSkillCasterFlag = false;
    private int attackCharacterIndexCount = 0;

    private double aBeat;
    private double realbeat;

    private Skill castingSkill;

    private int attackCommandBeforeWaitCount;
    private int attackCommandAfterWaitCount;

    private Queue<Note> noteQueue = new Queue<Note>();
    private Dictionary<int, Arrow> tickDict = new Dictionary<int, Arrow>();

    private void Start()
    {
        aBeat = (60d / bpm);
        realbeat = (30d / bpm); // �ݹ��ڸ� ��������

        circleWaitTimeTwo = aBeat * circleWaitTickTwo;
        circleWaitTimeOne = aBeat * circleWaitTickOne;
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
                GameManager.Instance.BounceAnimation();
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

        // �÷��̾��� ����Ŀ�ǵ� Ÿ�̹��� 5,6,7,8�� 1�� ���� �̸� ��Ŭ�ִϸ��̼� ����
        if(tickCount == 5)
        {
            tickDict.Add(tickCount + circleWaitTickOne, Arrow.None);
            GameManager.Instance.PlayPartyTimingCircle(0, realCurrentTime, circleWaitTimeOne, Arrow.Up, TimingType.Command, NoteType.Short, tickCount + circleWaitTickOne);
        }
        if (tickCount == 7)
        {
            tickDict.Add(tickCount + circleWaitTickOne, Arrow.None);
            GameManager.Instance.PlayPartyTimingCircle(1, realCurrentTime, circleWaitTimeOne, Arrow.Up, TimingType.Command, NoteType.Short, tickCount + circleWaitTickOne);
        }
        if (tickCount == 9)
        {
            tickDict.Add(tickCount + circleWaitTickOne, Arrow.None);
            GameManager.Instance.PlayPartyTimingCircle(2, realCurrentTime, circleWaitTimeOne, Arrow.Up, TimingType.Command, NoteType.Short, tickCount + circleWaitTickOne);
        }
        if (tickCount == 11)
        {
            tickDict.Add(tickCount + circleWaitTickOne, Arrow.None);
            GameManager.Instance.PlayPartyTimingCircle(3, realCurrentTime, circleWaitTimeOne, Arrow.Up, TimingType.Command, NoteType.Short, tickCount + circleWaitTickOne);
        }

        if(turnState == TurnState.PlayerAttacking)
        {
            PartyCommandAttack();
        }
    }
   
    private void PartyCommandAttack()
    {
        if (attackCommandAfterWaitCount > 0)
        {
            attackCommandAfterWaitCount--;
            return;
        }

        if (!changeSkillCasterFlag)
        {
            GameManager.Instance.SetNowSkillCaster(ref castingSkill, attackCharacterIndexCount);

            if (GameManager.Instance.NowSkillCaster == null)
            {
                GameManager.Instance.ZoomOutCharacter();
                GameManager.Instance.ZoomOutTargets();
                turnState = TurnState.EnemyCommanding;

                GameManager.Instance.ZoomOutCam();
                return;
            }

            if(castingSkill != null)
            {
                GameManager.Instance.ZoomOutCharacter();
                GameManager.Instance.ZoomInCharacter(castingSkill);

                GameManager.Instance.ZoomOutTargets();
                GameManager.Instance.ZoomInTargets(castingSkill);

                GameManager.Instance.ZoomInCam();
                castingSkill.GetSkillCommandList(ref noteQueue);
            }
            else
            {
                GameManager.Instance.ZoomOutCam();
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
                tickDict.Add(tickCount + circleWaitTickOne, note.Dir);

                GameManager.Instance.PlayPartyTimingCircle(
                    attackCharacterIndexCount, 
                    realCurrentTime, circleWaitTimeOne, 
                    note.Dir, TimingType.Attack, note.Type, 
                    tickCount + circleWaitTickOne);
            }
        }
        else
        {
             GameManager.Instance.ShowCommandFailed(attackCharacterIndexCount);
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

    public Accuracy GetAccuracy(int targetTick)
    {
        if (!tickDict.ContainsKey(targetTick))
        {
            return Accuracy.Miss;
        }
      
        if (tickCount < targetTick - 1)
        {
            return Accuracy.Miss;
        }

        double time = 0;
        double beat = 0;
        if(targetTick % 2 != 0)
        {
            time = currentTime;
            beat = aBeat;
        }
        else
        {
            time = realCurrentTime;
            beat = realbeat;
        }
        // 5/5�� ���� ƽ�̶��
        // Mathf.Abs((float)(currentTime - 60d / bpm)) ���� ƽ�� �󸶳� �����޴��� 4/5
        // Mathf.Abs((float)currentTime) <= tolerance ����ƽ���� �󸶳� �����ƴ��� 6/5

        if (Mathf.Abs((float)time) <= criticalTolerance || Mathf.Abs((float)(time - beat)) <= criticalTolerance)
        {
            return Accuracy.Critical;
        }
        else if (Mathf.Abs((float)time) <= strikeTolerance || Mathf.Abs((float)(time - beat)) <= strikeTolerance)
        {
            return Accuracy.Strike;
        }
        else if (Mathf.Abs((float)time) <= hitTolerance || Mathf.Abs((float)(time - beat)) <= hitTolerance)
        {
            return Accuracy.Hit;
        }

        return Accuracy.Miss;
    }
}

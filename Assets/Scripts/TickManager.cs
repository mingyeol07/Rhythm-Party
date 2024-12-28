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
    private double currentTime = 0d; // 한박자씩 0으로 초기화됨
    private double realCurrentTime = 0d; // 반박자씩 0으로 초기화됨
    private double absoluteTime = 0d; // 오차 없는 절대 시간

    // music
    [SerializeField] private int bpm = 120;

    // 오차범위
    private double criticalTolerance = 0.05d; // 허용 오차 범위 0.05 == 50ms
    private double strikeTolerance = 0.125d;
    private double hitTolerance = 0.2d;

    // 원
    private int circleWaitTickOne = 2; // 원을 때리기 전이 몇 틱 전인지 정의
    private int circleWaitTickTwo = 4;
    private double circleWaitTimeOne; 
    private double circleWaitTimeTwo; // 원을 보여주기 전 시점과 타이밍의 차이를 계산
    private int attackWaitTime = 8; // 박자를 누르고 나서 얼마나 기다리는지의 값

    private double enemyCircleWaitTime;

    // 틱
    private int tickCount = 0; // 증가하는 틱 카운트
    public int TickCount => tickCount; // 증가하는 틱 카운트

    // 정박 타이밍들
    // 1, 3, 5, 7, 준비마디
    // 9, 11, 13, 15, 커맨드마디
    // 17, 19, 21, 23, 공격마디
    // 25, 27, 29, 31, 적 커맨드마디? ~
    // 33, 35, 37, 39 적 공격마디~

    // 턴
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
        realbeat = (30d / bpm); // 반박자를 세기위함

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

        // 박자가 끝나고 뿜!! 새로 시작하는 시점
        if (realCurrentTime < Time.deltaTime)
        {
            CalculationTurn();

            if(tickCount % 2 != 0)
            {
                // 정박이라면 (홀수가 정박)
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
        // 플레이어의 어택커맨드 타이밍인 5,6,7,8의 1초 전에 미리 서클애니메이션 진행
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

        // 5/5가 다음 틱이라면
        // Mathf.Abs((float)(currentTime - 60d / bpm)) 다음 틱에 얼마나 근접햇는지 4/5
        // Mathf.Abs((float)currentTime) <= tolerance 이전틱에서 얼마나 지나쳤는지 6/5

        return Accuracy.Miss;
    }
}

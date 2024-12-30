// # Systems
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;


// # Unity
using UnityEngine;

public class Character : MonoBehaviour
{
    protected Skill[] skills;

    private int hp = 999;
    [SerializeField] private int speed;
    public int Speed => speed;

    private Skill nextSkill;
    public Skill NextSkill => nextSkill;

    [Header("Component")]
    [SerializeField] private CircleManager circleManager;
    public CircleManager CircleManager => circleManager;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator bounceAnimator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("UI")]
    [SerializeField] private DamageText damageText;
    [SerializeField] private Animator commandFailedAnimator;

    private Vector3 defaultPosition;
    private bool isMoveToCam;

    private readonly int hashTrigCommandFailed = Animator.StringToHash("Failed");
    private readonly int hashTrigBounce = Animator.StringToHash("Bounce");

    private readonly int hashTrigExit = Animator.StringToHash("Exit");
    private readonly int hashTrigCommand = Animator.StringToHash("Command");
    private readonly int hashTrigDamage = Animator.StringToHash("Damage");

    private readonly int hashTrigAtkLeft = Animator.StringToHash("AttackLeft");
    private readonly int hashTrigAtkUp = Animator.StringToHash("AttackUp");
    private readonly int hashTrigAtkDown = Animator.StringToHash("AttackDown");
    private readonly int hashTrigAtkRight = Animator.StringToHash("AttackRight");

    protected virtual void Start()
    {
        defaultPosition = transform.position;
    }

    private void Update()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            Debug.Log(gameObject.name);
            Debug.Log(skills[i]);
        }
    }

    public void Damaged(int damage)
    {
        damageText.TextPlay(damageText.transform, damage);
        //animator.SetTrigger(hashTrigDamage);

        hp -= damage;
        if (hp < 0)
        {
            Die();
        }
    }

    private void Die()
    {

    }

    public void Attack(Accuracy accuracy)
    {
        if (nextSkill == null)
        {
            return;
        }

        int calculationDamage = nextSkill.DamageCalculation(accuracy);
        nextSkill.Activate(calculationDamage);
    }

    public void BounceAnimation()
    {
        bounceAnimator.SetTrigger(hashTrigBounce);
    }

    public void ReBounce()
    {
        animator.SetTrigger(hashTrigExit);
    }

    public ReduceCircle GetFirstCircleInSpawner(Arrow arrow)
    {
        if (circleManager.GetCircleSpawner(arrow).ReduceCircleQueue.Count > 0)
        {
            return circleManager.GetCircleSpawner(arrow).ReduceCircleQueue.Peek();
        }

        return null;
    }

    public void AttackCommand(Accuracy accuracy, Arrow arrow)
    {
        circleManager.PressedAttackCommand(accuracy, arrow);

        switch (arrow)
        {
            case Arrow.Left:
                animator.SetTrigger(hashTrigAtkLeft);
                break;
            case Arrow.Up:
                animator.SetTrigger(hashTrigAtkUp);
                break;
            case Arrow.Down:
                animator.SetTrigger(hashTrigAtkDown);
                break;
            case Arrow.Right:
                animator.SetTrigger(hashTrigAtkRight);
                break;
        }

        Attack(accuracy);
    }

    public void GuardCommand(Accuracy accuracy, Arrow arrow)
    {
        circleManager.PressedGuardCommand(accuracy, arrow);
    }

    public void GuardAnim()
    {

    }

    public void SkillCommand(Accuracy accuracy, int skillIndex)
    {
        animator.SetTrigger(hashTrigCommand);
        circleManager.PressedCommand(accuracy);

        // 스킬 세팅
        SetNextSkill(skillIndex);
    }

    public void SetNextSkill(int skillIndex)
    {
        nextSkill = skills[skillIndex];
    }

    public void CommandFailedAnimation()
    {
        commandFailedAnimator.SetTrigger(hashTrigCommandFailed);
    }

    public IEnumerator Co_MoveToCameraFront(float x)
    {
        spriteRenderer.sortingLayerName = "ZoomInCharacter";
        isMoveToCam = true;

        Vector3 endPos = new Vector3(x, 0.5f, 0);
        Vector3 endScale = new Vector3(1.5f, 1.5f, 1.5f);

        float maxTime = 0.2f;
        float time = 0;

        while(time < maxTime)
        {
            time += Time.deltaTime;
            float t = time / maxTime;
            transform.position = Vector3.Lerp(defaultPosition, endPos, t);
            transform.localScale = Vector3.Lerp(Vector3.one, endScale, t);
            yield return null;
        }

        transform.position = endPos;
        transform.localScale = endScale;
        isMoveToCam = false;
    }

    public IEnumerator Co_ReturnToInPlace()
    {
        spriteRenderer.sortingLayerName = "Character";

        Vector3 nowPos = transform.position;
        Vector3 nowScale = new Vector3(1.5f, 1.5f, 1.5f);

        float maxTime = 0.2f;
        float time = 0;

        while (time < maxTime)
        {
            if (isMoveToCam)
            {
                isMoveToCam = false;
                yield break;
            }
            time += Time.deltaTime;
            float t = time / maxTime;
            transform.position = Vector3.Lerp(nowPos, defaultPosition, t);
            transform.localScale = Vector3.Lerp(nowScale, Vector3.one, t);
            yield return null;
        }

        transform.position = defaultPosition;
        transform.localScale = Vector3.one;
    }
}

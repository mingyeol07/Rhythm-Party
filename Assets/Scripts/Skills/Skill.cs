// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class Skill : MonoBehaviour
{
    protected int damage;
    protected int[] targetIndex; // 0, 1, 2, 3?
    
    /// <summary>
    /// ��ų ����
    /// </summary>
    public virtual void Activate()
    {
        Debug.Log("��ų �ߵ�");
    }
}
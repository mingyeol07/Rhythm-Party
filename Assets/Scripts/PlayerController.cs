// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

/// <summary>
/// ĳ���Ϳ� ����� ��ų���� ������ ȭ��ǥ�� ������ ����..?
/// </summary>
public enum Arrow
{
    None = 99,
    Left = 0,
    Up = 1,
    Down = 2,
    Right = 3,
}

public class PlayerController : MonoBehaviour
{
    private KeyCode pressedKey = KeyCode.None;

    private void Update()
    {      
        if (pressedKey == KeyCode.None)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                GameManager.Instance.PressedKey(Arrow.Down);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                GameManager.Instance.PressedKey(Arrow.Right);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                GameManager.Instance.PressedKey(Arrow.Left);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                GameManager.Instance.PressedKey(Arrow.Up);
            }
        }
    }
}

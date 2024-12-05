// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

/// <summary>
/// ĳ���Ϳ� ����� ��ų���� ������ ȭ��ǥ�� ������ ����..?
/// </summary>
public enum KeyIndexInArray
{
    UP,
    LEFT,
    DOWN,
    RIGHT,
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
                GameManager.Instance.PressedKey(KeyIndexInArray.DOWN);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                GameManager.Instance.PressedKey(KeyIndexInArray.RIGHT);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                GameManager.Instance.PressedKey(KeyIndexInArray.LEFT);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                GameManager.Instance.PressedKey(KeyIndexInArray.UP);
            }
        }
    }
}

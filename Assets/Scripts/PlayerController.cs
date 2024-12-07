// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

/// <summary>
/// 캐릭터에 저장된 스킬들의 순서와 화살표의 순서를 맞춤..?
/// </summary>
public enum KeyIndexInArray
{
    None = 99,
    LEFT = 0,
    UP = 1,
    DOWN = 2,
    RIGHT = 3,
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
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                GameManager.Instance.PressedKey(KeyIndexInArray.RIGHT);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                GameManager.Instance.PressedKey(KeyIndexInArray.LEFT);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                GameManager.Instance.PressedKey(KeyIndexInArray.UP);
            }
        }
    }
}

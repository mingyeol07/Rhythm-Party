// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private KeyCode pressedKey = KeyCode.None;

    private void Update()
    {      
        if (pressedKey == KeyCode.None)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                GameManager.Instance.PressedKey();
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                GameManager.Instance.PressedKey();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                GameManager.Instance.PressedKey();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                GameManager.Instance.PressedKey();
            }
        }
    }
}

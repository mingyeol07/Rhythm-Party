// # Systems
using System.Collections;
using System.Collections.Generic;

// # Unity
using UnityEngine;

public class CamMoving : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public IEnumerator Co_MoveAttackAngle(int sign)
    {
        Vector3 endRot = new Vector3(0, 0, 3 * sign);
        float maxTime = 0.2f;
        float time = 0;

        while (time < maxTime)
        {
            time += Time.deltaTime;
            float t = time / maxTime;
            transform.eulerAngles = Vector3.Lerp(Vector3.zero, endRot, t);
            cam.orthographicSize = Mathf.Lerp(5, 4, t);
            yield return null;
        }

        transform.eulerAngles = endRot;
    }

    public IEnumerator Co_MoveDefaultAngle(int sign)
    {
        Vector3 startRot = new Vector3(0, 0, 3 * sign);
        float maxTime = 0.2f;
        float time = 0;

        while (time < maxTime)
        {
            time += Time.deltaTime;
            float t = time / maxTime;
            transform.eulerAngles = Vector3.Lerp(startRot, Vector3.zero, t);
            cam.orthographicSize = Mathf.Lerp(4, 5, t);
            yield return null;
        }

        transform.eulerAngles = Vector3.zero;
    }
}

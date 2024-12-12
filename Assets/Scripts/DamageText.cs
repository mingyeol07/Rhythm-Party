// # Systems
using System.Collections;
using System.Collections.Generic;
using TMPro;


// # Unity
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    private Stack<GameObject> textList = new Stack<GameObject>();

    public void TextPlay(Transform parent, int damageValue)
    {
        int index = textList.Count;
        Vector3 pos = new Vector3(0, index * 0.5f, 0);

        GameObject textPrefab = Instantiate(prefab, parent);
        textPrefab.transform.localPosition = pos;
        textPrefab.GetComponentInChildren<TMP_Text>().text = damageValue.ToString();

        textList.Push(textPrefab);
        StartCoroutine(Co_RemoveText(textPrefab));
    }

    private IEnumerator Co_RemoveText(GameObject text)
    {
        yield return new WaitForSeconds(2);
        textList.Pop();
        Destroy(text);
    }
}

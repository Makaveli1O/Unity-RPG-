using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class QuestWindowPointer : MonoBehaviour
{
    [SerializeField] private List<GameObject> pointers;   //quest direction pointers
    [SerializeField] private GameObject mapObj;
    private Map map;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Awake()
    {
        map = mapObj.GetComponent<Map>();
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        StartCoroutine(InitPointers());
    }

    private IEnumerator InitPointers(){
        yield return new WaitUntil(()=>map.generationComplete);
        foreach (KeyObject keyObject in map.IncompleteKeyStones)
        {
            //get pointer object
            GameObject pointer = pointers[0];
            pointers.RemoveAt(0);
            //get reference and start pointing to given keyObject
            QuestUIPointer pointerSctipt = pointer.GetComponent<QuestUIPointer>();
            pointerSctipt.StartPointing(keyObject);
        }
    }
}

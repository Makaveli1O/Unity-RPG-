using UnityEngine;
using System.Collections;

public interface ObjectPoolInterface{
    void Awake();
    GameObject GetPooledObject();
}

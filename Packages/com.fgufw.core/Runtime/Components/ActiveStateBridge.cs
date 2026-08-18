using System.Collections;
using System.Collections.Generic;
using FGUFW;
using UnityEngine;

[ExecuteAlways]
public class ActiveStateBridge : MonoBehaviour
{
    public GameObject[] Targets;
    public bool Invert;

    void OnValidate()
    {
        resetTargetsActive();
    }

    void OnEnable()
    {
       resetTargetsActive();
    }

    void OnDisable()
    {
        resetTargetsActive();
    }

    void resetTargetsActive()
    {
        if(Targets==default)return;
        
        var active = Invert?!gameObject.activeInHierarchy:gameObject.activeInHierarchy;
        foreach (var target in Targets)
        {
            if(!target.IsNull())
            {
                target.SetActive(active);
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EditorUIController : MonoBehaviour
{
    [SerializeField] private Transform[] _toggles;

    public void PunchButtonBasic(bool value)
    {
        if (value)
            _toggles[0].DOPunchScale(new(0f, 0.2f, 0f), 0.5f);
    }

    public void PunchButtonControl(bool value)
    {
        if (value)
            _toggles[1].DOPunchScale(new(0f, 0.2f, 0f), 0.5f);
    }
}

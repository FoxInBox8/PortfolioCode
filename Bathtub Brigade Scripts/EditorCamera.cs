using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class EditorCamera : MonoBehaviour
{
    [SerializeField]
    private float editorXSensitivity, editorYSensitivity;

    private CinemachineFreeLook cam;

    private void Awake()
    {
        // Use editor-specific mouse sensitivity because Cinemachine kinda doodoo
        if(Application.isEditor)
        {
            cam = GetComponent<CinemachineFreeLook>();

            cam.m_XAxis.m_MaxSpeed = editorXSensitivity;
            cam.m_YAxis.m_MaxSpeed = editorYSensitivity;
        }
    }
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    bool m_locked = false;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !m_locked)
        {
            m_locked = true;
            SceneManager.LoadScene(0);
        }
    }
}

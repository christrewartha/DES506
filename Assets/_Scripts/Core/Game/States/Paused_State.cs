﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paused_State : GameCore.System.State
{
    public Paused_State(GameCore.System.Automaton owner) : base(owner)
    {
        Time.timeScale = 0;
        Debug.Log("Game is paused");
    }

    public override void Manage() 
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            m_owner.SetState(new Playing_State(m_owner));
        }
    }
}
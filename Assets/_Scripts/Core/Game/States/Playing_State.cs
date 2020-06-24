﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.GameState
{

    public class Playing_State : GameCore.System.State
    {

        //"Resume" button functionality will come from the button calling thisObject.setState(new PlayingState);
        //I'd like to do this programatically but it's pretty redundant when Automaton already has this functionality.

        GameStateController controller;

        public Playing_State(GameCore.System.Automaton owner) : base(owner)
        {
            Time.timeScale = 1.0f;

            Cursor.visible = false;

            if (!owner.TryGetComponent<GameStateController>(out controller))
            {
                Debug.Log("Failed to find GameStateController script from owner");
            }

            controller.GetPauseMenu().SetActive(false);

            Debug.Log("Game is unpaused");
        }

        public override void Manage()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                m_owner.SetState(new Paused_State(m_owner));
            }
        }
    }
}
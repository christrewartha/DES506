﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateDefault : NPCState
{

    /// <summary>
    /// This is the constructor just for start up, not for switching states
    /// </summary>
    public NPCStateDefault(Animator animator, string anim, LetterBox letterBox) : base(animator, anim)
    {
        letterBox.TurnOff();
        animator.SetBool("isTalking", false);
        m_endOfConvo = false;
    }

    /// <summary>
    /// Call each frame to update and perform NPC related operations (Default state override)
    /// </summary>
    public override void StateUpdate()
    {
        base.StateUpdate();
    }
}
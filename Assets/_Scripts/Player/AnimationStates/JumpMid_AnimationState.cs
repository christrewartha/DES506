﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class JumpMid_AnimationState : GameCore.System.State
    {
        PlayerAnimator m_playerAnimator;

        public JumpMid_AnimationState(GameCore.System.Automaton owner) : base(owner)
        {
            m_playerAnimator = (PlayerAnimator)owner;

            m_playerAnimator.Animation.wrapMode = WrapMode.Once;
            m_playerAnimator.StopAllCoroutines();
            m_playerAnimator.StartCoroutine(Transition());
        }
        //You might be thinking "why use a switch statement here? surely it's better and more efficent to just change the state from within the player entity class!" 
        //while this might be true, this allows us to control which states can be transitioned into others.
        public override void Manage()
        {
            switch (m_playerAnimator.PlayerAnimProperties)
            {
                case PlayerAnimationProperties.FALLING:
                    m_owner.SetState(new Falling_AnimationState(m_owner));
                    break;
            }
        }

        IEnumerator Transition()
        {
            m_playerAnimator.Animation.Play("jumpMid", PlayMode.StopAll);
            while (m_playerAnimator.Animation.isPlaying)
            {
                //do nothing
                yield return null;
            }
            yield break;
        }
    }
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{

    public class SlidingMid_AnimationState : GameCore.System.State
    {
        PlayerAnimator m_playerAnimator;

        public SlidingMid_AnimationState(GameCore.System.Automaton owner) : base(owner)
        {
            m_playerAnimator = (PlayerAnimator)m_owner;

            m_playerAnimator.Animation.wrapMode = WrapMode.Loop;
            m_playerAnimator.StopAllCoroutines();
            m_playerAnimator.StartCoroutine(Transition());

            //Debug.Log("Sliding");
        }
        //You might be thinking "why use a switch statement here? surely it's better and more efficent to just change the state from within the player entity class!" 
        //while this might be true, this allows us to control which states can be transitioned into others.
        public override void Manage()
        {
            switch (m_playerAnimator.PlayerAnimProperties)
            {
                case PlayerAnimationProperties.JUMPING:
                    m_playerAnimator.SetState(new Jumping_AnimationState(m_playerAnimator));
                    m_playerAnimator.SetExpression(PlayerFacialExpression.NATURAL);
                    break;
                case PlayerAnimationProperties.RUNNING:
                    m_playerAnimator.SetState(new Running_AnimationState(m_playerAnimator));
                    m_playerAnimator.SetExpression(PlayerFacialExpression.NATURAL);
                    break;
                    //intentionally set to sliding end when in property idle
                case PlayerAnimationProperties.IDLE:
                    m_playerAnimator.SetState(new SlidingEnd_AnimationState(m_playerAnimator));
                    m_playerAnimator.SetExpression(PlayerFacialExpression.NATURAL);
                    break;
                case PlayerAnimationProperties.FALLING:
                    m_playerAnimator.SetState(new Falling_AnimationState(m_playerAnimator));
                    break;
                case PlayerAnimationProperties.FREE_FALLING:
                    m_playerAnimator.SetState(new FreeFalling_AnimationState(m_playerAnimator));
                    break;
            }
        }

        IEnumerator Transition()
        {
            try
            {
                m_playerAnimator.Animation.CrossFadeQueued("slidingMid", 0.2f);
                //m_playerAnimator.Animation.PlayQueued("slidingMid");
            }
            catch
            {
                Debug.LogError("Sliding Mid animation not set in editor or is null for some other reason");
            }
            yield break;
        }
    }
}
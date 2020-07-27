﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class Death_PlayerState : GameCore.System.State
    {
        GameCore.Camera.PlayerMoveCamera m_camera;
        PlayerEntity m_playerEntity;
        bool m_animFinished = false;


        public Death_PlayerState(GameCore.System.Automaton owner) : base(owner)
        {
            if (!Camera.main.transform.TryGetComponent<GameCore.Camera.PlayerMoveCamera>(out m_camera))
            {
                Debug.LogError("Cannot get PlayerMoveCamera Component on Main Camera!");
            }

            m_playerEntity = (PlayerEntity)owner;
            m_playerEntity.transform.position = m_playerEntity.PlayerStartPosition;

            m_playerEntity.Animator.SetProperty(PlayerAnimationProperties.FREE_FALLING);
        }

        public override void Manage()
        {
            if (m_animFinished)
            {
                m_playerEntity.RemoveEntityProperty(PlayerEntityProperties.DYING);
                m_playerEntity.transform.GetChild(0).GetChild(0).transform.localEulerAngles = Vector3.zero;
                m_playerEntity.SetState(new Default_PlayerState(m_playerEntity));
                m_camera.SetState(new GameCore.Camera.Default_CameraState(m_camera));
            }

            if (!m_playerEntity.Grounded)
            {
                m_playerEntity.Velocity -= Vector3.up * m_playerEntity.Gravity * Time.fixedDeltaTime;
            }
            else
            {
                m_playerEntity.Velocity = Vector3.zero;
                m_playerEntity.Animator.SetProperty(PlayerAnimationProperties.RECOVERING);
                m_playerEntity.StartCoroutine(WaitForAnimFinish());
            }
        }

        IEnumerator WaitForAnimFinish()
        {
            while (m_playerEntity.Animator.Animation.isPlaying)
            {
                yield return null;
            }
            m_animFinished = true;
            yield break;
        }
    }
}
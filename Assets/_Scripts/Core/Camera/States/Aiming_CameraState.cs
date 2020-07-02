﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCore.System;

namespace GameCore.Camera
{
    public class Aiming_CameraState : State
    {
        PlayerMoveCamera m_playerMoveCamera;
        UnityEngine.Camera m_camera;
        Vector3 m_rotation;
        Vector3 m_offset;

        bool m_transitioned = false;
        Quaternion m_startRotation;
        Vector3 m_startingPos;
        Vector3 m_endingPos;
        float m_startDistance;

        float m_aimFOV = 45;
        float m_startFOV;

        public Aiming_CameraState(Automaton owner) : base (owner)
        {
            m_playerMoveCamera = (PlayerMoveCamera)owner;

            if (!m_playerMoveCamera.TryGetComponent<UnityEngine.Camera>(out m_camera))
            {
                Debug.LogError("Camera component not found! Camera movement script is not attached to a Camera!");
            }
            m_startFOV = m_camera.fieldOfView;

            m_rotation = Quaternion.LookRotation(new Vector3(m_playerMoveCamera.transform.forward.x, 0, m_playerMoveCamera.transform.forward.z)).eulerAngles;

            m_startRotation = m_playerMoveCamera.transform.rotation;
            m_startDistance = (m_playerMoveCamera.p_CameraTarget.position - m_playerMoveCamera.transform.position).magnitude;

            //Camera transition between current position and expected distance without changing angle.
            m_playerMoveCamera.StopAllCoroutines();
            m_playerMoveCamera.StartCoroutine(Transition());
        }

        //Gets mouse input and rotates around the player by making the camera face the player, the subtracting it's forward vector by the desired distance
        public override void Manage()
        {
            Vector2 input = new Vector2(Input.GetAxis("Camera Y"), Input.GetAxis("Camera X"));
            //if what the camera is pointing at is of interest (enchantable)
            if (m_playerMoveCamera.p_AimedAtTransform != null)
            {
                input /= 2; //half the input, potentially expose this to the editor
                //rotation direction fro aim assist (not normalised yet)
                Quaternion rotationDirection = Quaternion.LookRotation(m_playerMoveCamera.p_AimedAtTransform.position - m_playerMoveCamera.transform.position, Vector3.up);
                m_rotation = Quaternion.RotateTowards(Quaternion.Euler(m_rotation), rotationDirection, Time.deltaTime * m_playerMoveCamera.p_AutoAimStrength).eulerAngles;
                
            }
            m_rotation.x = Mathf.Clamp(m_rotation.x - (input.x * (m_playerMoveCamera.p_AimingMovementSpeed / 2)), m_playerMoveCamera.p_AimingMinAngle, m_playerMoveCamera.p_AimingMaxAngle);
            m_rotation.y += input.y * (m_playerMoveCamera.p_AimingMovementSpeed / 2);

            m_offset = (m_playerMoveCamera.transform.right * m_playerMoveCamera.p_AimingOffset.x) + (m_playerMoveCamera.transform.up * m_playerMoveCamera.p_AimingOffset.y);

            Vector3 targetPosition;

            if (m_transitioned)
            {
                targetPosition = m_playerMoveCamera.p_CameraTarget.position - ((m_playerMoveCamera.transform.forward * m_playerMoveCamera.p_AimingDistance) - m_offset);
                m_playerMoveCamera.transform.position = targetPosition;
                m_playerMoveCamera.transform.eulerAngles = m_rotation;
            }            
        }

        IEnumerator Transition()
        {
            float time = 0;
            float distance = (m_playerMoveCamera.transform.position - m_playerMoveCamera.p_CameraTarget.transform.position).magnitude;
            while (true)
            {
                m_offset = (m_playerMoveCamera.transform.right * m_playerMoveCamera.p_AimingOffset.x) + (m_playerMoveCamera.transform.up * m_playerMoveCamera.p_AimingOffset.y);    //updating offset as the player may be moving
                m_startingPos = m_playerMoveCamera.p_CameraTarget.position - (m_playerMoveCamera.transform.forward * m_startDistance);
                m_endingPos = m_playerMoveCamera.p_CameraTarget.position - ((m_playerMoveCamera.transform.forward * m_playerMoveCamera.p_AimingDistance) - m_offset);
                m_playerMoveCamera.transform.position = Vector3.Lerp(m_startingPos, m_endingPos, time);
                m_playerMoveCamera.transform.rotation = Quaternion.Lerp(m_startRotation, Quaternion.Euler(m_rotation), time);

                m_camera.fieldOfView = Mathf.Lerp(m_startFOV, m_aimFOV, time);

                time += Time.deltaTime * m_playerMoveCamera.p_AimingLerpSpeed;
                time = m_playerMoveCamera.p_LerpCurve.Evaluate(time);

                if (time > 1)
                {
                    m_transitioned = true;
                    yield break;
                }

                yield return null;
            }
        }
    }
}
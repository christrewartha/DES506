﻿using GameCore.Spells;
using GameCore.System;
using GameCore.Utils;
using GameUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace Player
{
    public class Aiming_PlayerState : GameCore.System.State
    {

        PlayerEntity m_playerEntity;
        GameCore.Camera.PlayerMoveCamera m_camera;
        Vector3 m_velocity;

        RaycastHit m_rayHitInfo;
        Transform m_aimedAt = null;
        Renderer m_aimedAtRenderer = null;
        Shader m_highlightedOldShader = null;
        Texture m_baseTexture = null;
        Color m_baseColor;
        bool m_locked = false;

        PlayerEquipableItems m_itemEquipped;

        private RFX4_EffectEvent m_vfxHandler;

        public Aiming_PlayerState(GameCore.System.Automaton owner, RFX4_EffectEvent vfx) : base(owner)
        {
            m_vfxHandler = vfx;

            m_playerEntity = (PlayerEntity)owner;
            m_velocity = m_playerEntity.Velocity;
            if (!Camera.main.transform.TryGetComponent<GameCore.Camera.PlayerMoveCamera>(out m_camera))
            {
                Debug.LogError("Cannot get PlayerMoveCamera Component on Main Camera!");
            }
            //Storing a reference to this state object to transition back to after a fall
            m_playerEntity.PreviousGroundState = PlayerGroundStates.AIMING;

            //SpellWheel state change, probably doesn't need to be a switch statement, but would make it easier in the future if we add more states / items
            switch (m_playerEntity.EquipedItem)
            {
                case PlayerEquipableItems.SPELL_QUILL:
                    m_playerEntity.SpellWheel.SetState(new GameUI.Aiming_SpellWheelState(m_playerEntity.SpellWheel));
                    break;
                case PlayerEquipableItems.ERASER:
                    m_playerEntity.SpellWheel.SetState(new GameUI.Idle_SpellWheelState(m_playerEntity.SpellWheel));
                    break;
                default:
                    m_playerEntity.SpellWheel.SetState(new GameUI.Idle_SpellWheelState(m_playerEntity.SpellWheel));
                    break;
            }

            m_camera.SetState(new GameCore.Camera.Aiming_CameraState(m_camera));

            m_playerEntity.Animator.SetProperty(PlayerAnimationProperties.AIMING);

            m_itemEquipped = m_playerEntity.EquipedItem;
            //temp
            m_playerEntity.Reticle.gameObject.SetActive(true);

            LevelManager.ShowEnchantableParticles(true);
        }

        public override void Manage()
        {
            //if button "aim" up get out of this state, axis is for joystick trigger buttons
            if (!Input.GetButton("Aim") && Input.GetAxisRaw("Aim") == 0)
            {
                m_playerEntity.SpellWheel.SetState(new GameUI.Idle_SpellWheelState(m_playerEntity.SpellWheel));
                m_camera.SetState(new GameCore.Camera.Default_CameraState(m_camera));
                if (m_aimedAt)
                {
                    ResetAimedAt();
                }

                m_playerEntity.Reticle.gameObject.SetActive(false);
                //TEMP REMOVE LATER
                m_owner.SetState(new Default_PlayerState(m_owner));
                return;
            }            
        }

        public override void FixedManage()
        {
            //if we change items in this state, we should activate / deactivate the radial UI (probably not worth keeping as I think we change items through
            //kinda temporary code 
            if (m_itemEquipped != m_playerEntity.EquipedItem)
            {
                m_itemEquipped = m_playerEntity.EquipedItem;
                switch (m_itemEquipped)
                {
                    case PlayerEquipableItems.SPELL_QUILL:
                        m_playerEntity.SpellWheel.SetState(new GameUI.Aiming_SpellWheelState(m_playerEntity.SpellWheel));
                        break;
                    case PlayerEquipableItems.ERASER:
                        m_playerEntity.SpellWheel.SetState(new GameUI.Idle_SpellWheelState(m_playerEntity.SpellWheel));
                        break;
                    default:
                        m_playerEntity.SpellWheel.SetState(new GameUI.Idle_SpellWheelState(m_playerEntity.SpellWheel));
                        break;
                }
            }

            //Casting ray forward from the camera to check if there is an enchantable object where the player is aiming
            if (Physics.Raycast(Camera.main.transform.position + ((m_playerEntity.transform.position - m_camera.transform.position).magnitude * Camera.main.transform.forward), //Moves the position to start parellel to the player
                Camera.main.transform.forward, out m_rayHitInfo, m_playerEntity.Projectile.Range))
            {
                Enchantable enchantable = HierarchyTraverser.RetrieveEnchantableComponent(m_rayHitInfo.transform);
                if (enchantable != null)
                {
                    if (m_aimedAt != enchantable.transform)
                    {
                        //if there is a old shader, then we've been aiming at something else before so change that object's shader back to the old one before setting new variables
                        if (m_highlightedOldShader)
                        {
                            m_aimedAtRenderer.material.shader = m_highlightedOldShader;
                        }

                        m_aimedAt = enchantable.transform;
                        SpellWheel.SetTargetEnchantable(enchantable.transform);
                        Renderer renderer = LevelManager.Instance.GetRenderer(enchantable);
                        if (renderer == null)
                        {
                            Debug.LogError($"Enchantable Object {m_rayHitInfo.transform.name} doesn't have a renderer component!");
                            return;
                        }
                        m_aimedAtRenderer = renderer;
                        
                        //Retaun original attributes
                        m_highlightedOldShader = renderer.material.shader;
                        m_baseTexture = renderer.material.mainTexture;
                        m_baseColor = renderer.material.color;

                        //Set new attributes for highlight effect
                        renderer.material.shader = m_playerEntity.HighlightShader;
                        renderer.material.SetTexture("_Texture", m_baseTexture);
                        renderer.material.SetColor("_Color", m_baseColor);
                    }
                }
                //if the hit object is not enchantable and there is a currently selected enchantable, set to null
                else if (m_aimedAt)
                {
                    ResetAimedAt();
                }
            }
            //not sure about this condition as it is similar to the one above...
            //if it doesn't hit and there is an aimed at transform, set to null also
            else if (m_aimedAt)
            {
                ResetAimedAt();
            }

            //Setting the cameras aimed at transfrom for the aiming camera state. Not sure if we could move the functionality for the shader changing there? or if it makes sense to be here?
            m_camera.p_AimedAtTransform = m_aimedAt;

            if (!m_locked && (Input.GetButtonDown("Fire") || Input.GetAxisRaw("Fire") != 0))            {
                
                if(m_playerEntity.EquipedItem == PlayerEquipableItems.SPELL_QUILL && (!SpellWheel.p_Active || SpellWheel.p_Aiming))
                {
                    return;
                }
                if(m_playerEntity.EquipedItem == PlayerEquipableItems.ERASER && !m_aimedAt)
                {
                    return;
                }

                m_locked = true;
                //TEMP (kinda): Currently only fires if the equipped item is the quill, in future it should fire regardless but shoot a different projectile based on the equiped item passed to the projectile handler when
                //the equipped item is changed on the player 
                m_playerEntity.Animator.SetProperty(PlayerAnimationProperties.CASTING);

                if (m_playerEntity.EquipedItem == PlayerEquipableItems.SPELL_QUILL)
                {
                    Vector3 direction = Vector3.zero;

                    if (m_rayHitInfo.point != Vector3.zero)
                    {
                        direction = m_rayHitInfo.point - m_playerEntity.transform.position;
                    }
                    else
                    {
                        direction = (Camera.main.transform.position + (Camera.main.transform.forward * m_playerEntity.Projectile.Range)) - m_playerEntity.transform.position;
                    }

                    m_vfxHandler.CastSpellEffect(m_rayHitInfo.transform.position);

                   // m_vfxHandler.CastSpellEffect();
                }
                else if (m_aimedAt)
                {
                    m_vfxHandler.CastEraserEffect(m_rayHitInfo.transform.position);
                    //make sure that there is an enchantable script attached before casting a new spell
                    GameCore.Spells.Enchantable enchantable;
                    if (m_aimedAt.TryGetComponent<GameCore.Spells.Enchantable>(out enchantable))
                    {
                        enchantable.CastSpell(new GameCore.Spells.Spell(GameCore.Spells.SpellType.TRANSFORM_RESET));
                    }
                }
            }

            else if (!((Input.GetButtonDown("Fire") || Input.GetAxisRaw("Fire") != 0)))
            {
                m_locked = false;
            }
            //rotate the player to face the direction they are aiming in
            m_playerEntity.Rotation = Quaternion.LookRotation(new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z));

            //Slope detection 
            //if the slope is climable, modify the direction the player is traveling in 
            float slopeAngle = Vector3.Angle(m_playerEntity.GroundHitInfo.normal, Vector3.up);
            if (slopeAngle < m_playerEntity.MaxClimableAngle)
            {
                Vector3 slopeDirection = Vector3.zero;
                if (m_playerEntity.HasProperty(PlayerEntityProperties.SLIDING) && slopeAngle > 1)
                {
                    //get the players right based on direction of movement, then use it to calculate the new direction of travel
                    Vector3 playerRight = Vector3.Cross(m_playerEntity.transform.forward, -m_playerEntity.transform.up);
                    //getting the slope angle for the ground the player is walking on
                    slopeDirection = Vector3.Cross(playerRight, m_playerEntity.GroundHitInfo.normal);
                    if (slopeDirection.y > 0)
                    {
                        slopeDirection *= -1;
                    }
                }
                else
                {
                    //get the players right based on direction of movement, then use it to calculate the new direction of travel
                    Vector3 playerRight = Vector3.Cross(m_playerEntity.Direction, -m_playerEntity.transform.up);
                    //getting the slope angle for the ground the player is walking on
                    slopeDirection = Vector3.Cross(playerRight, m_playerEntity.GroundHitInfo.normal);
                }

                m_playerEntity.Direction = slopeDirection;
            }

            //If there is input, add velocity in that direction, but clamp the movement speed, Y velocity should always equal zero in this state, so no need to preserve down / up velocity
            if (m_playerEntity.Direction != Vector3.zero)
            {
                if (m_playerEntity.HasProperty(PlayerEntityProperties.SLIDING))
                {
                    m_velocity += (m_playerEntity.Direction * m_playerEntity.IceAcceleration) * Time.deltaTime;
                    m_velocity = Vector3.ClampMagnitude(m_velocity, m_playerEntity.IceMaxSpeed);
                }
                else
                {
                    m_velocity = m_velocity.magnitude * m_playerEntity.Direction;
                    m_velocity += (m_playerEntity.Direction * m_playerEntity.AimingAcceleration) * Time.deltaTime;
                    m_velocity = Vector3.ClampMagnitude(m_velocity, m_playerEntity.AimingMaxSpeed);
                }

                //ANIMATIONS
                //Based on movementInput, play animation, this is a monstrosity... can't use a switch statement here ;w;
                //could work out the angle from forward but that might actually be worse performance wise
                //forward
                if (m_playerEntity.MovementInput.x == 0 && m_playerEntity.MovementInput.y > 0)
                {
                    m_playerEntity.Animator.SetProperty(PlayerAnimationProperties.AIM_RUN_FORWARD);
                }
                //back
                else if (m_playerEntity.MovementInput.x == 0 && m_playerEntity.MovementInput.y < 0)
                {
                    m_playerEntity.Animator.SetProperty(PlayerAnimationProperties.AIM_RUN_BACK);
                }
                //left
                else if (m_playerEntity.MovementInput.x < 0 && m_playerEntity.MovementInput.y == 0)
                {
                    m_playerEntity.Animator.SetProperty(PlayerAnimationProperties.AIM_RUN_LEFT);
                }
                //right
                else if (m_playerEntity.MovementInput.x > 0 && m_playerEntity.MovementInput.y == 0)
                {
                    m_playerEntity.Animator.SetProperty(PlayerAnimationProperties.AIM_RUN_RIGHT);
                }
                //Top diagonals
                else if (m_playerEntity.MovementInput.x != 0 && m_playerEntity.MovementInput.y > 0)
                {
                    //diagonals face the way they are moving
                    //m_playerEntity.transform.rotation = Quaternion.LookRotation(new Vector3(m_playerEntity.Direction.x, 0, m_playerEntity.Direction.z));
                    m_playerEntity.Animator.SetProperty(PlayerAnimationProperties.AIM_RUN_FORWARD);
                }
                //bottom diagonals
                else if (m_playerEntity.MovementInput.x != 0 && m_playerEntity.MovementInput.y < 0)
                {
                    //back diagonals face the oposite way of movement
                    //m_playerEntity.transform.rotation = Quaternion.LookRotation(new Vector3(-m_playerEntity.Direction.x, 0, -m_playerEntity.Direction.z));
                    m_playerEntity.Animator.SetProperty(PlayerAnimationProperties.AIM_RUN_BACK);
                }
            }
            else if (Mathf.Abs(m_velocity.x - m_playerEntity.GroundAddedVelocity.x) > 0.1 || Mathf.Abs(m_velocity.z - m_playerEntity.GroundAddedVelocity.z) > 0.1)
            {
                if (m_playerEntity.HasProperty(PlayerEntityProperties.SLIDING))
                {
                    m_velocity += (m_velocity * -1) * m_playerEntity.IceDeceleration * Time.fixedDeltaTime;
                }
                else
                {
                    m_velocity += (m_velocity * -1) * m_playerEntity.WalkingDeceleration * Time.fixedDeltaTime;
                }
            }
            else
            {
                m_playerEntity.Animator.SetProperty(PlayerAnimationProperties.AIMING);
                m_velocity.x = 0.0f;
                m_velocity.z = 0.0f;
            }

            m_playerEntity.Velocity = m_velocity;
        }

        public void ResetAimedAt()
        {
            //only change if the shader hasn't already changed
            if(m_aimedAtRenderer != null && m_aimedAtRenderer.material.shader == m_playerEntity.HighlightShader)
            {
                m_aimedAtRenderer.material.shader = m_highlightedOldShader;
            }

            m_highlightedOldShader = null;
            m_aimedAtRenderer = null;
            m_aimedAt = null;
            SpellWheel.SetTargetEnchantable(null);
        }
    }
}
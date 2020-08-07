﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Player
{
    //You might be wondering why this class even exists when the Animation state machine already exists in unity
    //Well something about searching for parameters by string rubbed me the wrong way, so now this exists :)

    public enum PlayerAnimationProperties
    {
        IDLE                = 0,   
        WALKING,
        RUNNING,     
        JUMPING,
        FALLING,
        AIMING,
        AIM_RUN_FORWARD,
        AIM_RUN_BACK,
        AIM_RUN_RIGHT,
        AIM_RUN_LEFT,
        CASTING,
        PUSHING,
        DYING,
        FREE_FALLING,
        RECOVERING,
        SLIDING,
        LEFT_TURN,
        RIGHT_TURN
    }

    public enum PlayerFacialExpression
    {
        NATURAL                 = 0,
        SCARED,
        TALKING
    }

    public class PlayerAnimator : GameCore.System.Automaton
    {
        PlayerEntity m_playerEntity;
        PlayerEquipableItems m_previousEquipped;

        Renderer m_playerRenderer;
        PlayerFacialExpression m_playerFacialExpression;

        [Header("Expression")]
        [SerializeField] Texture m_naturalExpression;
        [SerializeField] Texture m_scaredExpression;
        [SerializeField] Texture m_talkingExpression;

        Animator m_playerAnimator;
        PlayerAnimationProperties m_playerAnimProperties;

        [Header("Animation")]
        [SerializeField] AnimationClip m_idleAnim;
        [SerializeField] AnimationClip m_walkingAnim;
        [SerializeField] AnimationClip m_runnningAnim;
        [SerializeField] AnimationClip m_jumpStartAnim;
        [SerializeField] AnimationClip m_jumpMidAnim;
        [SerializeField] AnimationClip m_jumpLandAnim;
        [SerializeField] AnimationClip m_fallingAnim;
        [SerializeField] AnimationClip m_aimingAnim;
        [SerializeField] AnimationClip m_aimingEraserAnim;
        [SerializeField] AnimationClip m_aimRunForwardAnim;
        [SerializeField] AnimationClip m_aimRunBackAnim;
        [SerializeField] AnimationClip m_aimRunLeftAnim;
        [SerializeField] AnimationClip m_aimRunRightAnim;
        [SerializeField] AnimationClip m_aimEraserRunForwardAnim;
        [SerializeField] AnimationClip m_aimEraserRunBackAnim;
        [SerializeField] AnimationClip m_aimEraserRunLeftAnim;
        [SerializeField] AnimationClip m_aimEraserRunRightAnim;
        [SerializeField] AnimationClip m_castingAnim;
        [SerializeField] AnimationClip m_freeFallingAnim;
        [SerializeField] AnimationClip m_recoveringAmim;
        [SerializeField] AnimationClip m_slidingStartAnim;
        [SerializeField] AnimationClip m_slidingMidAnim;
        [SerializeField] AnimationClip m_slidingEndAnim;
        [SerializeField] AnimationClip m_turnLeftAnim;
        [SerializeField] AnimationClip m_turnRightAnim;
        [SerializeField] Animation m_animation;
        [Header("Playback Speeds")]
        [SerializeField] float m_idleAnimSpeed = 2;
        [SerializeField] float m_walkingAnimSpeed = 2;
        [SerializeField] float m_runningAnimSpeed = 2;
        [SerializeField] float m_jumpStartAnimSpeed = 2;
        [SerializeField] float m_jumpMidAnimSpeed = 2;
        [SerializeField] float m_jumpLandAnimSpeed = 2;
        [SerializeField] float m_fallingAnimSpeed = 2;
        [SerializeField] float m_aimingAnimSpeed = 2;
        [SerializeField] float m_aimingEraserAnimSpeed = 1;
        [SerializeField] float m_aimRunAnimSpeed = 2;
        [SerializeField] float m_aimRunEraserAnimSpeed = 1;
        [SerializeField] float m_aimStrafeAnimSpeed = 2;
        [SerializeField] float m_castingAnimSpeed = 2;
        [SerializeField] float m_freeFallingAnimSpeed = 2;
        [SerializeField] float m_recoveringAnimSpeed = 2;
        [SerializeField] float m_slidingStartAnimSpeed = 2;
        [SerializeField] float m_slidingMidAnimSpeed = 2;
        [SerializeField] float m_slidingEndAnimSpeed = 2;
        [SerializeField] float m_turnAnimSpeed = 2;
        [Header("Weapon Equip")]
        [SerializeField] Transform m_quill;
        [SerializeField] Transform m_eraser;
        [Header("Properties")]
        [SerializeField] float m_timeOnGroundBeforeRecovering = 1;

        //in hindsight we don't actually need all of these things, however doesn't hurt to have them if we need them
        //non serialized fields
        AnimationState m_idleState;
        AnimationState m_walkingState;
        AnimationState m_runningState;
        AnimationState m_jumpStartState;
        AnimationState m_jumpMidState;
        AnimationState m_jumpEndState;
        AnimationState m_fallingState;
        AnimationState m_aimingState;
        AnimationState m_aimRunForwardState;
        AnimationState m_aimRunBackState;
        AnimationState m_aimRunLeftState;
        AnimationState m_aimRunRightState;
        AnimationState m_castingState;
        AnimationState m_freeFallingState;
        AnimationState m_recoveringState;
        AnimationState m_slidingStartState;
        AnimationState m_slidingMidState;
        AnimationState m_slidingEndState;
        AnimationState m_turnLeftState;
        AnimationState m_turnRightState;



        #region PUBLIC ACCESSORS
        public AnimationClip Idle { get => m_idleAnim; }
        public AnimationClip Walking { get => m_walkingAnim; }
        public AnimationClip Running { get => m_runnningAnim; }
        public AnimationClip JumpingStart { get => m_jumpStartAnim; }
        public AnimationClip JumpingMid { get => m_jumpMidAnim; }
        public AnimationClip JumpingLand { get => m_jumpLandAnim; }
        public AnimationClip Falling { get => m_fallingAnim; }
        public AnimationClip Aiming { get => m_aimingAnim; }
        public AnimationClip AimRunForward { get => m_aimRunForwardAnim; }
        public AnimationClip AimRunBack { get => m_aimRunBackAnim; }
        public AnimationClip AimRunLeft { get => m_aimRunLeftAnim; }
        public AnimationClip AimRunRight { get => m_aimRunRightAnim; }
        public AnimationClip Casting { get => m_castingAnim; }
        public AnimationClip FreeFalling { get => m_freeFallingAnim; }
        public AnimationClip Recovering { get => m_recoveringAmim; }
        public AnimationClip SlidingStart { get => m_slidingStartAnim; }
        public AnimationClip SlidingMid { get => m_slidingMidAnim; }
        public AnimationClip SlidingEnd { get => m_slidingEndAnim; }
        public AnimationClip TurnLeft { get => m_turnLeftAnim; }
        public AnimationClip TurnRight { get => m_turnRightAnim; }

        public AnimationState IdleState { get => m_idleState; }
        public AnimationState WalkingState { get => m_walkingState; }
        public AnimationState RunningState { get => m_runningState; }
        public AnimationState JumpingStartState { get => m_jumpStartState; }
        public AnimationState JumpingMidState { get => m_jumpMidState; }
        public AnimationState JumpingEndState { get => m_jumpEndState; }
        public AnimationState FallingState { get => m_fallingState; }
        public AnimationState AimingState { get => m_aimingState; }
        public AnimationState AimRunForwardState { get => m_aimRunForwardState; }
        public AnimationState AimRunBackState { get => m_aimRunBackState; }
        public AnimationState AimRunLeftState { get => m_aimRunLeftState; }
        public AnimationState AimRunRightState { get => m_aimRunRightState; }
        public AnimationState CastingState { get => m_castingState; }
        public AnimationState FreeFallingState { get => m_freeFallingState; }
        public AnimationState RecoveringState { get => m_recoveringState; }
        public AnimationState SlidingStartState { get => m_slidingStartState; }
        public AnimationState SlidingMidState { get => m_slidingMidState; }
        public AnimationState SlidingEndState { get => m_slidingEndState; }
        public AnimationState TurnLeftState { get => m_turnLeftState; }
        public AnimationState TurnRightState { get => m_turnRightState; }

        public float RunningAnimSpeed { get => m_runningAnimSpeed; }
        public float TurningAnimSpeed { get => m_turnAnimSpeed; }
        public float RecoverAnimSpeed { get => m_recoveringAnimSpeed; }
        public float TimeOnGroundBeforeRecover { get => m_timeOnGroundBeforeRecovering; }

        public Animation Animation { get => m_animation; }
        public PlayerAnimationProperties PlayerAnimProperties { get => m_playerAnimProperties; }
        public PlayerEntity Player { get => m_playerEntity; }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            if (!TryGetComponent(out m_playerEntity))
            {
                Debug.LogError($"PlayerEntity.cs not found on {transform.name} object with PlayerAnimator script attached");
            }
            m_playerRenderer = transform.GetChild(0).Find("Main_Character").GetComponent<Renderer>();
            try
            {
                m_animation.AddClip(m_idleAnim, "idle");
                m_animation.AddClip(m_walkingAnim, "walking");
                m_animation.AddClip(m_runnningAnim, "running");
                m_animation.AddClip(m_jumpStartAnim, "jumpStart");
                m_animation.AddClip(m_jumpMidAnim, "jumpMid");
                m_animation.AddClip(m_jumpLandAnim, "jumpLand");
                m_animation.AddClip(m_fallingAnim, "falling");
                m_animation.AddClip(m_aimingAnim, "aiming");
                m_animation.AddClip(m_aimingEraserAnim, "aimingEraser");
                m_animation.AddClip(m_aimRunForwardAnim, "aimRunForward");
                m_animation.AddClip(m_aimRunBackAnim, "aimRunBack");
                m_animation.AddClip(m_aimRunLeftAnim, "aimRunLeft");
                m_animation.AddClip(m_aimRunRightAnim, "aimRunRight");
                m_animation.AddClip(m_aimEraserRunForwardAnim, "aimEraserRunForward");
                m_animation.AddClip(m_aimEraserRunBackAnim, "aimEraserRunBack");
                m_animation.AddClip(m_aimEraserRunLeftAnim, "aimEraserRunLeft");
                m_animation.AddClip(m_aimEraserRunRightAnim, "aimEraserRunRight");
                m_animation.AddClip(m_castingAnim, "casting");
                m_animation.AddClip(m_freeFallingAnim, "freeFalling");
                m_animation.AddClip(m_recoveringAmim, "recovering");
                m_animation.AddClip(m_slidingStartAnim, "slidingStart");
                m_animation.AddClip(m_slidingMidAnim, "slidingMid");
                m_animation.AddClip(m_slidingEndAnim, "slidingEnd");
                m_animation.AddClip(m_turnLeftAnim, "turnLeft");
                m_animation.AddClip(m_turnRightAnim, "turnRight");
            }
            catch
            {
                Debug.LogWarning("One or more animations for the player are not set in editor");
            }
            foreach (AnimationState state in m_animation)
            {
                switch (state.name)
                {
                    case "idle":
                        state.speed = m_idleAnimSpeed;
                        m_idleState = state;
                        break;
                    case "walking":
                        state.speed = m_walkingAnimSpeed;
                        m_walkingState = state;
                        break;
                    case "running":
                        state.speed = m_runningAnimSpeed;
                        m_runningState = state;
                        break;
                    case "jumpStart":
                        state.speed = m_jumpStartAnimSpeed;
                        m_jumpStartState = state;
                        break;
                    case "jumpMid":
                        state.speed = m_jumpMidAnimSpeed;
                        m_jumpMidState = state;
                        break;
                    case "jumpLand":
                        state.speed = m_jumpLandAnimSpeed;
                        m_jumpEndState = state;
                        break;
                    case "falling":
                        state.speed = m_fallingAnimSpeed;
                        m_fallingState = state;
                        break;
                    case "aiming":
                        state.speed = m_aimingAnimSpeed;
                        m_aimingState = state;
                        break;
                    case "aimingEraser":
                        state.speed = m_aimingEraserAnimSpeed;
                        break;
                    case "aimRunForward":
                        state.speed = m_aimRunAnimSpeed;
                        m_aimRunForwardState = state;
                        break;
                    case "aimRunBack":
                        state.speed = m_aimRunAnimSpeed;
                        m_aimRunBackState = state;
                        break;
                    case "aimRunLeft":
                        state.speed = m_aimStrafeAnimSpeed;
                        m_aimRunLeftState = state;
                        break;
                    case "aimRunRight":
                        state.speed = m_aimStrafeAnimSpeed;
                        m_aimRunRightState = state;
                        break;
                    case "aimEraserRunForward":
                        state.speed = m_aimRunEraserAnimSpeed;
                        break;
                    case "aimEraserRunBack":
                        state.speed = m_aimRunEraserAnimSpeed;
                        break;
                    case "aimEraserRunLeft":
                        state.speed = m_aimStrafeAnimSpeed;
                        break;
                    case "aimEraserRunRight":
                        state.speed = m_aimStrafeAnimSpeed;
                        break;
                    case "casting":
                        state.speed = m_castingAnimSpeed;
                        m_castingState = state;
                        break;
                    case "freeFalling":
                        state.speed = m_freeFallingAnimSpeed;
                        m_freeFallingState = state;
                        break;
                    case "recovering":
                        state.speed = m_recoveringAnimSpeed;
                        m_recoveringState = state;
                        break;
                    case "slidingStart":
                        state.speed = m_slidingStartAnimSpeed;
                        m_slidingStartState = state;
                        break;
                    case "slidingMid":
                        state.speed = m_slidingMidAnimSpeed;
                        m_slidingMidState = state;
                        break;
                    case "slidingEnd":
                        state.speed = m_slidingEndAnimSpeed;
                        m_slidingEndState = state;
                        break;
                    case "turnLeft":
                        state.speed = m_turnAnimSpeed;
                        m_turnLeftState = state;
                        break;
                    case "turnRight":
                        state.speed = m_turnAnimSpeed;
                        m_turnRightState = state;
                        break;
                }
            }

            SetExpression(PlayerFacialExpression.NATURAL);
            m_playerAnimProperties = PlayerAnimationProperties.IDLE;
            SetState(new Idle_AnimationState(this));
        }

        // Update is called once per frame
        protected override void Update()
        {
            


            if ((m_playerAnimProperties == PlayerAnimationProperties.AIMING 
                || m_playerAnimProperties == PlayerAnimationProperties.AIM_RUN_FORWARD || m_playerAnimProperties == PlayerAnimationProperties.AIM_RUN_BACK) 
                || m_playerAnimProperties == PlayerAnimationProperties.AIM_RUN_LEFT || m_playerAnimProperties == PlayerAnimationProperties.AIM_RUN_RIGHT)
            {
                //if the weapon changes in idle mode, set the property to aiming again to change the anim
                if (m_playerAnimProperties == PlayerAnimationProperties.AIMING && m_playerEntity.EquipedItem != m_previousEquipped)
                {
                    SetProperty(PlayerAnimationProperties.AIMING);
                }
                //activate the equipped item
                if (m_playerEntity.EquipedItem == PlayerEquipableItems.SPELL_QUILL)
                {
                    m_quill.gameObject.SetActive(true);
                    m_eraser.gameObject.SetActive(false);
                }
                else
                {
                    m_quill.gameObject.SetActive(false);
                    m_eraser.gameObject.SetActive(true);
                }

                m_previousEquipped = m_playerEntity.EquipedItem;
            }
            else
            {
                m_quill.gameObject.SetActive(false);
                m_eraser.gameObject.SetActive(false);
            }
            //base.Update(); //Does not call manage on update as we only want to manage when a state changes
        }

        public void SetExpression(PlayerFacialExpression expression)
        {
            m_playerFacialExpression = expression;
            switch (expression)
            {
                case PlayerFacialExpression.NATURAL:
                    m_playerRenderer.material.SetTexture("_BaseMap", m_naturalExpression);
                    break;
                case PlayerFacialExpression.SCARED:
                    m_playerRenderer.material.SetTexture("_BaseMap", m_scaredExpression);
                    break;
                case PlayerFacialExpression.TALKING:
                    m_playerRenderer.material.SetTexture("_BaseMap", m_talkingExpression);
                    break;
            }
        }

        public void SetProperty(PlayerAnimationProperties property)
        {
            m_playerAnimProperties = property;
            if (m_state != null)
            {
                m_state.Manage();
            }
        }
    }
}
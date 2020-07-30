﻿using System.Collections;
using System.Collections.Generic;
using GameUI.Dialogue;
using Player;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCSimple : MonoBehaviour
{
    #region
    [Header("NPC Behaviour")]
    [SerializeField]
    private bool m_isPlayerTalking = false;

    [SerializeField]
    private LetterBox m_letterBox;

    [SerializeField]
    private float m_viewRadius;

    [Header("Animations")]
    [SerializeField]
    private string m_defaultAnimation;

    [SerializeField]
    private string m_talkingAnimation;

    [Header("Player Reference")]
    [SerializeField]
    private GameObject m_player;

    [Header("Input for starting dialogue")]
    [SerializeField]
    private KeyCode m_dialogueInput;

    Dialogue m_dialogue;

    bool m_talking = false;

    private Animator m_animator;
    private NPCState m_state;

    #endregion

    void Start()
    {
        m_dialogue = GetComponent<Dialogue>();
        m_animator = GetComponent<Animator>();
        m_state = new NPCStateDefault(m_animator, m_defaultAnimation, m_letterBox);
    }

    void Update()
    {
        //Entering dialogue mode
        if (Vector3.Distance(transform.position, m_player.transform.position) < m_viewRadius)
        {
            if(Input.GetKeyDown(m_dialogueInput) && !m_talking)
            {
                m_state = new NPCStateTalking(m_animator, m_talkingAnimation, m_letterBox);
                m_talking = true;
                m_dialogue.m_lastLineReached = false;
                Debug.Log("I am talking");
            }
        }

        //handling exit of conversation
        if (m_dialogue.m_lastLineReached && m_talking)
        {
            m_state = new NPCStateDefault(m_animator, m_defaultAnimation, m_letterBox);
            Debug.Log("I have stopped talking");
            m_talking = false;
        }


    }


    /// <summary>
    /// Inverts a bool to handle player interaction
    /// </summary>
    public void PlayerInteracts()
    {
        m_isPlayerTalking = !m_isPlayerTalking;
    }
}

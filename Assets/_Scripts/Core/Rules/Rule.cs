﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCore.System;

namespace GameCore.Rules
{
    public sealed class Rule
    {
        public enum ApplicationMode
        {
            APPLY, UNDO
        }
        private static List<RuleChunk.ChunkType> s_basicRule = new List<RuleChunk.ChunkType>(3)
        {
            RuleChunk.ChunkType.SUBJECT,
            RuleChunk.ChunkType.VERB,
            RuleChunk.ChunkType.OBJECT,
        };

        // We won't need these anymore, as we're only going for basic rules of length 3
        /*
        private static List<RuleChunk.ChunkType> s_complexRuleA = new List<RuleChunk.ChunkType>(5)
        {
            RuleChunk.ChunkType.SUBJECT,
            RuleChunk.ChunkType.VERB,
            RuleChunk.ChunkType.OBJECT,
            RuleChunk.ChunkType.LOGICAL_OP,
            RuleChunk.ChunkType.OBJECT,

        };
        private static List<RuleChunk.ChunkType> s_complexRuleB = new List<RuleChunk.ChunkType>(5)
        {
            RuleChunk.ChunkType.SUBJECT,
            RuleChunk.ChunkType.LOGICAL_OP,
            RuleChunk.ChunkType.SUBJECT,
            RuleChunk.ChunkType.VERB,
            RuleChunk.ChunkType.OBJECT
        };*/

        // Keyword "params" allows the constructor to be called with a comma-separated list of (zero or more) RuleChunk instances
        public Rule(params RuleChunk[] ruleChunks)
        {
            m_ruleChunks = new List<RuleChunk>(ruleChunks);
        }

        public readonly List<RuleChunk> m_ruleChunks;        
        private RuleDelegate m_ptrToMutables;

        private List<RuleChunk> FilterValidChunks()
        {
            // Don't even bother to filter the rule when we don't have at least 3 chunks (needed for the basic rule)
            if(m_ruleChunks.Count < s_basicRule.Count)
            {
                return null;
            }
           
            
            // No more complex rules in the game
            /*
            // TODO: consider the case when more than 5 rule chunks compose the rule
            if(IsValidComplexRule())
            {
                return m_ruleChunks;
            }
            */

            List<RuleChunk> validChunks = new List<RuleChunk>();

            // Here we are checking for a valid PARTITION of the list of chunks...
            // This means the rule can only be valid for the following set of indices:
            // [0, 1, 2] [1, 2, 3] [2, 3, 4] ... [m_ruleChunks.Count - 3, m_ruleChunks.Count - 2, m_ruleChunks.Count - 1]

            // Outermost loop is for setting up the partition's starting index
            for (int i = 0; i <= m_ruleChunks.Count - s_basicRule.Count; ++i)
            {
                // Innermost loop is for both setting up the partition's ending index and validate it
                for(int j = i, k = 0; j < s_basicRule.Count; ++j, ++k)
                {
                    if(m_ruleChunks[j].m_chunkType == s_basicRule[k])
                    {
                        validChunks.Add(m_ruleChunks[j]);
                    }
                    else
                    {
                        validChunks.Clear();
                        break;
                    }
                }

                if(validChunks.Count > 0)
                {
                    return validChunks;
                }
            }

            return null;
        }


        // Complex rules do not exist anymore in the game
        /*
        private bool IsValidComplexRule()
        {
            // Need to keep track of the for guard outside of its scope, so I'm declaring it here C style
            int i = 0;

            // Although 5 is the length of both complex rules, this is not guaranteed to stay constant throughout development:
            // for this reason, I prefer going for the "uglier" but decoupled check of both complex rules separately
            if(m_ruleChunks.Count == s_complexRuleA.Count)
            {                
                for(i = 0; i < m_ruleChunks.Count; i++)
                {
                    // At the very first mismatch, stop checking because it's not a valid complex rule A
                    if(m_ruleChunks[i].m_chunkType != s_complexRuleA[i])
                    {
                        break;
                    }

                    // All the chunk types matched in this case, it's a valid complex rule A!
                    if(i == m_ruleChunks.Count)
                    {
                        return true;
                    }
                }
            }

            if (m_ruleChunks.Count == s_complexRuleB.Count)
            {                
                for (i = 0; i < m_ruleChunks.Count; i++)
                {
                    // At the very first mismatch, stop checking because it's not a valid complex rule B
                    if (m_ruleChunks[i].m_chunkType != s_complexRuleB[i])
                    {
                        break;
                    }                    
                }
            }

            return i == m_ruleChunks.Count;
        }*/


        public static bool IsValidRule(params RuleChunk[] ruleChunks)
        {
            // We have a much simpler approach now, so it is enough to only check rules of length 3...
            if(ruleChunks.Length == s_basicRule.Count)
            {
                // Let's check if the order of the rule boxes describes a valid rule...
                for (int i = 0; i < ruleChunks.Length; ++i)
                {
                    // If any mismatch occurs, rule is not valid.
                    if(ruleChunks[i].m_chunkType != s_basicRule[i])
                    {
                        return false;
                    }
                }

                // We made it till this point, the rule is valid.
                return true;
            }

            // Rule is not of length 3, thus invalid.
            return false;
        }

        public void Apply(ApplicationMode mode = ApplicationMode.APPLY)
        {
            if(!IsValidRule(m_ruleChunks[0], m_ruleChunks[1], m_ruleChunks[2]))
            {
                return;
            }

            var ruleSubject = m_ruleChunks[0];
            var ruleVerb = m_ruleChunks[1];
            var ruleObject = m_ruleChunks[2];

            /*
            var mutableEntities = LevelManager.GetMutablesFromSubject(ruleSubject.m_lexeme);

            if(mutableEntities != null)
            {
                foreach (var mutableEntity in mutableEntities)
                {
                    if (mode == ApplicationMode.APPLY)
                    {
                        switch (ruleVerb.m_lexeme.ToLower())
                        {
                            case "is":
                                m_ptrToMutables += mutableEntity.Is;
                                break;
                            case "has":
                                m_ptrToMutables += mutableEntity.Has;
                                break;
                            case "can":
                                m_ptrToMutables += mutableEntity.Can;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (ruleVerb.m_lexeme.ToLower())
                        {
                            case "is":
                                m_ptrToMutables += mutableEntity.UndoIs;
                                break;
                            case "has":
                                m_ptrToMutables += mutableEntity.UndoHas;
                                break;
                            case "can":
                                m_ptrToMutables += mutableEntity.UndoCan;
                                break;
                            default:
                                break;
                        }
                    }

                }
            }           

            if(m_ptrToMutables != null)
            {
                m_ptrToMutables(ruleObject.m_lexeme);
            }*/
        }
    }
}
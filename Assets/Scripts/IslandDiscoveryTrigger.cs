﻿////////////////////////////////////////////////////////////////////////////////////////
// Action At Sea - IslandDiscoveryTrigger.cs
////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IslandDiscoveryTrigger : MonoBehaviour
{
    public float onCaptureScore = 20.0f;
    public float scorePerTime = 1.0f;
    public float timePassedForScore = 2.0f;
    private float m_scoreToAdd = 0.0f;
    public UnityEngine.UI.Image tickImage = null;
    public UnityEngine.UI.Text ownerText = null;
    public UnityEngine.UI.Text scoreText = null;
    public float m_scoreSpeed = 1.0f;
    public float m_minScoreSize = 1.0f;
    public float m_maxScoreSize = 2.0f;

    private RectTransform m_scoreTransform = null;
    private Vector3 m_scoreScale;

    private float m_timePassed = 0.0f;
    private Canvas m_canvas = null;
    private IslandDiscoveryNode[] m_nodes;
    private GameObject m_owner = null;
    private List<SpriteRenderer> m_islands = new List<SpriteRenderer>();

    /// <summary>
    /// Initialises the script
    /// </summary>
    void Start()
    {
        m_scoreTransform = scoreText.GetComponent<RectTransform>();
        scoreText.gameObject.SetActive(false);

        m_nodes = transform.parent.GetComponentsInChildren<IslandDiscoveryNode>();
        foreach(var node in m_nodes)
        {
            node.SetTrigger(this);
        }

        var islands = transform.parent.GetComponentsInChildren<SpriteRenderer>();
        foreach(var island in islands)
        {
            if(island.CompareTag("Island"))
            {
                m_islands.Add(island);
            }
        }

        if(m_islands.Count == 0)
        {
            Debug.LogError("No associated island sprite");
        }
        if(m_nodes.Length == 0)
        {
            Debug.LogError("No associated nodes");
        }

        m_canvas = GetComponent<Canvas>();
        m_canvas.enabled = false;
    }

    /// <summary>
    /// Checks whether the island has been discovered
    /// </summary>
    void Update()
    {
        GameObject owner = m_nodes[0].Owner;
        for (int i = 1; i < m_nodes.Length; ++i)
        {
            if (owner == null ||
               m_nodes[i].Owner == null ||
               owner.name != m_nodes[i].Owner.name)
            {
                // Island was captured but is no longer
                owner = null;
                break;
            }
        }

        if (owner == null)
        {
            SetCaptured(null);
        }
        else if (m_owner == null || m_owner.name != owner.name)
        {
            Debug.Log("Setting new owner of island: " + owner.name);
            SetCaptured(owner);
        }

        if (m_owner != null && PlayerManager.IsControllablePlayer(m_owner))
        {
            m_timePassed += Time.deltaTime;
            if (m_timePassed >= timePassedForScore)
            {
                m_scoreToAdd += scorePerTime;
                m_timePassed = 0.0f;

                if ((int)m_scoreToAdd > 0)
                {
                    ShowScore(m_scoreToAdd);
                    m_owner.GetComponent<PlayerScore>().AddScore(m_scoreToAdd);
                    m_scoreToAdd = 0.0f;
                }
            }
        }

        if(scoreText.gameObject.activeSelf)
        {
            m_scoreScale.x += Time.deltaTime * m_scoreSpeed;
            m_scoreScale.x = Mathf.Min(Mathf.Max(0.0f, m_scoreScale.x), m_maxScoreSize);
            m_scoreScale.y = m_scoreScale.x;
            m_scoreScale.z = m_scoreScale.x;
            m_scoreTransform.localScale = m_scoreScale;

            if (m_scoreScale.x == m_maxScoreSize)
            {
                scoreText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Shows the score text score
    /// </summary>
    void ShowScore(float score)
    {
        scoreText.gameObject.SetActive(true);
        m_scoreScale.x = m_minScoreSize;
        m_scoreScale.y = m_minScoreSize;
        m_scoreScale.z = m_minScoreSize;
        m_scoreTransform.localScale = m_scoreScale;
        scoreText.text = "+" + ((int)score).ToString();
    }

    /// <summary>
    /// Sets whether the island is captured
    /// </summary>
    void SetCaptured(GameObject owner)
    {
        m_canvas.enabled = owner != null;

        if(owner != null)
        {
            tickImage.color = Utilities.GetPlayerColor(owner);
            ownerText.text = Utilities.GetPlayerName(owner);
            ShowScore(onCaptureScore);
            scoreText.color = tickImage.color;

            if (PlayerManager.IsCloseToPlayer(owner.transform.position, 30.0f))
            {
                SoundManager.Get().PlaySound(SoundManager.SoundID.ISLAND_FIND);
            }

            var player = PlayerManager.GetControllablePlayer();
            if(player != null && player.name == owner.name)
            {
                owner.GetComponent<PlayerScore>().AddScore(onCaptureScore);
            }
        }
        else
        {
            tickImage.color = new Color(1.0f, 1.0f, 1.0f);
            ownerText.text = "";
        }

        foreach(var island in m_islands)
        {
            island.color = tickImage.color;
        }

        m_scoreToAdd = 0.0f;
        m_timePassed = 0.0f;
        m_owner = owner;
    }

    /// <summary>
    /// Returns whether this island has been discovered
    /// </summary>
    public bool IsDiscovered()
    {
        return m_canvas.enabled;
    }

    /// <summary>
    /// Returns the owner of this island
    /// </summary>
    public GameObject GetOwner()
    {
        return m_owner;
    }
}
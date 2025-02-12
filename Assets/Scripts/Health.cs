﻿////////////////////////////////////////////////////////////////////////////////////////
// Action At Sea - Health.cs
////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

/// <summary>
/// NOTE: Instantiated by Photon Networking
/// Start() cannot include any code relying on the world/level as 
/// this object can be instantiated before the level is created
/// </summary>
public abstract class Health : MonoBehaviour
{
    protected GameObject m_healthBar = null;
    protected UnityEngine.UI.Text m_floatingHealthBarText = null;
    protected float m_minBarWidth = 0.0f;
    protected float m_maxBarWidth = 0.0f;
    protected float m_barHeight = 0.0f;
    protected float m_healthMax = 100.0f;
    protected float m_healthMin = 0.0f;
    protected float m_healthLevel = 100.0f;
    protected bool m_isAlive = true;
    protected bool m_initialised = false;

    /// <summary>
    /// Initialises the health
    /// Code not relying on the world goes here
    /// </summary>
    void Start()
    {
        m_healthLevel = m_healthMax;
    }

    /// <summary>
    /// Initialises the health
    /// Code relying on the world goes here
    /// </summary>
    protected virtual void Initialise()
    {
        if(m_healthBar == null)
        {
            var floatingHealthBar = transform.parent.transform.FindChild("FloatingHealthBar");
            var canvas = floatingHealthBar.FindChild("Canvas").transform;
            m_healthBar = canvas.FindChild("HealthBar").gameObject;
            var playerName = canvas.FindChild("PlayerName").gameObject;
            m_floatingHealthBarText = playerName.GetComponent<UnityEngine.UI.Text>();
        }

        if(m_healthBar == null)
        {
            Debug.LogError("Could not find health bar");
        }

        m_maxBarWidth = m_healthBar.GetComponent<RectTransform>().rect.width;
        m_barHeight = m_healthBar.GetComponent<RectTransform>().rect.height;
        m_initialised = true;
    }

    /// <summary>
    /// On Destroy set the GUI health back to normal
    /// </summary>
    void OnDestroy()
    {
        if(m_healthBar != null)
        {
            m_healthBar.GetComponent<RectTransform>().sizeDelta =
                new Vector2(m_maxBarWidth, m_barHeight);
        }
    }

    /// <summary>
    /// Updates the health
    /// </summary>
    void Update()
    {
        if(!Utilities.IsLevelLoaded())
        {
            return;
        }

        if(!m_initialised)
        {
            Initialise();
        }

        if(m_floatingHealthBarText != null)
        {
            m_floatingHealthBarText.text = 
                Utilities.GetPlayerName(gameObject);
        }

        if (m_healthLevel > m_healthMax)
        {
            m_healthLevel = m_healthMax;
        }

        if (m_healthLevel <= m_healthMin)
        {
            m_healthLevel = 0.0f;
            m_isAlive = false;
        }

        // If the object has a health bar scale it to show the health
        if(m_healthBar != null)
        {
            // Convert the value range from 0->100 to 0->maxBarScale
            float barWidth = ((m_healthLevel-m_healthMin)*((m_maxBarWidth-
                m_minBarWidth)/(m_healthMax-m_healthMin)))+m_minBarWidth;

            m_healthBar.GetComponent<UnityEngine.UI.Image>().enabled = barWidth > m_minBarWidth;
            m_healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(barWidth, m_barHeight);
        }

        OnUpdate();
    }

    /// <summary>
    /// Called on update
    /// </summary>
    protected abstract void OnUpdate();

    /// <summary>
    /// Inflicts damage to the health
    /// </summary>
    public void InflictDamage(float damage)
    {
        m_healthLevel -= damage;
    }

    /// <summary>
    /// Repairs damage to the health
    /// </summary>
    public void RepairDamage(float repairAmount)
    {
        m_healthLevel += repairAmount;
    }

    /// <summary>
    /// Sets the health level
    /// </summary>
    public void SetHealthLevel(float level)
    {
        m_healthLevel = level;
    }

    /// <summary>
    /// Gets the health level
    /// </summary>
    public float HealthLevel
    {
        get { return m_healthLevel; }
    }

    /// <summary>
    /// Gets the maximum health
    /// </summary>
    public float HealthMax
    {
        get { return m_healthMax; }
    }

    /// <summary>
    /// Gets the minimum health
    /// </summary>
    public float HealthMin
    {
        get { return m_healthMin; }
    }

    /// <summary>
    /// Gets whether the health is alive
    /// </summary>
    public void SetAlive(bool isAlive)
    {
        m_isAlive = isAlive; 
        SetHealthLevel(isAlive ? HealthMax : HealthMin);
    }

    /// <summary>
    /// Gets whether the health is alive
    /// </summary>
    public bool IsAlive
    {
        get 
        { 
            return m_isAlive; 
        }
    }
}

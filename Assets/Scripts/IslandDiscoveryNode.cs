﻿////////////////////////////////////////////////////////////////////////////////////////
// Action At Sea - IslandDiscoveryNode.cs
////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IslandDiscoveryNode : MonoBehaviour
{ 
    private int m_ownerID = 0;
    private GameObject m_owner = null;
    private double m_timestamp = 0.0;

    /// <summary>
    /// Initialises the node
    /// </summary>
    void Start()
    {
    }

    /// <summary>
    /// Checks whether the owner is still valid
    /// </summary>
    void Update()
    {
        if(m_owner == null && m_ownerID != -1)
        {
            GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
            m_ownerID = -1;
            m_timestamp = 0.0;
        }
    }

    /// <summary>
    /// On collision with a player
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        SetOwner(other.gameObject);
    }

    /// <summary>
    /// Sets the owner of this node only if the timestamp is less
    /// </summary>
    public void TrySetOwner(int ownerID, double timestamp)
    {
        if(ownerID != -1 && (m_owner == null || timestamp < m_timestamp))
        {
            GameObject player = PlayerManager.GetPlayerWithID(ownerID);
            if(player != null)
            {
                SetOwner(player);
                m_timestamp = timestamp;
            }
        }
    }

    /// <summary>
    /// Sets the owner of this node
    /// </summary>
    public void SetOwner(GameObject owner)
    {
        if(m_owner == null || m_owner.name != owner.name)
        {
            var networkedPlayer = owner.GetComponent<NetworkedPlayer>();

            GetComponent<SpriteRenderer>().color = networkedPlayer.PlayerColor;
            SoundManager.Get().PlaySound(SoundManager.SoundID.ISLAND_NODE);

            m_owner = owner;
            m_ownerID = networkedPlayer.PlayerID;
            m_timestamp = NetworkMatchmaker.Get().GetTime();
        }
    }

    /// <summary>
    /// Gets the owner of this node
    /// </summary>
    public GameObject Owner
    {
        get { return m_owner; }
    }

    /// <summary>
    /// Gets the timestep the node was owned at
    /// </summary>
    public double TimeStamp
    {
        get { return (double)m_timestamp; }
    }

    /// <summary>
    /// Gets the ID of the node
    /// </summary>
    public int OwnerID
    {
        get { return m_ownerID; }
    }
}
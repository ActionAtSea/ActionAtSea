﻿////////////////////////////////////////////////////////////////////////////////////////
// Action At Sea - NetworkedEntity.cs
////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class NetworkedEntity : MonoBehaviour
{
    public PhotonView photonView = null;

    /// <summary>
    /// Information required which is not networked
    /// </summary>
    #region infonotnetworked
    protected int m_spawnIndex = -1; // based on players in room. helps assign spawn spots. not networked.
    public Color m_colour = new Color(1.0f, 1.0f, 1.0f);
    protected bool m_initialised = false;
    protected bool m_visible = true;
    protected bool m_isAI = false;
    protected bool m_recievedValidData = false;
    private bool m_initialisedComponents = false;
    protected Health m_healthBar = null;
    protected GameObject m_floatingHealthBar = null;
    protected CannonController m_cannonController = null;
    protected Rigidbody m_rigidBody = null;
    protected CapsuleCollider m_collider = null;
    protected TrailRenderer m_trailRenderer = null;
    #endregion

    /// <summary>
    /// Information networked peer-to-peer
    /// </summary>
    #region infonetworkedp2p
    protected int m_ID = -1; // photon creates one to uniquely identify this. 
    protected string m_name = "";
    private int m_hue = -1;
    protected float m_health = -1.0f;
    protected float m_mouseCursorAngle = 0.0f;
    protected bool m_firedCannonsLeft = false;
    protected bool m_firedCannonsRight = false;
    protected Vector3 m_networkedPosition;
    protected Quaternion m_networkedRotation;
    #endregion

    /// <summary>
    /// Initilaises the networked entity
    /// Code not relying on the world goes here
    /// </summary>
    protected void InitialiseAtStart()
    {
        if(!m_initialisedComponents)
        {
            InitialiseEntityComponents();
        }

        // Photon Networking will destroy the object
        var parent = transform.parent;
        DontDestroyOnLoad(parent);
    }

    /// <summary>
    /// Initilaises the networked entity
    /// Code relying on the world goes here
    /// </summary>
    protected virtual void InitialiseAtWorld()
    {
        if(photonView.isMine)
        {
            //NOTE: I switched the ID to be using the photonView id instead of the actor ID
            //      as the view ID is unique to each object with a photonView. Whilst
            //      the actor ID is only unique to each user within the game. 
            m_ID = photonView.viewID;
            
            //OLD CODE: Used actorID to identify players. m_ID implementation not yet finalised. Don't remove.
            //m_ID = matchMaker.GetPlayerID();

            ResetPosition();
            NotifyPlayerCreation();
        }

        m_floatingHealthBar = transform.parent.FindChild("FloatingHealthBar").gameObject;
        if (m_floatingHealthBar == null)
        {
            Debug.LogError("Could not find floating health bar");
        }

        Debug.Log("Created " + gameObject.tag);
        m_initialised = true;
    }

    /// <summary>
    /// Adds the entity to the minimap and colours sections of the ship
    /// </summary>
    protected virtual void NotifyPlayerCreation()
    {
        var minimap = GameObject.FindObjectOfType<Minimap>();
        minimap.AddPlayer(gameObject, photonView.isMine, () => { return m_colour; });
    }

    /// <summary>
    /// Sets the colour of the ship from its hue
    /// </summary>
    protected void SetHue(int hue)
    {
        m_hue = hue;
        m_colour = Colour.HueToRGB(m_hue);

        SetShipColour[] colouredComponets = GetComponentsInChildren<SetShipColour>();
        if (colouredComponets != null)
        {
            foreach (SetShipColour component in colouredComponets)
            {
                component.SetColour(m_colour);
            }
        }
    }

    /// <summary>
    /// Positions the ship on a spawn
    /// </summary>
    private void ResetPosition()
    {
        var playerManager = PlayerManager.Get();
        var place = playerManager.GetNewPosition(m_spawnIndex, m_isAI);

        if(!m_isAI)
        {
            SetHue(place.hue);
        }

        m_rigidBody.velocity = Vector3.zero;
        gameObject.transform.position = place.position;
        gameObject.transform.localEulerAngles = place.rotation;
    }

    /// <summary>
    /// Hides/shows the ship. Keep the networked entity active 
    /// to allow connected entitys to still recieve information
    /// </summary>
    protected virtual void ShowShip(bool show)
    {
        m_visible = show;
        m_collider.enabled = show;
        m_trailRenderer.enabled = show;
        m_floatingHealthBar.SetActive(show);

        for(int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(show);
        }
    }

    /// <summary>
    /// Callback when game over is set
    /// </summary>
    public void SetVisible(bool isVisible, bool shouldExplode)
    {
        // Set visible can be called before initialisation is completed
        if (!m_initialisedComponents)
        {
            InitialiseEntityComponents();
        }

        if (isVisible)
        {
            ShowShip(true);
            m_healthBar.SetAlive(true);
        }
        else
        {
            if (shouldExplode)
            {
                m_healthBar.SetAlive(false);
                Explode();
            }

            ShowShip(false);

            if (photonView.isMine)
            {
                ResetPosition();
            }
        }
    }

    /// <summary>
    /// Explodes the ship
    /// </summary>
    protected void Explode()
    {
        // Will be null if leaving game
        var animationGenerator = FindObjectOfType<AnimationGenerator>();
        if (animationGenerator != null)
        {
            animationGenerator.PlayAnimation(
                transform.position, AnimationGenerator.ID.EXPLOSION);
        }
    }

    /// <summary>
    /// On destroy called for both client and non-client controlled
    /// </summary>
    protected void Destroy()
    {
        Debug.Log("Destroying ship: " + name);
        Explode();
    }

    /// <summary>
    /// Updates the entity from the networked data
    /// </summary>
    protected void OnUpdate()
    {
        if (!Utilities.IsLevelLoaded())
        {
            return;
        }

        if (!m_initialised)
        {
            InitialiseAtWorld();
        }

        if (photonView.isMine)
        {
            m_health = m_healthBar.HealthLevel;
            m_mouseCursorAngle = m_cannonController.MouseCursorAngle;

            if (m_cannonController.CannonsFiredRight)
            {
                m_firedCannonsRight = true;
            }
            if (m_cannonController.CannonsFiredLeft)
            {
                m_firedCannonsLeft = true;
            }
        }
        else if (m_recievedValidData)
        {
            PositionNonClientPlayer();
            m_cannonController.MouseCursorAngle = m_mouseCursorAngle;

            if (m_firedCannonsLeft)
            {
                m_cannonController.FireWeaponLeft();
                m_firedCannonsLeft = false;
            }

            if (m_firedCannonsRight)
            {
                m_cannonController.FireWeaponRight();
                m_firedCannonsRight = false;
            }

            if (m_health >= 0)
            {
                // Only update health if networked version is lower
                // This can mean however that networked version thinks its higher
                // and the entity can be running around seemingly empty
                // Because of this, initially set if lower but slowly increment if higher
                // This means if theres a difference it'll eventually correct itself 

                float health = m_healthBar.HealthLevel;
                if (m_health <= health)
                {
                    m_healthBar.SetHealthLevel(m_health);
                }
                else
                {
                    float difference = m_health - health;
                    float addSpeed = Time.deltaTime * 0.05f;
                    float incrementingHealth = health + (difference * addSpeed);
                    m_healthBar.SetHealthLevel(incrementingHealth);
                }
            }
        }
    }

    /// <summary>
    /// Serialises entity data to each entity
    /// Note not called if only entity in the room
    /// Note not called every tick or at regular intervals
    /// </summary>
    public void Serialize(PhotonStream stream)
    {
        if (stream.isWriting)
        {
            stream.SendNext(m_ID);
            stream.SendNext(m_hue);
            stream.SendNext(m_name);
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(m_health);
            stream.SendNext(m_mouseCursorAngle);
            stream.SendNext(m_firedCannonsLeft);
            stream.SendNext(m_firedCannonsRight);
            stream.SendNext(m_visible);

            m_firedCannonsRight = false;
            m_firedCannonsLeft = false;
        }
        else
        {
            m_ID = (int)stream.ReceiveNext();
            int hue = (int)stream.ReceiveNext();
            m_name = (string)stream.ReceiveNext();

            m_networkedPosition = (Vector3)stream.ReceiveNext();
            m_networkedRotation = (Quaternion)stream.ReceiveNext();

            m_health = (float)stream.ReceiveNext();
            m_mouseCursorAngle = (float)stream.ReceiveNext();

            bool firedCannonsLeft = (bool)stream.ReceiveNext();
            if (firedCannonsLeft)
            {
                m_firedCannonsLeft = true;
            }

            bool firedCannonsRight = (bool)stream.ReceiveNext();
            if (firedCannonsRight)
            {
                m_firedCannonsRight = true;
            }

            bool isVisible = (bool)stream.ReceiveNext();
            if (isVisible != m_visible)
            {
                SetVisible(isVisible, !isVisible);
            }

            if (m_initialised && hue >= 0)
            {
                // On first recieve valid data
                if (!m_recievedValidData)
                {
                    m_recievedValidData = true;
                    m_rigidBody.velocity = Vector3.zero;
                    transform.rotation = m_networkedRotation;
                    transform.position = m_networkedPosition;
                    SetHue(hue);
                    NotifyPlayerCreation();
                }

                // Only change the colour once fully initialised
                if (hue != m_hue)
                {
                    SetHue(hue);
                }
            }
        }
    }

    /// <summary>
    /// Attempts to predict where the non-client entity would be to reduce network latency
    /// </summary>
    protected virtual void PositionNonClientPlayer()
    {
        transform.position = Vector3.Lerp(
            transform.position, m_networkedPosition, Time.deltaTime * 5);

        transform.rotation = Quaternion.Lerp(
            transform.rotation, m_networkedRotation, Time.deltaTime * 5);
    }

    /// <summary>
    /// Gets the entity Color
    /// </summary>
    public Color PlayerColor
    {
        get { return m_colour; }
    }

    /// <summary>
    /// Gets the entity Hue
    /// </summary>
    public int PlayerHue
    {
        get { return m_hue; }
    }

    /// <summary>
    /// Returns whether the entity can control this
    /// </summary>
    public bool IsControllable()
    {
        return photonView.isMine;
    }

    /// <summary>
    /// Whether the entity is fully initialised
    /// </summary>
    public bool IsInitialised()
    {
        return IsControllable() ? m_initialised :
            m_initialised && m_recievedValidData;
    }

    /// <summary>
    /// Whether the entity is visible
    /// </summary>
    public bool IsVisible()
    {
        return m_visible;
    }

    /// <summary>
    /// Gets the player ID
    /// </summary>
    public int PlayerID
    {
        get { return m_ID; }
    }

    /// <summary>
    /// Gets the player name
    /// </summary>
    public string PlayerName
    {
        get { return m_name; }
    }

    /// <summary>
    /// Gets the entity components
    /// </summary>
    protected virtual void InitialiseEntityComponents()
    {
        m_collider = GetComponent<CapsuleCollider>();
        if (m_collider == null)
        {
            Debug.LogError("Could not find capsule collider");
        }

        m_rigidBody = GetComponent<Rigidbody>();
        if (m_rigidBody == null)
        {
            Debug.LogError("Could not find rigid body");
        }

        m_trailRenderer = GetComponentInChildren<TrailRenderer>();
        if (m_trailRenderer == null)
        {
            Debug.LogError("Could not find trail renderer");
        }

        m_initialisedComponents = true;
    }

}

﻿////////////////////////////////////////////////////////////////////////////////////////
// Action At Sea - PlayerPlacer.cs
////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public class PlayerPlacer : MonoBehaviour 
{    
    private GameObject m_gameboard = null;
    private float m_gameboardOffset = 20.0f;
    private float m_playerRadious = 5.0f;
    private GameObject[] m_spawns = null;

    /// <summary> 
    /// Position/rotation information
    /// </summary>
    public class Placement
    {
        public Vector3 position = new Vector3();
        public Vector3 rotation = new Vector3();
    }

    /// <summary>
    /// Initialises the script
    /// </summary>
    void Start()
    {
        m_gameboard = GameObject.FindGameObjectWithTag("GameBoard");
        if(m_gameboard == null)
        {
            Debug.LogError("Could not find game board");
        }

        m_spawns = GameObject.FindGameObjectsWithTag("Spawn");
    }

    /// <summary>
    /// Utility function to determine if the given position is roughly visible to the player
    /// </summary>
    static public bool IsCloseToPlayer(Vector3 position)
    {
        var player = PlayerManager.GetControllablePlayer();
        if(player != null)
        {
            const float maxDistance = 30.0f;
            return (player.transform.position - position).magnitude <= maxDistance;
        }
        return false;
    }

    /// <summary>
    /// Retrieves a new position on the map
    /// </summary>
    public Placement GetNewPosition(GameObject player)
    {
        if(m_spawns != null)
        {
            PlayerSpawn chosenSpawn = null;

            foreach (GameObject obj in m_spawns)
            {
                var spawn = obj.GetComponent<PlayerSpawn>();
                if(!spawn.Owner)
                {
                    chosenSpawn = spawn;
                }
                else if(spawn.Owner == player)
                {
                    chosenSpawn = spawn;
                    break;
                }
            }

            if(chosenSpawn != null)
            {
                chosenSpawn.Owner = player;
                Placement place = new Placement();
                place.position.x = chosenSpawn.transform.position.x;
                place.position.y = chosenSpawn.transform.position.y;
                place.rotation.x = chosenSpawn.transform.localEulerAngles.x;
                place.rotation.y = chosenSpawn.transform.localEulerAngles.y;
                place.rotation.z = chosenSpawn.transform.localEulerAngles.z;
                return place;
            }
        }

        return GetRandomPosition();
    }

    /// <summary>
    /// Retrieves a new position on the map that doesn't collide
    /// </summary>
    public Placement GetRandomPosition()
    {
        GameObject[] players = PlayerManager.GetEnemies();

        Vector2 position = new Vector2();
        bool foundPosition = false;
        
        var boardBounds = m_gameboard.GetComponent<SpriteRenderer>().bounds;
        var halfBoardWidth = Mathf.Abs(boardBounds.max.x - boardBounds.min.x) / 2.0f;
        var halfBoardLength = Mathf.Abs(boardBounds.max.y - boardBounds.min.y) / 2.0f;

        while (!foundPosition) 
        {
            foundPosition = true;
            position.x = Random.Range(-halfBoardWidth + m_gameboardOffset, 
                                      halfBoardWidth - m_gameboardOffset);

            position.y = Random.Range(-halfBoardLength + m_gameboardOffset, 
                                      halfBoardLength - m_gameboardOffset);


            GameObject[] terrain = GameObject.FindGameObjectsWithTag("Island");
            if(terrain == null)
            {
                Debug.LogError("Could not find any terrain");
            }

            for(int i = 0; i < terrain.Length; ++i)
            {
                var islandBounds = terrain[i].GetComponent<SpriteRenderer>().bounds;
                if(position.x > islandBounds.center.x - islandBounds.extents.x &&
                   position.x < islandBounds.center.x + islandBounds.extents.x &&
                   position.y > islandBounds.center.y - islandBounds.extents.y &&
                   position.y < islandBounds.center.y + islandBounds.extents.y)
                {
                    foundPosition = false;
                    break;
                }
            }

            if(foundPosition && players != null)
            {
                foreach(GameObject player in players)
                {
                    if(player != null)
                    {
                        Vector2 playerPosition = new Vector2(player.transform.position.x, player.transform.position.y);
                        Vector2 difference = position - playerPosition;
                        if(difference.magnitude <= m_playerRadious)
                        {
                            foundPosition = false;
                            break;
                        }
                    }
                }
            }
        }

        Placement place = new Placement();
        place.position.x = position.x;
        place.position.y = position.y;
        return place;
    }
}

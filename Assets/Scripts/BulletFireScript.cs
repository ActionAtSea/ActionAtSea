﻿////////////////////////////////////////////////////////////////////////////////////////
// Action At Sea - BulletFireScript.cs
////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// NOTE: Instantiated by Photon Networking
/// Start() cannot include any code relying on the world/level as 
/// this object can be instantiated before the level is created
/// </summary>
public class BulletFireScript : MonoBehaviour
{
    public Vector3 SpawnOffset = new Vector3(0.0f, 0.0f, 0.0f);
    private float m_bulletSpeedUp = 50.0f;
    private float m_bulletSpeedForward = 500.0f;
    private Vector2 m_firingDirection;

    /// <summary>
    /// Fires a bullet
    /// </summary>
    public void Fire(int owner, Vector3 firePosition, Quaternion fireRotation)
    {
        GameObject obj = NewObjectPooler.Get().GetPooledObject();

        if (obj == null)
        {
            Debug.LogError("Could not generate bullet from pooler");
            return;
        }

        obj.transform.position = firePosition;
        obj.transform.rotation = fireRotation;
        obj.transform.Translate(SpawnOffset);
        obj.SetActive(true);
        obj.GetComponent<Bullet>().Owner = owner;

        Vector3 bulletVelocity = 
            transform.right * m_bulletSpeedForward + 
            Vector3.up * m_bulletSpeedUp;

        obj.GetComponent<Rigidbody>().AddForce(bulletVelocity);

        if(PlayerManager.IsCloseToPlayer(obj.transform.position, 30.0f))
        {
            SoundManager.Get().PlaySound(SoundManager.SoundID.FIRE);
        }
    }
}

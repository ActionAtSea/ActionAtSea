﻿////////////////////////////////////////////////////////////////////////////////////////
// Action At Sea - FillScoreBoard.cs
////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FillScoreBoard : MonoBehaviour 
{
    public GameObject scoreText;
    private List<UnityEngine.UI.Text> m_scoreFront = new List<UnityEngine.UI.Text>();
    private List<UnityEngine.UI.Text> m_scoreBack = new List<UnityEngine.UI.Text>();

    /// <summary>
    /// Instantiates player text
    /// </summary>
    void Start()
    {
        AddText(scoreText);
        scoreText.SetActive(false);
        float xPosition = scoreText.GetComponent<RectTransform>().localPosition.x;
        float yPosition = scoreText.GetComponent<RectTransform>().localPosition.y;

        var level = Utilities.GetLoadedLevel();
        int amount = Utilities.GetMaxPlayersForLevel(level);
        float spacingOffset = 20.0f;

        for(int i = 1; i < amount; ++i)
        {
            var obj = Instantiate(scoreText);
            obj.SetActive(false);
            obj.transform.SetParent(transform);

            obj.GetComponent<RectTransform>().localRotation = Quaternion.identity;

            obj.GetComponent<RectTransform>().localScale =
                scoreText.GetComponent<RectTransform>().localScale;

            obj.GetComponent<RectTransform>().localPosition = new Vector3(
                xPosition, 
                yPosition - (i * spacingOffset),
                0.0f);

            AddText(obj);
        }
    }

    /// <summary
    /// Instantiates player text
    /// </summary>
    void AddText(GameObject obj)
    {
        var frontText = obj.transform.GetChild(0);
        m_scoreBack.Add(obj.GetComponent<UnityEngine.UI.Text>());
        m_scoreFront.Add(frontText.GetComponent<UnityEngine.UI.Text>());
    }

    /// <summary>
    /// Compiles a score board from all game players
    /// </summary>
    void Update () 
    {
        Dictionary<int, int> islandsOwned = IslandDiscoveryTrigger.GetIslandsOwned();
        List<GameObject> players = PlayerManager.GetAllPlayersByScore();
        int maxShown = Mathf.Min(players.Count, m_scoreBack.Count);
        int i = 0;

        for(; i < maxShown; ++i)
        {
            var obj = players[i];
            if(obj != null)
            {
                m_scoreBack[i].gameObject.SetActive(true);
                m_scoreFront[i].color = Utilities.GetPlayerColor(obj);

                int ID = Utilities.GetPlayerID(obj);
                string text = Utilities.GetPlayerScore(obj).ToString() +
                    ": " + Utilities.GetPlayerName(obj) + " [";

                if(islandsOwned.ContainsKey(ID))
                {
                    text += islandsOwned[ID].ToString();
                }
                else
                {
                    text += "0";
                }

                text += "]";

                m_scoreBack[i].text = text;
                m_scoreFront[i].text = text;
            }

        }

        for (; i < m_scoreBack.Count; ++i)
        {
            m_scoreBack[i].gameObject.SetActive(false);
        }
    }
}

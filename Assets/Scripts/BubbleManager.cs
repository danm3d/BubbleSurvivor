﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BubbleManager : MonoBehaviour
{
	private enum BubblesMode
	{
		Arena = 0,
		Maze = 1
	}
    public GameObject bubblePrefab;
    private float playWidth, playHeight;
    public GameObject player;//reference to the player
    public bool hcMode = false;

    // Use this for initialization
    void Start()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
        playWidth = pos.x;
        playHeight = pos.y;
        SpawnBubbles();
    }
	
    private void SpawnBubbles()
    {
        Vector3 spawnPos = Vector3.zero;
        GameObject bubble;
        for (int i = 0; i < 10; i++)
        {
            spawnPos = new Vector3(Random.Range(-playWidth, playWidth), Random.Range(-playHeight, playHeight), 0f);
            bubble = Instantiate(bubblePrefab, spawnPos, Quaternion.identity) as GameObject;
            bubble.GetComponent<BubbleBehaviour>().bubbleManager = this;
            bubble.transform.SetParent(transform);
            bubble.GetComponent<BubbleBehaviour>().Spawn();
            if (hcMode)
            {
                bubble.GetComponent<BubbleBehaviour>().player = player;
                bubble.GetComponent<BubbleBehaviour>().hcMode = hcMode;
        }
    }
    }

    public void SaveBubble(GameObject bubble)
    {
        bubble.SetActive(false);
        bubble.transform.position = new Vector3(Random.Range(-playWidth, playWidth), Random.Range(-playHeight, playHeight), 0f);
        bubble.SetActive(true);
        bubble.GetComponent<BubbleBehaviour>().Respawn();
        if (hcMode)
        {
            bubble.GetComponent<BubbleBehaviour>().player = player;
            bubble.GetComponent<BubbleBehaviour>().hcMode = hcMode;
        }
    }
	
}

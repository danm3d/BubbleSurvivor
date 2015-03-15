using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BubbleManager : MonoBehaviour
{

    public GameObject bubblePrefab;
    private float playWidth, playHeight;

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
        }
    }

    public void SaveBubble(GameObject bubble)
    {
        bubble.SetActive(false);
        bubble.transform.position = new Vector3(Random.Range(-playWidth, playWidth), Random.Range(-playHeight, playHeight), 0f);
        bubble.SetActive(true);
        bubble.GetComponent<BubbleBehaviour>().Respawn();
    }
	
}

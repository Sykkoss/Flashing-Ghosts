﻿using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Tooltip("GameObject to spawn")]
    public GameObject toSpawn;

    [Tooltip("Initial time before first spawn (in s)")]
    public float spawnWait;

    [Tooltip("Time between each spawn (in s)")]
    public float spawnInterval;

    public Node node;

    private const float maxDistDetect = 1f;
    private const float speed = 0.05f;

    private float timer;

    private void Start()
    {
        timer = spawnWait;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            Instantiate(toSpawn, transform.position, Quaternion.identity);
            timer = spawnInterval;
        }
    }

    private void FixedUpdate()
    {
        int x = 0, y = 0;

        if (transform.position.x + maxDistDetect < node.transform.position.x)
            x = 1;
        else if (transform.position.x - maxDistDetect > node.transform.position.x)
            x = -1;

        if (transform.position.y + maxDistDetect < node.transform.position.y)
            y = 1;
        else if (transform.position.y - maxDistDetect > node.transform.position.y)
            y = -1;

        if (x == 0 && y == 0)
            node = node.GetNextNode();
        else
            transform.Translate(new Vector2(x, y) * speed);
    }
}

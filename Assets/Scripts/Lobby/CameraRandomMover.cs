using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraRandomMover : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Vector2 horizontal;
    [SerializeField] private Vector2 vertical;
    private Vector3 _destination;

    void Start()
    {
        StartCoroutine(RandomMove());
    }

    IEnumerator RandomMove()
    {
        while (true)
        {
            _destination = randomPointInArea();
            while (transform.position != _destination)
            {
                transform.position = Vector3.MoveTowards(transform.position, _destination, speed * Time.deltaTime);
                yield return null;
            }
        }
    }

    Vector3 randomPointInArea()
    {
        float randomX = Random.Range(vertical.x, vertical.y);
        float randomZ = Random.Range(horizontal.x, horizontal.y);
        return new Vector3(randomX, transform.position.y, randomZ);
    }
}

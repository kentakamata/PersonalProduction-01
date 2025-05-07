using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class P: MonoBehaviour
{
    public GameObject player;
    Vector3 touchWorldPosition;
    public int Speed = 5;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 touchScreenPosition = Input.mousePosition;
            touchScreenPosition.z = 5.0f;
            Camera camera = Camera.main;
            touchWorldPosition = camera.ScreenToWorldPoint(touchScreenPosition);
        }
        player.transform.position = Vector3.MoveTowards(player.transform.position,
            touchWorldPosition, Speed * Time.deltaTime);
    }
}

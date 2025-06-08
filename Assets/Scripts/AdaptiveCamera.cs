using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveCamera : MonoBehaviour
{
    public string[] playerNames;  // Noms dels jugadors
    public Vector3 offset;        // Desplaçament camera
    public float smoothSpeed = 0.125f; // Velocitat de suavitzat
    public float minZoom = 40f;   // Zoom mínim
    public float maxZoom = 10f;   // Zoom màxim
    public float zoomLimiter = 50f; // Factor de límit per adjustar zoom
    private Transform[] players;  // Referencies als jugadors
    private Camera cam;

    void Start()
    {

    }
    
    void LateUpdate()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        FindPlayersByName();
        if (players == null || players.Length == 0) return;

        Move();
        Zoom();
    }


    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 desiredPosition = centerPoint + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }

    void Zoom()
    {
        float greatestDistance = GetGreatestDistance();
        float desiredSize = Mathf.Lerp(maxZoom, minZoom, greatestDistance / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredSize, smoothSpeed);
    }

    Vector3 GetCenterPoint()
    {
        if (players.Length == 0) return Vector3.zero;

        Bounds bounds = new Bounds(players[0].position, Vector3.zero);
        foreach (Transform player in players)
        {
            if (player != null)
            {
                bounds.Encapsulate(player.position);
            }
        }
        return bounds.center;
    }

    float GetGreatestDistance()
    {
        if (players.Length == 0) return 0;

        Bounds bounds = new Bounds(players[0].position, Vector3.zero);
        foreach (Transform player in players)
        {
            if (player != null)
            {
                bounds.Encapsulate(player.position);
            }
        }
        return bounds.size.x > bounds.size.y ? bounds.size.x : bounds.size.y;
    }

    void FindPlayersByName()
    {
        players = new Transform[playerNames.Length];

        for (int i = 0; i < playerNames.Length; i++)
        {
            GameObject player = GameObject.Find(playerNames[i]);
            if (player != null)
            {
                players[i] = player.transform;
            }
            else
            {
                Debug.LogWarning($"No s'ha trobat el jugador amb el nom '{playerNames[i]}'.");
            }
        }
    }
}


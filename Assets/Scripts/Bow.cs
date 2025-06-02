using System;
using UnityEngine;

public class Bow : MonoBehaviour
{
    public Vector2 direction;

    public Transform arrowSpawnPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 MousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 bowPos = transform.position;
        direction = MousePos - bowPos; // Calcula la posició
        FaceMouse();

    }
    void FaceMouse()
    {
        transform.right = direction;
    }
}

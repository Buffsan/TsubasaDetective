using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMap_Controller : MonoBehaviour
{
    PlayerController controller => PlayerController.Instance;

    public Vector2 MoveCamera = new Vector2(8,6);

    public Tilemap FloorMap;

    public GameObject StartPoint;
    public GameObject Goal;

    public enum Stage
    {

        Ferust,
        Diana,
        Garewo,
        Kardia,
        Pastal,
        Teruseto,
        Shuela

    }
    public Stage stage = Stage.Ferust;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

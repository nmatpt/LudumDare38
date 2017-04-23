using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapManager : MonoBehaviour {

    public Camera gameCamera;

    public int mapRadius;

    public GameObject hexTemplate;
    public float hexRadius;

    public GameObject personTemplate;
    public int numberOfPeople;

	public GameObject rocketTemplate;
	private Vector3 rocketCoordinates;
	private int nrPersonsIn = 0;

    private GameObjectCubeMatrix tileMatrix;
    private GameObjectCubeMatrix peopleMatrix;
    private GameObject selectedTile;
    private GameObject selectedPerson;
    private Vector3 selectedTileCoordinates;
    private HashSet<GameObject> walkableTiles;
    private HashSet<MovingPersonData> movingPeople;

	public UnityEngine.UI.Text winText;

	// Use this for initialization
	void Start () {

        tileMatrix = GenerateMap();
        peopleMatrix = PopulateMap();
		rocketCoordinates = PlaceRocket ();

        selectedTile = null;
        selectedPerson = null;

        walkableTiles = new HashSet<GameObject>();
        movingPeople = new HashSet<MovingPersonData>();
    }
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 clickedTileCoordinates = GridUtils.PointyTopSceneToCubeCoordinates(mousePosition, hexRadius);

            GameObject tile = tileMatrix.GetValue( (int)clickedTileCoordinates.x, (int)clickedTileCoordinates.y, (int)clickedTileCoordinates.z);
            GameObject person = peopleMatrix.GetValue((int)clickedTileCoordinates.x, (int)clickedTileCoordinates.y, (int)clickedTileCoordinates.z);

            if (selectedTile != null && selectedPerson != null && GridUtils.CubeCoordinatesDistance(selectedTileCoordinates, clickedTileCoordinates) == 1)
            {
                /*
                // It's a move action
                selectedPerson.transform.position = GridUtils.CubeToPointyTopSceneCoordinates(clickedTileCoordinates, hexRadius);
                peopleMatrix.RemoveValue(selectedTileCoordinates);

                // It's moving inside the rocket
				if (rocketCoordinates == clickedTileCoordinates) {					
					selectedPerson.SetActive (false);
					nrPersonsIn += 1;
					if (nrPersonsIn >= numberOfPeople) {
						winText.text ="YOU WIN!!";
					}
				}
                else
                {
    				peopleMatrix.AddValue (clickedTileCoordinates, selectedPerson);
                }
                */

                selectedPerson.GetComponent<PersonAnimator>().StartMoving();
                movingPeople.Add(new MovingPersonData(selectedPerson, selectedTileCoordinates, clickedTileCoordinates));

                selectedTile.GetComponent<TileState>().Unselect();
                selectedTile = null;
                ResetWalkableTiles();
                selectedPerson.GetComponent<PersonAnimator>().Unselect();
                selectedPerson = null;
            }
            else
            {
                // Its a random click

                // handle tiles
				if (tile != null && tile != selectedTile && rocketCoordinates != clickedTileCoordinates)
                {
                    if (selectedTile != null)
                    {
                        selectedTile.GetComponent<TileState>().Unselect();
                        ResetWalkableTiles();
                    }
                    tile.GetComponent<TileState>().Select();
                    selectedTile = tile;
                    selectedTileCoordinates = clickedTileCoordinates;
                }

                // handle people
                if (person != null && person != selectedPerson && person.GetComponent<PersonAnimator>().CanMove())
                {
                    if (selectedPerson != null)
                    {
                        selectedPerson.GetComponent<PersonAnimator>().Unselect();
                    }
                    person.GetComponent<PersonAnimator>().Select();
                    selectedPerson = person;
                    ActivateWalkableTiles(clickedTileCoordinates);
                }
                else if (tile != null && selectedPerson != null && selectedPerson != person) // clicked a tile without people
                {
                    selectedPerson.GetComponent<PersonAnimator>().Unselect();
                    selectedPerson = null;
                }
            }
        }

        List<MovingPersonData> finishedMoving = movingPeople.Where(x => x.Person.GetComponent<PersonAnimator>().HasStoppedMoving()).ToList();

        foreach (MovingPersonData data in finishedMoving)
        {
            if (data.Person.GetComponent<PersonAnimator>().HasStoppedMoving())
            {
                data.Person.transform.position = GridUtils.CubeToPointyTopSceneCoordinates(data.Destination, hexRadius);
                peopleMatrix.RemoveValue(data.Origin);

                // It's moving inside the rocket
                if (rocketCoordinates == data.Destination)
                {
                    data.Person.SetActive(false);
                    nrPersonsIn += 1;
                    if (nrPersonsIn >= numberOfPeople)
                    {
                        winText.text = "YOU WIN!!";
                    }
                }
                else
                {
                    peopleMatrix.AddValue(data.Destination, data.Person);
                }
                data.Person.GetComponent<PersonAnimator>().ReadyToMove();
                movingPeople.Remove(data);
            }
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            selectedTile = null;
            foreach (GameObject go in tileMatrix)
            {
                Destroy(go);
            }
            tileMatrix = GenerateMap();

            selectedPerson = null;
            foreach (GameObject go in peopleMatrix)
            {
                Destroy(go);
            }
            peopleMatrix = PopulateMap();
        }
	}

    private GameObjectCubeMatrix GenerateMap()
    {
        GameObjectCubeMatrix tileMatrix = new GameObjectCubeMatrix();

        foreach (Vector3 cubeVec in GetAllCubeCoordinates())
        {
            Vector2 screenVec = GridUtils.CubeToPointyTopSceneCoordinates(cubeVec, hexRadius);
            GameObject hex = Instantiate(hexTemplate, new Vector3(screenVec.x, screenVec.y, 0), Quaternion.identity);
            tileMatrix.AddValue((int)cubeVec.x, (int)cubeVec.y, (int)cubeVec.z, hex);
        }

        return tileMatrix;
    }

	private Vector3 PlaceRocket()
	{
        Vector3 rocketScenePosition = new Vector3(0, 0, -1);
		GameObject rocket = Instantiate(rocketTemplate, rocketScenePosition, Quaternion.identity);

        return GridUtils.PointyTopSceneToCubeCoordinates(rocketScenePosition, hexRadius);
	}

    private GameObjectCubeMatrix PopulateMap()
    {
        GameObjectCubeMatrix peopleMatrix = new GameObjectCubeMatrix();

        List<Vector3> coordinates = GetAllCubeCoordinates();
        int i = 0;
        for (; i < coordinates.Count; i++)
        {
            if (coordinates[i] == Vector3.zero)
            {
                break;
            }
        }
        coordinates.RemoveAt(i);

        if (numberOfPeople > coordinates.Count)
        {
            throw new Exception("Number Of people must be less or equal to the number of tiles");
        }

        for (int j=0; j < numberOfPeople; j++)
        {
            int index = UnityEngine.Random.Range(0, coordinates.Count);
            Vector3 cube = coordinates[index];
            coordinates.RemoveAt(index);
            Vector2 screen = GridUtils.CubeToPointyTopSceneCoordinates(cube, hexRadius);
            GameObject person = Instantiate(personTemplate, new Vector3(screen.x, screen.y, 0), Quaternion.identity);
            person.GetComponent<PersonAnimator>().SetQuantity(1000);
            peopleMatrix.AddValue((int)cube.x, (int)cube.y, (int)cube.z, person);
        }

		return peopleMatrix;
    }

    private void ActivateWalkableTiles(Vector3 centerTileCubeCoordinates)
    {
        foreach (Vector3 vec in GridUtils.CubeCoordinatesNeightbours(centerTileCubeCoordinates))
        {
            GameObject go = tileMatrix.GetValue(vec);
            if (go != null)
            {
                walkableTiles.Add(go);
                go.GetComponent<TileState>().SetWalkable();
            }
        }
    }

    private void ResetWalkableTiles()
    {
        foreach (GameObject go in walkableTiles)
        {
            go.GetComponent<TileState>().Unselect();
        }
        walkableTiles.Clear();
    }

    private List<Vector3> GetAllCubeCoordinates()
    {
        List<Vector3> coordinates = new List<Vector3>();
        for (int i = -mapRadius; i <= mapRadius; i++)
        {
            for (int j = -mapRadius; j <= mapRadius; j++)
            {
                for (int k = -mapRadius; k <= mapRadius; k++)
                {
                    if (i + j + k != 0)
                    {
                        continue;
                    }
                    coordinates.Add(new Vector3(i, j, k));
                }
            }
        }
        return coordinates;
    }

    private class MovingPersonData
    {
        public GameObject Person { get; set; }
        public Vector3 Origin { get; set; }
        public Vector3 Destination { get; set; }

        public MovingPersonData(GameObject person, Vector3 origin, Vector3 destination)
        {
            Person = person;
            Origin = origin;
            Destination = destination;
        }
    }
}

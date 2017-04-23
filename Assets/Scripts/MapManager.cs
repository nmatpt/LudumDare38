using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Linq;

public class MapManager : MonoBehaviour {

    public Camera gameCamera;

    public int mapRadius;

    public GameObject hexTemplate;
    public float hexRadius;

    public GameObject personTemplate;
    public int numberOfPeople;
	public int numberOfObstacles;

	public GameObject rocketTemplate;
	private GameObject rocket;
	private Vector3 rocketCoordinates;
	private int nrPersonsIn = 0;

    private GameObjectCubeMatrix tileMatrix;
    private GameObjectCubeMatrix peopleMatrix;
    private GameObject selectedTile;
    private GameObject selectedPerson;
    private Vector3 selectedTileCoordinates;
    private HashSet<GameObject> walkableTiles;
    private HashSet<MovingPersonData> movingPeople;
	private List<Vector3> destroyedTiles;
	private List<Vector3> obstacles;

	public UnityEngine.UI.Text winText;

	public float destroyStartDelay = 1.0f;
	public float destroyDelay = 1.0f;

	// Use this for initialization
	void Start () {
		walkableTiles = new HashSet<GameObject>();
		movingPeople = new HashSet<MovingPersonData>();
		destroyedTiles = new List<Vector3> ();
		obstacles = new List<Vector3> ();

		rocketCoordinates = PlaceRocket ();
		tileMatrix = GenerateMap();
        peopleMatrix = PopulateMap();


        selectedTile = null;
        selectedPerson = null;

		DestructionDaemon ();
    }
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 clickedTileCoordinates = GridUtils.PointyTopSceneToCubeCoordinates(mousePosition, hexRadius);

            GameObject tile = tileMatrix.GetValue( (int)clickedTileCoordinates.x, (int)clickedTileCoordinates.y, (int)clickedTileCoordinates.z);
            GameObject person = peopleMatrix.GetValue((int)clickedTileCoordinates.x, (int)clickedTileCoordinates.y, (int)clickedTileCoordinates.z);

			if (selectedTile != null && selectedPerson != null && ValidNeighbourTiles(selectedTileCoordinates).Contains(clickedTileCoordinates))
            {
				Vector2 movingDirection = GridUtils.CubeToPointyTopSceneCoordinates(clickedTileCoordinates, hexRadius) - GridUtils.CubeToPointyTopSceneCoordinates(selectedTileCoordinates, hexRadius);
                print(GridUtils.CubeToPointyTopSceneCoordinates(clickedTileCoordinates, hexRadius) + " " + GridUtils.CubeToPointyTopSceneCoordinates(selectedTileCoordinates, hexRadius));

                selectedPerson.GetComponent<PersonAnimator>().StartMoving(movingDirection);
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
						rocket.GetComponent<RocketManager>().Launch();
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
		List<Vector3> cubeCoordinates = GetAllCubeCoordinates ();
		foreach (Vector3 cubeVec in cubeCoordinates)
        {
            Vector2 screenVec = GridUtils.CubeToPointyTopSceneCoordinates(cubeVec, hexRadius);
            GameObject hex = Instantiate(hexTemplate, new Vector3(screenVec.x, screenVec.y, 0), Quaternion.identity);
            tileMatrix.AddValue((int)cubeVec.x, (int)cubeVec.y, (int)cubeVec.z, hex);
        }

		if (numberOfObstacles > cubeCoordinates.Count - 1 - numberOfPeople)
		{
			throw new Exception("Too many obstacles");
		}

		for (int i = 0; i < numberOfObstacles; i++) {
			List<Vector3> nonObstacleTiles = cubeCoordinates.Except (obstacles).Except(new List<Vector3>{rocketCoordinates}).ToList();
			int index = UnityEngine.Random.Range (0, nonObstacleTiles.Count);
			Vector3 obstacleCoordinates = nonObstacleTiles[index];
			tileMatrix.GetValue (obstacleCoordinates).GetComponent<TileState> ().SetObstacle();
			obstacles.Add (obstacleCoordinates);
		}

        return tileMatrix;
    }

	private Vector3 PlaceRocket()
	{
        Vector3 rocketScenePosition = new Vector3(0, 0, -1);
		rocket = Instantiate(rocketTemplate, rocketScenePosition, Quaternion.identity);

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
		foreach (Vector3 vec in ValidNeighbourTiles(centerTileCubeCoordinates))
        {
            GameObject go = tileMatrix.GetValue(vec);
            if (go != null)
            {
                walkableTiles.Add(go);
                go.GetComponent<TileState>().SetWalkable();
            }
        }
    }

	private List<Vector3> ValidNeighbourTiles(Vector3 coordinates)
	{
		return GridUtils.CubeCoordinatesNeightbours (coordinates)
			.Except (destroyedTiles)
			.Except (obstacles)
			.Where(c => tileMatrix.GetValue(c) != null)
			.ToList();
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

	private void DestructionDaemon() {
		InvokeRepeating	("DestroyTile", destroyStartDelay, destroyDelay);
	}

	private void DestroyTile(){
		List<Vector3> coordinates = GetAllCubeCoordinates ()
			.Except(destroyedTiles).ToList()
			.ToList();
		
		if (coordinates.Count <= 1) {
			print ("YOU LOST!!!!!!!");
		}else{
			int maxX = coordinates.Max (c =>  (int) Math.Abs(c.x));
			int maxY = coordinates.Max (c => (int) Math.Abs(c.y));
			int maxZ = coordinates.Max (c => (int) Math.Abs(c.z));

			//Look for border tiles and tiles that have neighbours destroyed
			List<Vector3> borderTiles = coordinates.FindAll (c => Math.Abs(c.x) == maxX || Math.Abs(c.y) == maxY || Math.Abs(c.z) == maxZ);
			List<Vector3> destroyedNeighbours = new List<Vector3> ();
			destroyedTiles.ForEach (n => destroyedNeighbours.AddRange (ValidNeighbourTiles (n)));
			destroyedNeighbours = destroyedNeighbours.ToList();

			//Bias towards border tiles and many destroyed neighbours
			List <Vector3> selectableTiles = borderTiles
				.Concat(borderTiles)
				.Concat(borderTiles)
				.Concat (destroyedNeighbours)
				.Except(new List<Vector3>{rocketCoordinates}).ToList();
			
			int index = UnityEngine.Random.Range(0, selectableTiles.Count);
			Vector3 coordinatesToDestroy = selectableTiles [index];

			GameObject tile = tileMatrix.GetValue (coordinatesToDestroy);
			GameObject person = peopleMatrix.GetValue (coordinatesToDestroy);

			if (peopleMatrix.GetValue (coordinatesToDestroy) != null) {
				person.SetActive(false);
				peopleMatrix.RemoveValue (coordinatesToDestroy);
				print ("DEAD PERSON YOU LOST!!!!!!!");
			}

			tile.GetComponent<TileState>().Destroy();
			destroyedTiles.Add (coordinatesToDestroy);
		
		}
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapManager : MonoBehaviour {

    public Camera gameCamera;

    public int mapRadius;
    public int mapHeightThreshold;

    public GameObject hexTemplate;
    public float hexRadius;

    public GameObject personTemplate;
    public int numberOfPeople;
	public int numberOfObstacles;

    public int minimumPeopleQuantity;
    public int maximumPeopleQuantity;

	public GameObject rocketTemplate;
	public GameObject meteorTemplate;
	private GameObject rocket;
	private Vector3 rocketCoordinates;

    private GameObjectCubeMatrix tileMatrix;
    private GameObjectCubeMatrix peopleMatrix;
    private GameObject selectedTile;
    private GameObject selectedPerson;
    private Vector3 selectedTileCoordinates;
    private HashSet<GameObject> walkableTiles;
    private HashSet<MovingPersonData> movingPeople;
	private List<MeteorData> fallingMeteors;
	private List<Vector3> destroyedTiles;
	private List<Vector3> obstacles;

	//public UnityEngine.UI.Text winText;

	public float destroyStartDelay = 1.0f;
	public float destroyDelay = 1.0f;

	// Use this for initialization
	void Start () {
		walkableTiles = new HashSet<GameObject>();
		movingPeople = new HashSet<MovingPersonData>();
		destroyedTiles = new List<Vector3> ();
		obstacles = new List<Vector3> ();
		fallingMeteors = new List<MeteorData> ();

		rocketCoordinates = PlaceRocket ();
		tileMatrix = GenerateMap();
        peopleMatrix = PopulateMap();


        selectedTile = null;
        selectedPerson = null;

		DestructionDaemon ();
    }

	// Update is called once per frame
	void Update () {
		
        // FOR DEBUG ONLY
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 clickedTileCoordinates = GridUtils.PointyTopSceneToCubeCoordinates(mousePosition, hexRadius);

            DestroyTileAt(clickedTileCoordinates);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 clickedTileCoordinates = GridUtils.PointyTopSceneToCubeCoordinates(mousePosition, hexRadius);

            GameObject tile = tileMatrix.GetValue( (int)clickedTileCoordinates.x, (int)clickedTileCoordinates.y, (int)clickedTileCoordinates.z);
            GameObject person = peopleMatrix.GetValue((int)clickedTileCoordinates.x, (int)clickedTileCoordinates.y, (int)clickedTileCoordinates.z);

			if (selectedTile != null && selectedPerson != null && ValidNeighbourTiles(selectedTileCoordinates).Contains(clickedTileCoordinates))
            {
                // move person
				Vector2 movingDirection = GridUtils.CubeToPointyTopSceneCoordinates(clickedTileCoordinates, hexRadius) - GridUtils.CubeToPointyTopSceneCoordinates(selectedTileCoordinates, hexRadius);

                GameObject destinationPerson = peopleMatrix.GetValue(clickedTileCoordinates);
                if (destinationPerson == null)
                {
                    if (clickedTileCoordinates == rocketCoordinates)
                    {
                        destinationPerson = rocket;
                    }
                    else
                    {
                        destinationPerson = Instantiate(personTemplate, GridUtils.CubeToPointyTopSceneCoordinates(clickedTileCoordinates, hexRadius), Quaternion.identity);
                        destinationPerson.GetComponent<PersonAnimator>().SetQuantity(0);
                        destinationPerson.GetComponent<PersonAnimator>().SetReceivingPeople();
                        peopleMatrix.AddValue(clickedTileCoordinates, destinationPerson);
                    }
                }
                
                selectedPerson.GetComponent<PersonAnimator>().StartMoving(movingDirection, destinationPerson);
                movingPeople.Add(new MovingPersonData(selectedPerson, destinationPerson, selectedTileCoordinates, clickedTileCoordinates));

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

        List<MovingPersonData> finishedMoving = movingPeople.Where(x => x.OriginPerson.GetComponent<PersonAnimator>().HasStoppedMoving()).ToList();

        foreach (MovingPersonData data in finishedMoving)
        {
            if (data.OriginPerson.GetComponent<PersonAnimator>().HasStoppedMoving())
            {
                movingPeople.Remove(data);
                if (data.OriginPerson.GetComponent<PersonAnimator>().GetQuantity() != 0)
                {
                    // something happened to destination, and movement was interrupted
                    data.OriginPerson.GetComponent<PersonAnimator>().StopMoving();
                    data.OriginPerson.GetComponent<PersonAnimator>().ReadyToMove();
                    continue;
                }

                //data.OriginPerson.transform.position = GridUtils.CubeToPointyTopSceneCoordinates(data.Destination, hexRadius);
                peopleMatrix.RemoveValue(data.Origin);

                if (data.DestinationPerson.tag != "Rocket")
                {
                    data.DestinationPerson.GetComponent<PersonAnimator>().ReadyToMove();
                }
                data.OriginPerson.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
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


	void FixedUpdate () {
		List<MeteorData> toDestroy = new List<MeteorData> ();

		foreach(var item in fallingMeteors)
		{
			var meteor = item.Meteor;
			var tileHexCoordinates = item.TileCoordinate;
				
			Vector2 meteorPosition = gameCamera.WorldToScreenPoint (meteor.GetComponent < MetorMovement> ().transform.position);
			Vector2 tilePosition = gameCamera.WorldToScreenPoint(GridUtils.CubeToPointyTopSceneCoordinates(tileHexCoordinates, hexRadius));
			float distance = Vector2.Distance (meteorPosition, tilePosition);

			if (distance > item.Distance) {
				toDestroy.Add (item);
			}
			item.Distance = distance;
		}

		foreach (var item in toDestroy) {
			var tileHexCoordinates = item.TileCoordinate;
			print("Destroying tile " + tileHexCoordinates);
			item.Meteor.SetActive (false);
			DestroyTileAt(tileHexCoordinates);		
			fallingMeteors.Remove (item);
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
            person.GetComponent<PersonAnimator>().SetQuantity(UnityEngine.Random.Range(minimumPeopleQuantity, maximumPeopleQuantity));
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
        int maxHeight = Mathf.Min(mapRadius, mapHeightThreshold);
        for (int i = -mapRadius; i <= mapRadius; i++)
        {
            for (int j = -mapRadius; j <= mapRadius; j++)
            {
                for (int k = -maxHeight; k <= maxHeight; k++)
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
		InvokeRepeating	("RandomlySelectTile", destroyStartDelay, destroyDelay);
	}

	private void LaunchMeteor(Vector3 tileCoordinate){
		Vector2 screenBounds = gameCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
		Vector3 meteorStartPosition =  new Vector3(0,screenBounds.y + 1, -5);

		Vector2 meteorStartScreenCoordinate =  new Vector2(meteorStartPosition.x, meteorStartPosition.y);
		Vector2 tileScreenCoordinate = GridUtils.CubeToPointyTopSceneCoordinates (tileCoordinate, hexRadius);

		Vector2 direction = (tileScreenCoordinate - meteorStartScreenCoordinate);

		GameObject meteor = Instantiate(meteorTemplate, meteorStartPosition, Quaternion.identity);
		meteor.GetComponent < MetorMovement> ().MoveTo (direction);
		fallingMeteors.Add (new MeteorData(meteor, tileCoordinate, tileScreenCoordinate, float.MaxValue));
	}

	private void RandomlySelectTile(){
		var fallingMeteorTiles = fallingMeteors.Select (x => x.TileCoordinate);
		List<Vector3> coordinates = GetAllCubeCoordinates ()
			.Except(destroyedTiles).ToList()
			.Except(fallingMeteorTiles).ToList()
			.ToList();
		
		if (coordinates.Count <= 0) {
			print ("YOU LOST!!!!!!!");
		}else{
			int maxX = coordinates.Max (c =>  (int) Math.Abs(c.x));
			int maxY = coordinates.Max (c => (int) Math.Abs(c.y));
			int maxZ = coordinates.Max (c => (int) Math.Abs(c.z));

			//Look for border tiles and tiles that have neighbours destroyed
			List<Vector3> borderTiles = coordinates
				.FindAll (c => Math.Abs (c.x) == maxX || Math.Abs (c.y) == maxY || Math.Abs (c.z) == maxZ)
				.Except (new List<Vector3>{ rocketCoordinates }).ToList ();
			
			List<Vector3> destroyedNeighbours = new List<Vector3> ();
			destroyedTiles.ForEach (n => destroyedNeighbours.AddRange (ValidNeighbourTiles (n)));
			destroyedNeighbours = destroyedNeighbours.ToList();

			//Add a small change of selecting tiles from the middle
			var nrOfRandomTiles = (borderTiles.Count + destroyedNeighbours.Count) / 4;
			List<Vector3> randomTiles = coordinates
				.Except(new List<Vector3>{rocketCoordinates})
				.OrderBy (x => UnityEngine.Random.Range(0, coordinates.Count))
				.Take (nrOfRandomTiles).ToList();

			//Bias towards border tiles and many destroyed neighbours
			List <Vector3> selectableTiles = borderTiles
				.Concat(borderTiles)
				.Concat(borderTiles)
				.Concat(randomTiles)
				.Concat (destroyedNeighbours)
				.ToList();
			
			int index = UnityEngine.Random.Range(0, selectableTiles.Count);
			Vector3 coordinatesToDestroy = selectableTiles [index];

			LaunchMeteor (coordinatesToDestroy);
		}
	}

    private void DestroyTileAt(Vector3 cubeCoordinates)
    {
        GameObject tile = tileMatrix.GetValue(cubeCoordinates);
        GameObject person = peopleMatrix.GetValue(cubeCoordinates);

        if (peopleMatrix.GetValue(cubeCoordinates) != null)
        {
            person.SetActive(false);
            peopleMatrix.RemoveValue(cubeCoordinates);
            print("DEAD PERSON YOU LOST!!!!!!!");
        }

        tile.GetComponent<TileState>().Destroy();
        destroyedTiles.Add(cubeCoordinates);
    }

    private class MovingPersonData
    {
        public GameObject OriginPerson { get; set; }
        public GameObject DestinationPerson { get; set; }
        public Vector3 Origin { get; set; }
        public Vector3 Destination { get; set; }

        public MovingPersonData(GameObject originPerson, GameObject destinationPerson, Vector3 origin, Vector3 destination)
        {
            OriginPerson = originPerson;
            DestinationPerson = destinationPerson;
            Origin = origin;
            Destination = destination;
        }
    }



	private class MeteorData
	{
		public GameObject Meteor { get; set; }
		public Vector3 TileCoordinate { get; set; }
		public Vector2 TileScreenCoordinate { get; set; }
		public float Distance { get; set; }


				public MeteorData(GameObject meteor, Vector3 tileCoordinate, Vector2 tileScreenCoordinate, float distance)
		{
			TileScreenCoordinate = tileScreenCoordinate;
			Meteor = meteor;
			TileCoordinate = tileCoordinate;
			Distance = distance;
		}
	}
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

	private List<Vector3> destroyedTiles;

	public UnityEngine.UI.Text winText;

	// Use this for initialization
	void Start () {

        tileMatrix = GenerateMap();
        peopleMatrix = PopulateMap();
		rocketCoordinates = PlaceRocket ();

        selectedTile = null;
        selectedPerson = null;

        walkableTiles = new HashSet<GameObject>();
		destroyedTiles = new List<Vector3> ();
		DestructionDaemon ();
    }
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 clickedTileCoordinates = PointyTopSceneToCubeCoordinates(mousePosition);

            GameObject tile = tileMatrix.GetValue( (int)clickedTileCoordinates.x, (int)clickedTileCoordinates.y, (int)clickedTileCoordinates.z);
            GameObject person = peopleMatrix.GetValue((int)clickedTileCoordinates.x, (int)clickedTileCoordinates.y, (int)clickedTileCoordinates.z);

			if (selectedTile != null && selectedPerson != null && ValidNeighbourTiles(selectedTileCoordinates).Contains(clickedTileCoordinates))
            {
                // It's a move action
                selectedPerson.transform.position = CubeToPointyTopSceneCoordinates(clickedTileCoordinates);
                peopleMatrix.RemoveValue(selectedTileCoordinates);

                // It's moving inside the rocket
                print(rocketCoordinates + " " + clickedTileCoordinates);
				if (rocketCoordinates == clickedTileCoordinates) {					
					selectedPerson.SetActive (false);
					nrPersonsIn += 1;
					if (nrPersonsIn >= numberOfPeople) {
						winText.text ="YOU WIN!!";
					}
				}
				peopleMatrix.AddValue (clickedTileCoordinates, selectedPerson);
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
                if (person != null && person != selectedPerson)
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
            Vector2 screenVec = CubeToPointyTopSceneCoordinates(cubeVec);
            GameObject hex = Instantiate(hexTemplate, new Vector3(screenVec.x, screenVec.y, 0), Quaternion.identity);
            tileMatrix.AddValue((int)cubeVec.x, (int)cubeVec.y, (int)cubeVec.z, hex);
        }

        return tileMatrix;
    }

	private Vector3 PlaceRocket()
	{
        Vector3 rocketScenePosition = new Vector3(0, 0, -1);
		GameObject rocket = Instantiate(rocketTemplate, rocketScenePosition, Quaternion.identity);

        return PointyTopSceneToCubeCoordinates(rocketScenePosition);
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
            Vector2 screen = CubeToPointyTopSceneCoordinates(cube);
            GameObject person = Instantiate(personTemplate, new Vector3(screen.x, screen.y, 0), Quaternion.identity);
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
		return CubeCoordinatesNeightbours (coordinates).Except (destroyedTiles).ToList();
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
		InvokeRepeating	("DestroyTile", 3.0f, 3.0f);
	}

	private void DestroyTile(){
		List<Vector3> coordinates = GetAllCubeCoordinates ()
			.Except(destroyedTiles).ToList()
			.Except(new List<Vector3>{rocketCoordinates}).ToList();
		
		if (coordinates.Count <= 0) {
			print ("YOU LOST!!!!!!!");
		}else{
			int maxX = coordinates.Max (c => (int) c.x);
			int maxY = coordinates.Max (c => (int) c.y);
			int maxZ = coordinates.Max (c => (int) c.z);

			List<Vector3> borderTiles = coordinates.FindAll (c => c.x == maxX || c.y == maxY || c.z == maxZ);

			int index = UnityEngine.Random.Range(0, borderTiles.Count);
			Vector3 coordinatesToDestroy = borderTiles [index];

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
    //////////////////////////////////////////////////
    // TODO
    // This whole thing is an inneficient mess. 
    // Rewrite for direct Cube-To-Screen conversion
    //////////////////////////////////////////////////

    // Cube Coordinates to X
    private Vector2 CubeToAxialCoordinates(Vector3 cubeCoordinates)
    {
        float x = cubeCoordinates.x;
        float y = cubeCoordinates.z;
        return new Vector2(x, y);
    }

    private Vector2 CubeToFlatTopSceneCoordinates(Vector3 cubeCoordinates)
    {
        return AxialToFlatTopSceneCoordinates(CubeToAxialCoordinates(cubeCoordinates));
    }

    private Vector2 CubeToPointyTopSceneCoordinates(Vector3 cubeCoordinates)
    {
        return AxialToPointyTopSceneCoordinates(CubeToAxialCoordinates(cubeCoordinates));
    }

    // Axial Coordinates to X
    private Vector3 AxialToCubeCoordinates(Vector2 axialCoordinates)
    {
        float x = axialCoordinates.x;
        float z = axialCoordinates.y;
        float y = -x - z;
        return new Vector3(x, y, z);
    }

    private Vector2 AxialToFlatTopSceneCoordinates(Vector2 axialCoordinates)
    {
        float x = hexRadius * 3 / 2 * axialCoordinates.x;
        float y = hexRadius * Mathf.Sqrt(3) * (axialCoordinates.y + axialCoordinates.x / 2);
        return new Vector2(x, y);
    }

    private Vector2 AxialToPointyTopSceneCoordinates(Vector2 axialCoordinates)
    {
        float x = hexRadius * Mathf.Sqrt(3) * (axialCoordinates.x + axialCoordinates.y / 2);
        float y = hexRadius * 3 / 2 * axialCoordinates.y;

        return new Vector2(x, y);
    }

    // Scene Coordinates to X
    private Vector2 FlatTopSceneToAxialCoordinates(Vector2 sceneCoordinates)
    {
        float x = sceneCoordinates.x * 2 / 3 / hexRadius;
        float y = (-sceneCoordinates.x / 3 + Mathf.Sqrt(3) / 3 * sceneCoordinates.y) / hexRadius;
        return RoundAxialCoordinates(new Vector2(x, y));
    }

    private Vector2 PointyTopSceneToAxialCoordinates(Vector2 sceneCoordinates)
    {
        float x = (sceneCoordinates.x * Mathf.Sqrt(3) / 3 - sceneCoordinates.y / 3) / hexRadius;
        float y = sceneCoordinates.y * 2 / 3 / hexRadius;
        return RoundAxialCoordinates(new Vector2(x, y));
    }

    private Vector3 FlatTopSceneToCubeCoordinates(Vector3 sceneCoordinates)
    {
        return AxialToCubeCoordinates(FlatTopSceneToAxialCoordinates(sceneCoordinates));
    }

    private Vector3 PointyTopSceneToCubeCoordinates(Vector3 sceneCoordinates)
    {
        return AxialToCubeCoordinates(PointyTopSceneToAxialCoordinates(sceneCoordinates));
    }

    // Other coordinate stuff
    private Vector3 RoundCubeCoordinates(Vector3 coordinates)
    {
        float x = Mathf.Round(coordinates.x);
        float y = Mathf.Round(coordinates.y);
        float z = Mathf.Round(coordinates.z);

        float x_diff = Mathf.Abs(x - coordinates.x);
        float y_diff = Mathf.Abs(y - coordinates.y);
        float z_diff = Mathf.Abs(z - coordinates.z);

        if (x_diff > y_diff && x_diff > z_diff)
        {
            x = -y - z;
        }
        else if (y_diff > z_diff)
        {
            y = -x - z;
        }
        else
        {
            z = -x - y;
        }

        return new Vector3(x, y, z);
    }

    private Vector2 RoundAxialCoordinates(Vector2 coordinates)
    {
        return CubeToAxialCoordinates(RoundCubeCoordinates(AxialToCubeCoordinates(coordinates)));
    }

    private float CubeCoordinatesDistance(Vector3 pos1, Vector3 pos2)
    {
        return Mathf.Max(Mathf.Abs(pos1.x - pos2.x), Mathf.Abs(pos1.y - pos2.y), Mathf.Abs(pos1.z - pos2.z));
    }

    private HashSet<Vector3> CubeCoordinatesNeightbours(Vector3 vec)
    {
        HashSet<Vector3> vectors = new HashSet<Vector3>();
        vectors.Add(vec + new Vector3(-1, 1, 0));
        vectors.Add(vec + new Vector3(-1, 0, 1));
        vectors.Add(vec + new Vector3(0, 1, -1));
        vectors.Add(vec + new Vector3(0, -1, 1));
        vectors.Add(vec + new Vector3(1, -1, 0));
        vectors.Add(vec + new Vector3(1, 0, -1));

        return vectors;
    }

    private class GameObjectCubeMatrix : IEnumerable<GameObject>
    {
        private Dictionary<int, Dictionary<int, Dictionary<int, GameObject>>> matrix;

        public GameObjectCubeMatrix()
        {
            matrix = new Dictionary<int, Dictionary<int, Dictionary<int, GameObject>>>();
        }

        public void AddValue(int x, int y, int z, GameObject value)
        {
            Dictionary<int, Dictionary<int, GameObject>> dict1 = null;
            try
            {
                dict1 = matrix[x];
            }
            catch (KeyNotFoundException)
            {
                dict1 = new Dictionary<int, Dictionary<int, GameObject>>();
                matrix[x] = dict1;
            }

            Dictionary<int, GameObject> dict2 = null;
            try
            {
                dict2 = dict1[y];
            }
            catch (KeyNotFoundException)
            {
                dict2 = new Dictionary<int, GameObject>();
                dict1[y] = dict2;
            }

            dict2[z] = value;
        }

        public void AddValue(Vector3 vec, GameObject gameObject)
        {
            AddValue((int)vec.x, (int)vec.y, (int)vec.z, gameObject);
        }

        public GameObject GetValue(int x, int y, int z)
        {
            try
            {
                return matrix[x][y][z];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public GameObject GetValue(Vector3 vec)
        {
            return GetValue((int)vec.x, (int)vec.y, (int)vec.z);
        }

        public void RemoveValue(int x, int y, int z)
        {
            try
            {
                matrix[x][y].Remove(z);
            }
            catch (KeyNotFoundException)
            {
                // pass silently
            }
        }

        public void RemoveValue(Vector3 vec)
        {
            RemoveValue((int)vec.x, (int)vec.y, (int)vec.z);
        }

        public IEnumerator<GameObject> GetEnumerator()
        {
            foreach (int x in matrix.Keys)
            {
                foreach (int y in matrix[x].Keys)
                {
                    foreach (int z in matrix[x][y].Keys)
                    {
                        yield return matrix[x][y][z];
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

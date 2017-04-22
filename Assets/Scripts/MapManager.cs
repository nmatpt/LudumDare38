using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

    public Camera gameCamera;

    public int mapRadius;

    public GameObject hexTemplate;
    public float hexRadius;

    public GameObject personTemplate;
    public int numberOfPeople;

    private GameObjectCubeMatrix tileMatrix;
    private GameObjectCubeMatrix peopleMatrix;
    private GameObject selectedTile;
    private GameObject selectedPerson;
    

	// Use this for initialization
	void Start () {

        tileMatrix = GenerateMap();
        peopleMatrix = PopulateMap();

        selectedTile = null;
        selectedPerson = null;
    }
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 clickedTileCoordinates = PointyTopSceneToCubeCoordinates(mousePosition);

            // handle tiles
            GameObject tile = tileMatrix.GetValue( (int)clickedTileCoordinates.x, (int)clickedTileCoordinates.y, (int)clickedTileCoordinates.z);
            if (tile != null && tile != selectedTile)
            {
                if (selectedTile != null)
                {
                    selectedTile.GetComponent<TileState>().Unselect();
                }
                tile.GetComponent<TileState>().Select();
                selectedTile = tile;
            }

            // handle people
            GameObject person = peopleMatrix.GetValue((int)clickedTileCoordinates.x, (int)clickedTileCoordinates.y, (int)clickedTileCoordinates.z);
            if (person != null && person != selectedPerson)
            {
                if (selectedPerson != null)
                {
                    selectedPerson.GetComponent<PersonAnimator>().Unselect();
                }
                person.GetComponent<PersonAnimator>().Select();
                selectedPerson = person;
            }
            else if (tile != null && selectedPerson != null && selectedPerson != person) // clicked a tile without people
            {
                selectedPerson.GetComponent<PersonAnimator>().Unselect();
                selectedPerson = null;                
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
            tileMatrix.SetValue((int)cubeVec.x, (int)cubeVec.y, (int)cubeVec.z, hex);
        }

        return tileMatrix;
    }

    private GameObjectCubeMatrix PopulateMap()
    {
        GameObjectCubeMatrix peopleMatrix = new GameObjectCubeMatrix();

        List<Vector3> coordinates = GetAllCubeCoordinates();
        if (numberOfPeople > coordinates.Count)
        {
            throw new Exception("Number Of people must be less or equal to the number of tiles");
        }

        for (int i=0; i < numberOfPeople; i++)
        {
            int index = UnityEngine.Random.Range(0, coordinates.Count);
            Vector3 cube = coordinates[index];
            coordinates.RemoveAt(index);
            Vector2 screen = CubeToPointyTopSceneCoordinates(cube);
            GameObject person = Instantiate(personTemplate, new Vector3(screen.x, screen.y, 0), Quaternion.identity);
            peopleMatrix.SetValue((int)cube.x, (int)cube.y, (int)cube.z, person);
        }

        return peopleMatrix;
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
        return roundAxialCoordinates(new Vector2(x, y));
    }

    private Vector2 PointyTopSceneToAxialCoordinates(Vector2 sceneCoordinates)
    {
        float x = (sceneCoordinates.x * Mathf.Sqrt(3) / 3 - sceneCoordinates.y / 3) / hexRadius;
        float y = sceneCoordinates.y * 2 / 3 / hexRadius;
        return roundAxialCoordinates(new Vector2(x, y));
    }

    private Vector3 FlatTopSceneToCubeCoordinates(Vector3 sceneCoordinates)
    {
        return AxialToCubeCoordinates(FlatTopSceneToAxialCoordinates(sceneCoordinates));
    }

    private Vector3 PointyTopSceneToCubeCoordinates(Vector3 sceneCoordinates)
    {
        return AxialToCubeCoordinates(PointyTopSceneToAxialCoordinates(sceneCoordinates));
    }

    private Vector3 roundCubeCoordinates(Vector3 coordinates)
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

    private Vector2 roundAxialCoordinates(Vector2 coordinates)
    {
        return CubeToAxialCoordinates(roundCubeCoordinates(AxialToCubeCoordinates(coordinates)));
    }


    private class GameObjectCubeMatrix : IEnumerable<GameObject>
    {
        private Dictionary<int, Dictionary<int, Dictionary<int, GameObject>>> matrix;

        public GameObjectCubeMatrix()
        {
            matrix = new Dictionary<int, Dictionary<int, Dictionary<int, GameObject>>>();
        }

        public void SetValue(int x, int y, int z, GameObject value)
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

        public GameObject GetValue(int x, int y, int z)
        {
            try
            {
                return matrix[x][y][z];
            }
            catch (System.Exception)
            {
                return null;
            }
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

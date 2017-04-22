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

    private GameObjectMatrix tileMatrix;
    private GameObjectMatrix peopleMatrix;
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
            // Vector2 selectedTileCoordinates = PointyTopSceneToAxialCoordinates(mousePosition);
            Vector2 selectedTileCoordinates = FlatTopSceneToAxialCoordinates(mousePosition);

            // handle tiles
            GameObject tile = tileMatrix.GetValue( (int)selectedTileCoordinates.x, (int)selectedTileCoordinates.y);
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
            GameObject person = peopleMatrix.GetValue((int)selectedTileCoordinates.x, (int)selectedTileCoordinates.y);
            if (person != null && person != selectedPerson)
            {
                if (selectedPerson != null)
                {
                    selectedPerson.GetComponent<PersonAnimator>().Unselect();
                }
                person.GetComponent<PersonAnimator>().Select();
                selectedPerson = person;
            }
            else if (tile != null && selectedPerson != null) // clicked a tile without people
            {
                selectedPerson.GetComponent<PersonAnimator>().Unselect();
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

    private GameObjectMatrix GenerateMap()
    {
        GameObjectMatrix tileMatrix = new GameObjectMatrix();

        foreach (Vector3 cubeVec in GetAllCubeCoordinates())
        {
            Vector2 axialVec = CubeToAxialCoordinates(cubeVec);
            //Vector2 screenVec = AxialToPointyTopSceneCoordinates(axialVec);
            Vector2 screenVec = AxialToFlatTopSceneCoordinates(axialVec);
            GameObject hex = Instantiate(hexTemplate, new Vector3(screenVec.x, screenVec.y, 0), Quaternion.identity);
            tileMatrix.SetValue((int)axialVec.x, (int)axialVec.y, hex);
        }

        return tileMatrix;
    }

    private GameObjectMatrix PopulateMap()
    {
        GameObjectMatrix peopleMatrix = new GameObjectMatrix();

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
            Vector2 axial = CubeToAxialCoordinates(cube);
            //Vector2 screen = AxialToPointyTopSceneCoordinates(axial);
            Vector2 screen = AxialToFlatTopSceneCoordinates(axial);
            GameObject person = Instantiate(personTemplate, new Vector3(screen.x, screen.y, 0), Quaternion.identity);
            peopleMatrix.SetValue((int)axial.x, (int)axial.y, person);
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

    private Vector2 CubeToAxialCoordinates(Vector3 cubeCoordinates)
    {
        float x = cubeCoordinates.x;
        float y = cubeCoordinates.z;
        return new Vector2(x, y);
    }

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


    private class GameObjectMatrix : IEnumerable<GameObject>
    {
        private Dictionary<int, Dictionary<int, GameObject>> matrix;

        public GameObjectMatrix()
        {
            matrix = new Dictionary<int, Dictionary<int, GameObject>>();
        }

        public void SetValue(int x, int y, GameObject value)
        {
            Dictionary<int, GameObject> dict = null;
            try
            {
                dict = matrix[x];
            }
            catch (System.Exception)
            {
                dict = new Dictionary<int, GameObject>();
                matrix[x] = dict;
            }

            dict[y] = value;
        }

        public GameObject GetValue(int x, int y)
        {
            try
            {
                return matrix[x][y];
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
                    yield return matrix[x][y];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

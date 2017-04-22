using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

    public Camera gameCamera;

    public int mapRadius;

    public GameObject hexTemplate;
    public float hexRadius;

    private TileMatrix matrix;
    private GameObject selectedTile;
    

	// Use this for initialization
	void Start () {

        matrix = generateMap();

        selectedTile = null;
	}
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            //print(Input.mousePosition + " " + mousePosition + " " + gameCamera.ScreenToWorldPoint(Input.mousePosition) + " " + FlatTopSceneToAxialCoordinates(mousePosition));
            Vector2 selectedTileCoordinates = FlatTopSceneToAxialCoordinates(mousePosition);
            GameObject tile = matrix.GetValue( (int)selectedTileCoordinates.x, (int)selectedTileCoordinates.y);
            if (tile != null && tile != selectedTile)
            {
                if (selectedTile != null)
                {
                    selectedTile.GetComponent<TileState>().UnselectTile();
                }
                tile.GetComponent<TileState>().SelectTile();
                selectedTile = tile;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            selectedTile = null;
            foreach (GameObject go in matrix)
            {
                Destroy(go);
            }
            matrix = generateMap();
        }
	}

    private TileMatrix generateMap()
    {
        TileMatrix tileMatrix = new TileMatrix();

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
                    Vector3 cubeVec = new Vector3(i, j, k);
                    Vector2 axialVec = CubeToAxialCoordinates(cubeVec);
                    //Vector2 screenVec = AxialToPointyTopSceneCoordinates(axialVec);
                    Vector2 screenVec = AxialToFlatTopSceneCoordinates(axialVec);
                    GameObject hex = Instantiate(hexTemplate, new Vector3(screenVec.x, screenVec.y, 0), Quaternion.identity);
                    tileMatrix.SetValue((int)axialVec.x, (int)axialVec.y, hex);
                }
            }
        }
        return tileMatrix;
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


    private class TileMatrix : IEnumerable<GameObject>
    {
        private Dictionary<int, Dictionary<int, GameObject>> matrix;

        public TileMatrix()
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

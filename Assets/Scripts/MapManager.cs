using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

    public int mapRadius;

    public GameObject hex;
    public float hexRadius;

	// Use this for initialization
	void Start () {
        
        for (int i=-mapRadius; i <= mapRadius; i++)
        {
            for (int j=-mapRadius; j <= mapRadius; j++)
            {
                for (int k=-mapRadius; k <= mapRadius; k++)
                {
                    if (i+j+k != 0)
                    {
                        continue;
                    }
                    Vector3 cubeVec = new Vector3(i, j, k);
                    Vector2 axialVec = CubeToAxialCoordinates(cubeVec);
                    Vector2 screenVec = AxialToPointyTopSceneCoordinates(axialVec);
                    //print(cubeVec + " " + axialVec + " " + screenVec);
                    Instantiate(hex, new Vector3(screenVec.x, screenVec.y, 0), Quaternion.identity);
                }
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
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
}

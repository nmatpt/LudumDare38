using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridUtils
{
    //////////////////////////////////////////////////
    // TODO
    // This whole thing is an inneficient mess. 
    // Rewrite for direct Cube-To-Screen conversion
    //////////////////////////////////////////////////

    // Cube Coordinates to X
    public static Vector2 CubeToAxialCoordinates(Vector3 cubeCoordinates)
    {
        float x = cubeCoordinates.x;
        float y = cubeCoordinates.z;
        return new Vector2(x, y);
    }

    public static Vector2 CubeToFlatTopSceneCoordinates(Vector3 cubeCoordinates, float size)
    {
        return AxialToFlatTopSceneCoordinates(CubeToAxialCoordinates(cubeCoordinates), size);
    }

    public static Vector2 CubeToPointyTopSceneCoordinates(Vector3 cubeCoordinates, float size)
    {
        return AxialToPointyTopSceneCoordinates(CubeToAxialCoordinates(cubeCoordinates), size);
    }

    // Axial Coordinates to X
    public static Vector3 AxialToCubeCoordinates(Vector2 axialCoordinates)
    {
        float x = axialCoordinates.x;
        float z = axialCoordinates.y;
        float y = -x - z;
        return new Vector3(x, y, z);
    }

    public static Vector2 AxialToFlatTopSceneCoordinates(Vector2 axialCoordinates, float size)
    {
        float x = size * 3 / 2 * axialCoordinates.x;
        float y = size * Mathf.Sqrt(3) * (axialCoordinates.y + axialCoordinates.x / 2);
        return new Vector2(x, y);
    }

    public static Vector2 AxialToPointyTopSceneCoordinates(Vector2 axialCoordinates, float size)
    {
        float x = size * Mathf.Sqrt(3) * (axialCoordinates.x + axialCoordinates.y / 2);
        float y = size * 3 / 2 * axialCoordinates.y;

        return new Vector2(x, y);
    }

    // Scene Coordinates to X
    public static Vector2 FlatTopSceneToAxialCoordinates(Vector2 sceneCoordinates, float size)
    {
        float x = sceneCoordinates.x * 2 / 3 / size;
        float y = (-sceneCoordinates.x / 3 + Mathf.Sqrt(3) / 3 * sceneCoordinates.y) / size;
        return RoundAxialCoordinates(new Vector2(x, y));
    }

    public static Vector2 PointyTopSceneToAxialCoordinates(Vector2 sceneCoordinates, float size)
    {
        float x = (sceneCoordinates.x * Mathf.Sqrt(3) / 3 - sceneCoordinates.y / 3) / size;
        float y = sceneCoordinates.y * 2 / 3 / size;
        return RoundAxialCoordinates(new Vector2(x, y));
    }

    public static Vector3 FlatTopSceneToCubeCoordinates(Vector3 sceneCoordinates, float size)
    {
        return AxialToCubeCoordinates(FlatTopSceneToAxialCoordinates(sceneCoordinates, size));
    }

    public static Vector3 PointyTopSceneToCubeCoordinates(Vector3 sceneCoordinates, float size)
    {
        return AxialToCubeCoordinates(PointyTopSceneToAxialCoordinates(sceneCoordinates, size));
    }

    // Other coordinate stuff
    public static Vector3 RoundCubeCoordinates(Vector3 coordinates)
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

    public static Vector2 RoundAxialCoordinates(Vector2 coordinates)
    {
        return CubeToAxialCoordinates(RoundCubeCoordinates(AxialToCubeCoordinates(coordinates)));
    }

    public static float CubeCoordinatesDistance(Vector3 pos1, Vector3 pos2)
    {
        return Mathf.Max(Mathf.Abs(pos1.x - pos2.x), Mathf.Abs(pos1.y - pos2.y), Mathf.Abs(pos1.z - pos2.z));
    }

    public static HashSet<Vector3> CubeCoordinatesNeightbours(Vector3 vec)
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
}

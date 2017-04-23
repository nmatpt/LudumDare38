using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameObjectCubeMatrix : IEnumerable<GameObject>
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
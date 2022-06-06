using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRoughBlockMesher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Rough Cube 
        //30 50 62    48 63   44 66   5 46 67
        //28 57                       7 65
        //32 54                       9 60
        //34 38 53    36 52   40 56   11 42 59

        //Rough Cube 1 
        //18 29 59 ,19 58 ,21 55 ,8 22 54
        //28 57    ,      ,      ,7 48
        //31 52    ,      ,      ,5 47
        //32 42 51 ,41 50 ,39 44 ,4 38 46

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        
        Dictionary<Vector2Int, List<int>> vertexs = new Dictionary<Vector2Int, List<int>>();
        for (int i = 0; i < vertices.Length; i++)
        {
            //Debug.Log(vertices[i].y);
            if ((Mathf.Abs(vertices[i].x) > 0.4f || Mathf.Abs(vertices[i].z) > 0.4f) && vertices[i].y > 0.4f)
            {
                int x = vertices[i].x > 0 ? 2 : -2;
                x = Mathf.Abs(vertices[i].x) > 0.4f ? x : x/2;
                int z = vertices[i].z > 0 ? 2 : -2;
                z = Mathf.Abs(vertices[i].z) > 0.4f ? z : z/2;
                if (!vertexs.ContainsKey(new Vector2Int(x, z))) vertexs.Add(new Vector2Int(x, z), new List<int>());
                vertexs[new Vector2Int(x, z)].Add(i);
            }
        }
        string str = "";
        for (int x = -2; x <= 2; x++) {
            for (int z = -2; z <= 2; z++)
            {
                if (vertexs.ContainsKey(new Vector2Int(x, z)))
                {
                    foreach (int a in vertexs[new Vector2Int(x, z)]) { str += a + " "; }
                }
                else {
                    str += "            ";
                }
                str += ",";
            }
            str += "\n";
        }
        //Debug.Log(str);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

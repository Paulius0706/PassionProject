using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoughMesher : MonoBehaviour
{
    public ChunkController chunkController;
    
    public void ReMesh(Vector3Int position)
    {
        // Rough Cube
        //30 50 62    48 63   44 66   5 46 67
        //28 57                       7 65
        //32 54                       9 60
        //34 38 53    36 52   40 56   11 42 59

        //Rough Cube1 vright ^left >foward <back
        //18 29 59 ,19 58 ,21 55 ,8 22 54
        //28 57    ,      ,      ,7 48
        //31 52    ,      ,      ,5 47
        //32 42 51 ,41 50 ,39 44 ,4 38 46

        Mesh mesh = chunkController.terrainObjects[position].GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        if(!chunkController.IsBlockPalceExist(position + Vector3Int.up))
        {
            // Rough Cube1 vright ^left >foward <back
            //18 29 59 ,19 58 ,21 55 ,8 22 54
            //28 57    ,      ,      ,7 48
            //31 52    ,      ,      ,5 47
            //32 42 51 ,41 50 ,39 44 ,4 38 46
            if (!chunkController.IsBlockPalceExist(position + Vector3Int.right) && !chunkController.IsBlockPalceExist(position + Vector3Int.right + Vector3Int.up))
            {
                vertices[32].y = 0.3125f;
                vertices[42].y = 0.3125f;
                vertices[51].y = 0.3125f;

                vertices[41].y = 0.3125f;
                vertices[50].y = 0.3125f;

                vertices[39].y = 0.3125f;
                vertices[44].y = 0.3125f;

                vertices[4].y = 0.3125f;
                vertices[38].y = 0.3125f;
                vertices[46].y = 0.3125f;
            }
            if (!chunkController.IsBlockPalceExist(position + Vector3Int.left) && !chunkController.IsBlockPalceExist(position + Vector3Int.left + Vector3Int.up))
            {
                vertices[18].y = 0.3125f;
                vertices[29].y = 0.3125f;
                vertices[59].y = 0.3125f;

                vertices[19].y = 0.3125f;
                vertices[58].y = 0.3125f;

                vertices[21].y = 0.3125f;
                vertices[55].y = 0.3125f;

                vertices[8].y = 0.3125f;
                vertices[22].y = 0.3125f;
                vertices[54].y = 0.3125f;
            }
            if (!chunkController.IsBlockPalceExist(position + Vector3Int.forward) && !chunkController.IsBlockPalceExist(position + Vector3Int.forward + Vector3Int.up))
            {
                vertices[8].y = 0.3125f;
                vertices[22].y = 0.3125f;
                vertices[54].y = 0.3125f;

                vertices[7].y = 0.3125f;
                vertices[48].y = 0.3125f;

                vertices[5].y = 0.3125f;
                vertices[47].y = 0.3125f;

                vertices[4].y = 0.3125f;
                vertices[38].y = 0.3125f;
                vertices[46].y = 0.3125f;
            }
            if (!chunkController.IsBlockPalceExist(position + Vector3Int.back) && !chunkController.IsBlockPalceExist(position + Vector3Int.back + Vector3Int.up))
            {
                vertices[18].y = 0.3125f;
                vertices[29].y = 0.3125f;
                vertices[59].y = 0.3125f;

                vertices[28].y = 0.3125f;
                vertices[57].y = 0.3125f;

                vertices[31].y = 0.3125f;
                vertices[52].y = 0.3125f;

                vertices[32].y = 0.3125f;
                vertices[42].y = 0.3125f;
                vertices[51].y = 0.3125f;
            }

            if(!chunkController.IsBlockPalceExist(position + Vector3Int.back+ Vector3Int.left) && !chunkController.IsBlockPalceExist(position + Vector3Int.back + Vector3Int.up + Vector3Int.left))
            {
                vertices[18].y = 0.3125f;
                vertices[29].y = 0.3125f;
                vertices[59].y = 0.3125f;
            }
            if (!chunkController.IsBlockPalceExist(position + Vector3Int.back + Vector3Int.right) && !chunkController.IsBlockPalceExist(position + Vector3Int.back + Vector3Int.up + Vector3Int.right))
            {
                vertices[32].y = 0.3125f;
                vertices[42].y = 0.3125f;
                vertices[51].y = 0.3125f;
            }
            if (!chunkController.IsBlockPalceExist(position + Vector3Int.forward + Vector3Int.left) && !chunkController.IsBlockPalceExist(position + Vector3Int.forward + Vector3Int.up + Vector3Int.left))
            {
                vertices[8].y = 0.3125f;
                vertices[22].y = 0.3125f;
                vertices[54].y = 0.3125f;
            }
            if (!chunkController.IsBlockPalceExist(position + Vector3Int.forward + Vector3Int.right) && !chunkController.IsBlockPalceExist(position + Vector3Int.forward + Vector3Int.up + Vector3Int.right))
            {
                vertices[4].y = 0.3125f;
                vertices[38].y = 0.3125f;
                vertices[46].y = 0.3125f;
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            chunkController.terrainObjects[position].GetComponent<MeshFilter>().mesh = mesh;
            
        }
    }
}

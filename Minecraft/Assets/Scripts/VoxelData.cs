using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelData
{
    private static Vector3[] vertices = new Vector3[]
    {
        new Vector3(0,0,0),
        new Vector3(1,0,0),
        new Vector3(1,1,0),
        new Vector3(0,1,0),
        new Vector3(0,0,1),
        new Vector3(1,0,1),
        new Vector3(1,1,1),
        new Vector3(0,1,1),
    };

    public enum Face
    {
        FRONT = 0,
        RIGHT = 1,
        BACK = 2,
        LEFT = 3,
        TOP = 4,
        BOTTOM = 5
    };

    private static List<int[]> faces = new List<int[]>()
    {
        new int[]{0,3,1, 1,3,2},   //front
        new int[]{1,2,5, 5,2,6},   //right
        new int[]{5,6,4, 4,6,7},   //back
        new int[]{4,7,0, 0,7,3},   //left
        new int[]{3,7,2, 2,7,6},   //top
        new int[]{4,0,5, 5,0,1}    //bottom
    };

    private static List<Face> allFaces = new List<Face>()
    {
        Face.FRONT,
        Face.RIGHT,
        Face.BACK,
        Face.LEFT,
        Face.TOP,
        Face.BOTTOM
    };

    public static Vector3[] Vertices { get => vertices; }
    public static List<int[]> Faces { get => faces; }

    public static List<Face> AllFaces { get => allFaces; }
}

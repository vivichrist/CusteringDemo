using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]

class LightATest : MonoBehaviour
{
    public uint Z_Divisions = 32u, TileSize = 64u;
    private ulong sizex = 0, sizey = 0, sizez = 0;
    private Vector3[] vs;
    private int[] indices;
    private int[] bvindices;
    private Mesh mesh;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

//    void Awake()
//    {
//    }

    void Start()
    {
        LightAWrapper.createLightAssignment(Mathf.Deg2Rad * 60f, 800u, 600u, 5.0f, 50.0f);
        LightAWrapper.createFrustumMatrix(Mathf.Deg2Rad * 60f, 800u, 600u, 5.0f, 50.0f);
        Vector3 cpos = transform.position, updir = transform.up, dir = transform.forward;
        LightAWrapper.createCullingPlanes(cpos, updir, dir, TileSize, Z_Divisions);
        LightAWrapper.createPointGrid(cpos, updir, dir, TileSize, Z_Divisions);
        LightAWrapper.getMatrixSize(ref sizex, ref sizey, ref sizez);
        mesh = GetComponent<MeshFilter>().mesh;
        bvindices = new int[24];
        drawGrid();
        mesh.subMeshCount = 2;
    }

    void drawGrid()
    {
        vs = new Vector3[sizex * sizey * sizez];
        for ( uint k = 0; k < sizez; k++ )
        {
            for ( uint j = 0; j < sizey; j++ )
            {
                for ( uint i = 0; i < sizex; i++ )
                {
                    Vector3 v = LightAWrapper.getElement( i, j, k );
                    vs[i * sizey * sizez + j * sizez + k] = v;
                }
            }
        }
        mesh.vertices = vs;
        List<int> inds = new List<int>();
        int sx = Convert.ToInt32(sizex), sy = Convert.ToInt32(sizey), sz = Convert.ToInt32(sizez);
        for ( int k = 0; k < sz; k++ )
        {
            for ( int j = 0; j < sy; j++ )
            {
                for ( int i = 0; i < sx; i++ )
                {
                    int xdim = sy * sz;
                    int ind = i * xdim + j * sz + k;
                    int ind2 = (i + 1) * xdim + j * sz + k;
                    int ind3 = i * xdim + (j + 1) * sz + k;
                    int ind5 = i * xdim + j * sz + k + 1;
                    if (i < sx - 1)
                    {
                        inds.Add(ind);
                        inds.Add(ind2);
                    }
                    if (j < sy - 1)
                    {
                        inds.Add(ind);
                        inds.Add(ind3);
                    }
                    if (k < sz - 1)
                    {
                        inds.Add(ind);
                        inds.Add(ind5);
                    }
                }
            }
        }
        indices = inds.ToArray();
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
    }

    void updateLA()
    {
        LightAWrapper.clearLightAssignment();
        LightAWrapper.createCullingPlanes(transform.position, transform.up, transform.forward, TileSize, Z_Divisions);
    }

    void updateFG()
    {
        LightAWrapper.clearFrustumMatrix();
        LightAWrapper.createPointGrid(transform.position, transform.up, transform.forward, TileSize, Z_Divisions);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Vector3 position = this.transform.position;
            position.x--;
            this.transform.position = position;
        }else if (Input.GetKeyDown(KeyCode.L))
        {
            Vector3 position = this.transform.position;
            position.x++;
            this.transform.position = position;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            transform.localRotation = Quaternion.AngleAxis(++rotationX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
        }else if (Input.GetKeyDown(KeyCode.Period))
        {
            transform.localRotation = Quaternion.AngleAxis(--rotationX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Vector3 position = this.transform.position;
            position.z++;
            this.transform.position = position;
        }else if (Input.GetKeyDown(KeyCode.K))
        {
            Vector3 position = this.transform.position;
            position.z--;
            this.transform.position = position;
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(++rotationY, Vector3.left);
        }else if (Input.GetKeyDown(KeyCode.H))
        {
            transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(--rotationY, Vector3.left);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Vector3 position = this.transform.position;
            position.y++;
            this.transform.position = position;
        }else if (Input.GetKeyDown(KeyCode.O))
        {
            Vector3 position = this.transform.position;
            position.y--;
            this.transform.position = position;
        }
        if (transform.hasChanged)
        {
            updateLA();
        }
        GameObject sph = GameObject.Find("Sphere");
        if (LightAWrapper.isVisible(sph.transform.position, sph.transform.localScale.x))
        {
            int sy = Convert.ToInt32(sizey), sz = Convert.ToInt32(sizez);
            uint[] aabb = LightAWrapper.scanLight(sph.transform.position, sph.transform.localScale.x / 2.0f);
            //print( "aabb: " + aabb[0] + "," + aabb[1] + "," + aabb[2] + "," + aabb[3] + "," + aabb[4] + "," + aabb[5]+ "\n" );
            int l = (int)aabb[0], r = (int)aabb[1], u = (int)aabb[2], d = (int)aabb[3], n = (int)aabb[4], f = (int)aabb[5];

            bvindices[0] = l * sy * sz + d * sz + n;
            bvindices[1] = l * sy * sz + u * sz + n;
            bvindices[2] = l * sy * sz + u * sz + f;
            bvindices[3] = l * sy * sz + d * sz + f;

            bvindices[4] = r * sy * sz + d * sz + f;
            bvindices[5] = r * sy * sz + u * sz + f;
            bvindices[6] = r * sy * sz + u * sz + n;
            bvindices[7] = r * sy * sz + d * sz + n;

            bvindices[8] =  l * sy * sz + u * sz + n;
            bvindices[9] =  r * sy * sz + u * sz + n;
            bvindices[10] = r * sy * sz + u * sz + f;
            bvindices[11] = l * sy * sz + u * sz + f;

            bvindices[13] = l * sy * sz + d * sz + n;
            bvindices[12] = r * sy * sz + d * sz + n;
            bvindices[15] = r * sy * sz + d * sz + f;
            bvindices[14] = l * sy * sz + d * sz + f;

            bvindices[16] = l * sy * sz + u * sz + n;
            bvindices[17] = l * sy * sz + d * sz + n;
            bvindices[18] = r * sy * sz + d * sz + n;
            bvindices[19] = r * sy * sz + u * sz + n;

            bvindices[20] = l * sy * sz + d * sz + f;
            bvindices[21] = l * sy * sz + u * sz + f;
            bvindices[22] = r * sy * sz + u * sz + f;
            bvindices[23] = r * sy * sz + d * sz + f;

            //print("bounds: " + left + "," + right + "," + upper + "," + lower + "," + n + "," + f);
            //print("ubounds: " + sizex + "," + sizey + "," + sizez);
            mesh.SetIndices(bvindices, MeshTopology.Quads, 1);
        }
    }
}

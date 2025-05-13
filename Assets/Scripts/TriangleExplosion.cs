using UnityEngine;
 using System.Collections;
 using System.Collections.Generic;
 
 
 public class TriangleExplosion : MonoBehaviour {
    public GameObject newparent;
    public Vector3 offset = new Vector3(0f,0f,0f);
    public Vector3 norm = new Vector3(0f,2f,0f);
    //public GameObject expobj;
 
     public IEnumerator SplitMesh (bool destroy)    {
        /*
         if (tag.Equals("cone"))
         {
            offset = norm * gameObject.GetComponent<Transform>().localScale.x;
         }
         */
 
         if(GetComponent<MeshFilter>() == null || GetComponent<SkinnedMeshRenderer>() == null) {
             yield return null;
         }
 
         if(GetComponent<Collider>()) {
             GetComponent<Collider>().enabled = false;
         }
 
         Mesh M = new Mesh();
         if(GetComponent<MeshFilter>()) {
             M = GetComponent<MeshFilter>().mesh;
         }
         else if(GetComponent<SkinnedMeshRenderer>()) {
             M = GetComponent<SkinnedMeshRenderer>().sharedMesh;
         }

         Material[] materials = new Material[0];
         if(GetComponent<MeshRenderer>()) {
             materials = GetComponent<MeshRenderer>().materials;
         }
         else if(GetComponent<SkinnedMeshRenderer>()) {
             materials = GetComponent<SkinnedMeshRenderer>().materials;
         }
 
         
         
         //expobj.GetComponent<Experimentscript>().tm.text = cullingChance.ToString() + " " + cullingScaleCorrection.ToString();

         float cullingChance2 = 1f - (100f/M.vertices.Length);
         float cullingScaleCorrection = 1f / (1f-(cullingChance2/2f));
         if (cullingScaleCorrection < 1f) {
              cullingScaleCorrection = 1f;
         }

         //custom mesh scaling
         float ScaleX = gameObject.GetComponent<Transform>().localScale.x;
         float ScaleY = gameObject.GetComponent<Transform>().localScale.y;
         float ScaleZ = gameObject.GetComponent<Transform>().localScale.z;
         Vector3[] _baseVertices;
         bool RecalculateNormals = false;
         var meshs = M;
         _baseVertices = meshs.vertices;
         var vertices = new Vector3[_baseVertices.Length];
         for (var i = 0; i < vertices.Length; i++)
         {
             var vertex = _baseVertices[i];
             vertex.x = vertex.x * ScaleX * cullingScaleCorrection;
             vertex.y = vertex.y * ScaleY * cullingScaleCorrection;
             vertex.z = vertex.z * ScaleZ * cullingScaleCorrection;
             vertices[i] = vertex;
         }
         meshs.vertices = vertices;
         if (RecalculateNormals)
             meshs.RecalculateNormals();
         meshs.RecalculateBounds();

         Vector3[] verts = M.vertices;
         Vector3[] normals = M.normals;
         Vector2[] uvs = M.uv;
         newparent = new GameObject();

         //expobj = GameObject.FindGameObjectsWithTag("experimentobj")[0];
         //expobj.GetComponent<Experimentscript>().tm.text = verts.Length.ToString();

         //determine culling

         float cullingChance;
         if (verts.Length > 0)
         {
            cullingChance = 1f - (100f/verts.Length);
         } else {
            cullingChance = 0f;
         }


         /*
         if (M.subMeshCount > 30) {
              if (UnityEngine.Random.Range((int)0, M.subMeshCount) )
         }
         */
         for (int submesh = 0; submesh < M.subMeshCount; submesh++) {
             int[] indices = M.GetTriangles(submesh);
 
             for (int i = 0; i < indices.Length; i += 3)    {
                
                 
                 if (Random.Range(0f, 1f) < cullingChance) {
                    continue;
                 }
                 

                 Vector3[] newVerts = new Vector3[3];
                 Vector3[] newNormals = new Vector3[3];
                 Vector2[] newUvs = new Vector2[3];
                 for (int n = 0; n < 3; n++)    {
                     int index = indices[i + n];
                     newVerts[n] = verts[index];
                     newUvs[n] = uvs[index];
                     newNormals[n] = normals[index];
                 }
 
                 Mesh mesh = new Mesh();
                 mesh.vertices = newVerts;
                 mesh.normals = newNormals;
                 mesh.uv = newUvs;
                 
                 mesh.triangles = new int[] { 0, 1, 2, 2, 1, 0 };
 
                 GameObject GO = new GameObject("Triangle " + (i / 3));
                 GO.layer = LayerMask.NameToLayer("Particle");
                 //GO.tag = "part";
                 GO.transform.position = transform.position;
                 GO.transform.rotation = transform.rotation;
                 GO.AddComponent<MeshRenderer>().material = materials[submesh];
                 GO.AddComponent<MeshFilter>().mesh = mesh;
                 GO.AddComponent<BoxCollider>();
                 Vector3 explosionPos = new Vector3(transform.position.x + Random.Range(-0.5f, 0.5f), transform.position.y + Random.Range(0f, 0.5f), transform.position.z + Random.Range(-0.5f, 0.5f));
                 explosionPos += offset;
                 GO.AddComponent<Rigidbody>().AddExplosionForce(Random.Range(30,30), explosionPos, 5, 0.2f);
                 GO.GetComponent<Rigidbody>().useGravity = false;
                 GO.GetComponent<Transform>().parent = newparent.transform;
                 Destroy(GO, Random.Range(0.5f, 1.0f));
             }
         }

         GetComponent<Renderer>().enabled = false;
         GetComponent<Shapescript>().exploded = true;

         yield return new WaitForSeconds(1.0f);
         if(destroy == true) {
             Destroy(gameObject);
         }
         
 
     }
 
 
 }
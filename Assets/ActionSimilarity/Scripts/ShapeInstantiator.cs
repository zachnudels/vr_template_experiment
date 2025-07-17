// using UnityEngine;
//
// namespace ActionSimilarity
// {
//     public static class ShapeFactory
//     {
//         public static GameObject InstantiateObjectWithMesh(GameObject prefab,
//             Transform transform,
//             Mesh mesh,
//             Vector3 position,
//             Vector3 rotation,
//             string location)
//         {
//
//             //prefab.GetComponent<Stimulus>().session = session;
//             //prefab.GetComponent<Stimulus>().location = location;
//
//             GameObject gameObject = Instantiate(
//                 prefab,
//                 position,
//                 Quaternion.Euler(rotation),
//                 transform);
//             gameObject.GetComponent<MeshFilter>().mesh = mesh;
//             //gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
//             return gameObject;
//         }
//
//         GameObject InstantiateObjectWithMeshAndColor(GameObject prefab,
//             Mesh mesh,
//             Vector3 position,
//             Vector3 rotation,
//             Color color,
//             string location)
//         {
//
//
//             GameObject gameObject = Instantiate(
//                 prefab,
//                 position,
//                 Quaternion.Euler(rotation),
//                 this.transform);
//             gameObject.GetComponent<MeshFilter>().mesh = mesh;
//             gameObject.GetComponent<Renderer>().material.color = color;
//             //gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
//             return gameObject;
//         }
//
//         GameObject InstantiateObject(GameObject prefab,
//             Vector3 position,
//             Vector3 rotation,
//             string location)
//         {
//
//             //prefab.GetComponent<Stimulus>().session = session;
//             //prefab.GetComponent<Stimulus>().location = location;
//
//             GameObject gameObject = Instantiate(
//                 prefab,
//                 position,
//                 Quaternion.Euler(rotation),
//                 this.transform);
//             //gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
//             return gameObject;
//         }
//     }
// }
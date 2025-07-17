using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AssetManager : MonoBehaviour {

    public Dictionary<string, GameObject> preloadedObjects = new Dictionary<string, GameObject>();

    // Operation handle used to load and release assets
    AsyncOperationHandle<IList<IResourceLocation>> loadResourceLocationsHandle;


    public IEnumerator Start() {
        //find all the locations with label "SpaceHazards"
        loadResourceLocationsHandle = Addressables.LoadResourceLocationsAsync("default", typeof(GameObject));

        if (!loadResourceLocationsHandle.IsDone)
            yield return loadResourceLocationsHandle;

        //start each location loading
        List<AsyncOperationHandle> opList = new List<AsyncOperationHandle>();

        foreach (IResourceLocation location in loadResourceLocationsHandle.Result) {
            AsyncOperationHandle<GameObject> loadAssetHandle = Addressables.LoadAssetAsync<GameObject>(location);
            loadAssetHandle.Completed += obj => { preloadedObjects.Add(location.PrimaryKey, obj.Result); };
            opList.Add(loadAssetHandle);
        }

        //create a GroupOperation to wait on all the above loads at once. 
        var groupOp = Addressables.ResourceManager.CreateGenericGroupOperation(opList);

        if (!groupOp.IsDone)
            yield return groupOp;

        

        //take a gander at our results.
        //foreach (var item in preloadedObjects) {
         //   Debug.Log(item.Key + " - " + item.Value.name);
        //}
    }

    private void OnDestroy() {
        Addressables.Release(loadResourceLocationsHandle);
        // Release all the loaded assets associated with loadHandle
        // Note that if you do not make loaded addressables a child of this object,
        // then you will need to devise another way of releasing the handle when
        // all the individual addressables are destroyed.
    }
}
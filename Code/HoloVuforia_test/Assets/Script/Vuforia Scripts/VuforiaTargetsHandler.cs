using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.XR.CoreUtils;

public class VuforiaTargetsHandler : MonoBehaviour {

    // To inform the menu of the result of operations
    public static int DOWNLOAD_SUCCEEDED = 0;
    public static int DOWNLOAD_FAILED = 1;
    public static int INVALID_INDEX = 2;
    public static int EMPTY_TARGETS_LIST = 3;
    public static int DOWNLOADING = 4;


    [SerializeField] AssetReference[] targetsAddressables;        // ORDERED array (with respect to step order) of target addressable prefabs
    private GameObject[] targets;                                 // array of target INSTANTS (i.e. not prefabs but GameObjects)

    private int currentTarget = -1;

    private Action<int> menuCallback;

    void Start() {
        // Per gestire le eccezioni nel download dei remote bundle, necessario perche' Addressables non gestisce niente
        ResourceManager.ExceptionHandler = CustomExceptionHandler;
    }

    public void setCallback(Action<int> callback) {
        menuCallback = callback;
    }

    public void startTargets() {

        if (targetsAddressables.Length < 1) {
            Debug.Log("No targets added to targetsAddressables array!");
            // Notify menu
            menuCallback(EMPTY_TARGETS_LIST);
        }
        else { 
            // Initialise array targets of the same size as targetsAddressables
            targets = new GameObject[targetsAddressables.Length];
            // Download and instantiate the first addressable target
            instantiateTarget(0);
        }
    }



    private void instantiateTarget(int id) {

        // Set the Instructions scene as the active scene, otherwise objects are instantiated in the menu scene
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(gameObject.scene);
        // Informa il menu
        menuCallback(DOWNLOADING);

        // Download and instantiate the addressable
        // Asynchronous operation, so when it is called, it is executed in a coroutine and the script does not freeze and move forward
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(targetsAddressables[id]);  
        handle.Completed += (AsyncOperationHandle<GameObject> a) =>         
        // The operation once finished starts the following code that handles the result
        {
            // FAILED
            if (handle.Status == AsyncOperationStatus.Failed) {

                Debug.Log("Remote bundle download failed.");

                // Notify Menu
                menuCallback(DOWNLOAD_FAILED);
            }

            // SUCCEEDED
            else if (handle.Status == AsyncOperationStatus.Succeeded) {

                // When you have downloaded it, pass it its id and this script:
                targets[id] = handle.Result;


                currentTarget = id;

                // Notify Menu
                menuCallback(DOWNLOAD_SUCCEEDED);
            }

            // Restore the active scene from before, if it has not been changed by something else in the meantime
            if (SceneManager.GetActiveScene() == gameObject.scene) {
                SceneManager.SetActiveScene(activeScene);
            }
        };
    }


    public void jumpToTarget(int id) {

        // If the addressable list is empty
        if (targetsAddressables.Length < 1) {
            menuCallback(EMPTY_TARGETS_LIST);
            return;
        }

        // If id does not have a valid value
        if (id < 0 || id >= targetsAddressables.Length) {
            menuCallback(INVALID_INDEX);
            return;
        }

        // If the target id is already instantiated
        if (targets[id] != null) {
            menuCallback(DOWNLOAD_SUCCEEDED);
            return;
        }

        // Delete current target
        if (currentTarget >= 0 && currentTarget < targets.Length && targets[currentTarget] != null) {
            Addressables.ReleaseInstance(targets[currentTarget]);
        }

        // Target Instance
        instantiateTarget(id);
    }
//nextTarget (called from InstructionMenu) starts the jumpToTarget function, 
// passing the index of the next target as a parameter, which is calculated via the nextTargetId function
    public void nextTarget() {       
        jumpToTarget(nextTargetId(currentTarget));    
    }
    public void previousTarget() {
        jumpToTarget(previousTargetId(currentTarget));
    }

    private int nextTargetId(int targetId) {
        int targ = targetId + 1;
        if (targ >= targetsAddressables.Length) targ = 0;

        return targ;
    }

    private int previousTargetId(int targetId) {
        int targ = targetId - 1;
        if (targ < 0) targ = targetsAddressables.Length - 1;

        return targ;
    }


    public int getCurrentTarget() {
        return currentTarget;
    }
    public int getInstructionsCount() {
        return targetsAddressables.Length;
    }


    void CustomExceptionHandler(AsyncOperationHandle handle, Exception exception) {
        if (exception.HResult == -2146233088)
        {
            Debug.Log("Addressables: Connection error, failed to load remote bundle.");
        }
        else
        {
            Debug.LogWarning("Unhandled Error + " + exception.Message);
        }
    }
}


/* NOTA:
 * Addressables memory management:
 * https://docs.unity3d.com/Packages/com.unity.addressables@1.3/manual/MemoryManagement.html
 * "You can load an asset bundle, or its partial contents, but you cannot partially unload an asset bundle."
 * Quindi se vogliamo evitare di tenere tutti i target in memoria, dobbiamo mettere ogni target
 * in un asset bundle (cioe' addressables group) diverso.
 * In questo modo facendo il release del target dovrebbe eliminare dalla memoria tutto il bundle
*/
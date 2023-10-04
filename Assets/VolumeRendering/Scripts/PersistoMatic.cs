
/*using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;

public class PersistoMatic : MonoBehaviour
{
    public string ObjectAnchorStoreName;

    public GameObject AnchoredContent;

    WorldAnchorStore AnchorStore;

    bool PlacingObj = false;

    // Start is called before the first frame update
    void Start()
    {
        // set up the database of anchors
        WorldAnchorStore.GetAsync(AnchorStoreReady);
    }

    public void AnchorStoreReady(WorldAnchorStore store)
    {
        AnchorStore = store;
        string[] ids = AnchorStore.GetAllIds();
        for (int index = 0; index < ids.Length; index++)
        {
            if (ids[index] == ObjectAnchorStoreName)
            {
                AnchorStore.Load(ids[index], gameObject);
                break;
            }
        }
    }

    public void OnSelect()
    {

        if (AnchorStore == null)
        {
            return;
        }

        if (PlacingObj)
        {
            // If object needs to be placed else were
            WorldAnchor attachingAnchor = gameObject.AddComponent<WorldAnchor>();
            if (attachingAnchor.isLocated)
            {
                Debug.Log("Saving persisted position immediately");
                bool saved = AnchorStore.Save(ObjectAnchorStoreName, attachingAnchor);
                Debug.Log("saved: " + saved);
            }
            else
            {
                attachingAnchor.OnTrackingChanged += attachingAnchor_OnTrackingChanged;
            }

            // hide the anchored content when moving the object
            if (AnchoredContent != null)
            {
                AnchoredContent.SetActive(true);
            }
        }
        else
        {
            // if the user does want to move the object first delete the world anchor
            WorldAnchor anchor = gameObject.GetComponent<WorldAnchor>();
            if (AnchorStore != null)
            {
                DestroyImmediate(anchor);
            }

            // hide the anchored content when moving the object
            if (AnchoredContent != null)
            {
                AnchoredContent.SetActive(false);
            }

            string[] ids = AnchorStore.GetAllIds();
            for (int index = 0; index < ids.Length; index++)
            {
                Debug.Log(ids[index]);
                if (ids[index] == ObjectAnchorStoreName)
                {
                    bool deleted = AnchorStore.Delete(ids[index]);
                    Debug.Log("deleted: " + deleted);
                    break;
                }
            }
        }

        PlacingObj = !PlacingObj;
    }


    private void attachingAnchor_OnTrackingChanged(WorldAnchor self, bool located)
    {
        if (located)
        {
            Debug.Log("Saving persisted position in callback");
            bool saved = AnchorStore.Save(ObjectAnchorStoreName, self);
            Debug.Log("saved: " + saved);
            self.OnTrackingChanged -= attachingAnchor_OnTrackingChanged;
        }
    }
}*/
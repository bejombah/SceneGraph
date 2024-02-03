using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneGraph
{
    [ExecuteInEditMode]
    public class PortalManager : MonoBehaviour
    {
        public static PortalManager Instance { get; private set; }

        [SerializeField] SceneInstance sceneInstance; public SceneInstance SceneInstance { get => sceneInstance; set => sceneInstance = value; }
        [SerializeField] List<GameObject> portals = new List<GameObject>(); public List<GameObject> Portals { get => portals; set => portals = value; }

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            portals = new List<GameObject>();
            foreach (Transform child in transform)
            {
                portals.Add(child.gameObject);
                child.gameObject.GetComponent<Portal>().PortalData.GUID = sceneInstance.Portals.Find(x => x.UniqueID == child.gameObject.GetComponent<Portal>().PortalData.UniqueID).GUID;
            }
        }

        public GameObject FindDoor(string guid)
        {
            GameObject go = portals.Find(x => x.GetComponent<Portal>().PortalData.GUID == guid);

            if(go != null)
            {
                return go;
            }
            else
            {
                return null;
            }
        }
    }
}

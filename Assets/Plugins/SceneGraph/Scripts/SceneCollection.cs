using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SceneGraph
{
    public class SceneCollection : ScriptableObject
    {
        [Header("Prefabs")]
        [SerializeField] GameObject portalManagerPrefab; public GameObject PortalManagerPrefab => portalManagerPrefab;
        [SerializeField] GameObject portalPrefab; public GameObject PortalPrefab => portalPrefab;
        [SerializeField] GameObject gridPrefab; public GameObject GridPrefab => gridPrefab;
        [SerializeField] GameObject camSystemPrefab; public GameObject CamSystemPrefab => camSystemPrefab;
        [SerializeField] SceneAsset coreScene; public SceneAsset CoreScene => coreScene;
        [SerializeField] List<SceneInstance> scenes = new List<SceneInstance>(); public List<SceneInstance> Scenes => scenes;

        [Header("Editor")]
        public Vector3 position;
        public Vector3 zoom;
        public SceneInstance GetSceneInstance(string sceneName)
        {
            foreach (SceneInstance scene in scenes)
            {
                if (scene.name == sceneName)
                {
                    return scene;
                }
            }
            return null;
        }
    }
}

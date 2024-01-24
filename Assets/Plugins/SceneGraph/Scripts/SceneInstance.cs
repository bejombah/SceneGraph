using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


namespace SceneGraph
{
    public class SceneInstance : ScriptableObject
    {
        [SerializeField] string guid = System.Guid.NewGuid().ToString(); public string GUID => guid;
        [SerializeField] SceneAsset _sceneA; public SceneAsset SceneA => _sceneA;
        [SerializeField] string mapName; public string MapName => mapName;

        public SceneAsset sceneA
        {
            get { return _sceneA; }
            set
            {
                if(_sceneA != value)
                {
                    _sceneA = value;
                    OnValueChanged(_sceneA.name);
                }
            }
        }

        [SerializeField] List<Portal> portals = new List<Portal>(); public List<Portal> Portals => portals;

        #if UNITY_EDITOR
        public Rect position;
        public Sprite preview;
        public Bounds bounds;
        
        void OnValidate()
        {
            if(_sceneA != null)
            {
                if(_sceneA.name != this.name)
                {
                    OnValueChanged(_sceneA.name);
                }
            }
        }

        void OnValueChanged(string value)
        {
            this.name = value;
        }
        #endif
    }

    [System.Serializable]
    public class Portal
    {
        [SerializeField] string uniqueID = System.Guid.NewGuid().ToString(); public string UniqueID => uniqueID;
        [SerializeField] string guid; public string GUID => guid;
        [SerializeField] string name; public string Name => name;
        [SerializeField] Vector3 portalPosition; public Vector3 PortalPosition => portalPosition;
        [SerializeField] SceneAsset sceneTo; public SceneAsset SceneTo => sceneTo;
        [SerializeField] Orientation orientation; public Orientation Orientation => orientation;
        [SerializeField] Direction direction; public Direction Direction => direction;
        [SerializeField] bool isInteraction; public bool IsInteraction => isInteraction;

        [Header("Editor")]
        public Vector2 position;

        public void SetConnection(string guid, SceneAsset sceneTo)
        {
            this.guid = guid;
            this.sceneTo = sceneTo;

            AssetDatabase.SaveAssets();
        }
    }
}

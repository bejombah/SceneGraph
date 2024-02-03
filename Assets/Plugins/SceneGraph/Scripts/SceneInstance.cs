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
        [SerializeField] string mapName; public string MapName { get => mapName; set => mapName = value; }

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

        [SerializeField] List<PortalData> portals = new List<PortalData>(); public List<PortalData> Portals => portals;

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
    public class PortalData
    {
        [SerializeField] string uniqueID = System.Guid.NewGuid().ToString(); public string UniqueID => uniqueID;
        [SerializeField] string guid; public string GUID { get => guid; set => guid = value;}
        [SerializeField] string name; public string Name { get => name; set => name = value;}
        [SerializeField] Vector3 portalPosition; public Vector3 PortalPosition { get => portalPosition; set => portalPosition = value;}
        [SerializeField] SceneAsset sceneTo; public SceneAsset SceneTo { get => sceneTo; set => sceneTo = value;}
        [SerializeField] Orientation orientation; public Orientation Orientation { get => orientation; set => orientation = value;}
        [SerializeField] Direction direction; public Direction Direction { get => direction; set => direction = value;}
        [SerializeField] bool isInteraction; public bool IsInteraction { get => isInteraction; set => isInteraction = value;}

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

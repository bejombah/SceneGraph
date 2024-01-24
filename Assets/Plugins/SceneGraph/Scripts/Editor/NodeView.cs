using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.Tilemaps;

namespace SceneGraph.Editor
{
    public class NodeView : Node
    {
        [SerializeField] SceneInstance sceneInstance; public SceneInstance SceneInstance => sceneInstance;
        VisualElement grid;
        public NodeView(SceneInstance sceneInstance) : base("Assets/Plugins/SceneGraph/Scripts/Editor/NodeView.uxml")
        {
            this.sceneInstance = sceneInstance;
        }
    }
}

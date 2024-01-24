using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using SceneGraph;

namespace SceneGraph.Editor
{
    public class SceneGraphEditor : EditorWindow
    {
        SceneGraph.Editor.SceneGraphView _sceneGraphView;

        [MenuItem("Game/Scene Graph")]
        public static void OpenMapConnections()
        {
            SceneGraphEditor wnd = GetWindow<SceneGraphEditor>();
            wnd.titleContent = new GUIContent("Scene Graph");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;       

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Plugins/SceneGraph/Scripts/Editor/SceneGraph.uxml");
            visualTree.CloneTree(root);

            // Import USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/SceneGraph/Scripts/Editor/SceneGraph.uss");
            root.styleSheets.Add(styleSheet);

            // SceneGraphView
            _sceneGraphView = root.Q<SceneGraphView>();
        }
    }
}
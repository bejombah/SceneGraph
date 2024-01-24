using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;

namespace SceneGraph.Editor
{
    public class SceneGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<SceneGraphView, GraphView.UxmlTraits> { }
        [SerializeField] SceneCollection sceneCollection; public SceneCollection SceneCollection => sceneCollection;

        public SceneGraphView()
        {
            // create toolbar
            Toolbar toolbar = new Toolbar();
            this.Add(toolbar);

            // add grid
            Insert(0, new GridBackground());

            // manipulate view
            this.AddManipulator(new ContentDragger());
            var contentZoomer = new ContentZoomer
            {
                minScale = 0.01f,
                maxScale = 2.0f
            };
            this.AddManipulator(contentZoomer);
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Import USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugins/SceneGraph/Scripts/Editor/SceneGraph.uss");
            styleSheets.Add(styleSheet);

            // get scene collection
            sceneCollection = AssetDatabase.LoadAssetAtPath<SceneCollection>("Assets/Plugins/SceneGraph/Resources/SceneCollection.asset");
            if(sceneCollection == null)
            {
                Debug.Log("SceneCollection is null");
                return;
            }
        }
    }
}

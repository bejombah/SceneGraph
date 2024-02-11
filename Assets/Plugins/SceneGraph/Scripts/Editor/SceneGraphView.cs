using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

namespace SceneGraph.Editor
{
    public class SceneGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<SceneGraphView, GraphView.UxmlTraits> { }
        [SerializeField] SceneCollection sceneCollection; public SceneCollection SceneCollection => sceneCollection;
        GraphView graphView;

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach((port) =>
            {
                if (startPort != port && startPort.node != port.node)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        public SceneGraphView()
        {
            // GraphView
            graphView = this;

            // create toolbar
            Toolbar toolbar = new Toolbar();
            this.Add(toolbar);

            // create toolbar initialize
            Action clickAction = null;
            ToolbarButton initializeButton = new ToolbarButton(() => clickAction());
            initializeButton.text = "Initialize";
            clickAction = () => { InitializeGraph(toolbar, initializeButton); };
            
            // scene name field
            TextField sceneNameField = new TextField("Scene Name");

            // define the field's width
            sceneNameField.style.width = 200;
            toolbar.Add(sceneNameField);

            // new scene button
            ToolbarButton newSceneButton = new ToolbarButton(() => { NewScene(sceneNameField.text); })
            {
                text = "New Scene"
            };
            toolbar.Add(newSceneButton);

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

            // get scene collection// get scene collection
            sceneCollection = AssetDatabase.LoadAssetAtPath<SceneCollection>("Assets/Plugins/SceneGraph/Resources/SceneCollection.asset");
            if(sceneCollection == null)
            {
                Debug.Log("SceneCollection is null");
                toolbar.Add(initializeButton);
                return;
            } else 
            {
                // load graph positions
                if(sceneCollection.position != Vector3.zero)
                {
                    this.contentViewContainer.transform.position = sceneCollection.position;
                }

                // load graph zoom
                if(sceneCollection.zoom != Vector3.zero)
                {
                    this.contentViewContainer.transform.scale = sceneCollection.zoom;
                }
            }            

            // populate view
            PopulateView(sceneCollection);
            this.graphViewChanged += OnGraphViewChanged;
            this.viewTransformChanged += OnViewTransformChanged;

            // connections between ports
            PopulateConnections(sceneCollection);
        }

        void PopulateConnections(SceneCollection sceneCollection)
        {
            // get scene collection sub assets
            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Plugins/SceneGraph/Resources/SceneCollection.asset");

            for (int i = 0; i < subAssets.Length; i++)
            {
                if(subAssets [i].GetType() == typeof(SceneInstance))
                {
                    SceneInstance sceneInstance = (SceneInstance)subAssets[i];
                    foreach (PortalData portal in sceneInstance.Portals)
                    {
                        // check if its not null
                        if (portal.GUID == "")
                        {
                            continue;
                        } 
                        else 
                        {
                            // get scene instance port
                            Port sceneInstancePort = GetPort(portal.GUID, true);

                            // get scene to port
                            Port sceneToPort = GetPort(portal.GUID, false);

                            if(sceneInstancePort == null || sceneToPort == null)
                            {
                                continue;
                            }

                            // check if port is connected
                            if(sceneInstancePort.connections.Count() > 0)
                            {
                                continue;
                            }

                            // connect ports
                            UnityEditor.Experimental.GraphView.Edge edge = sceneInstancePort.ConnectTo(sceneToPort);

                            if(edge != null)
                                edge.layer = 1;
                                AddElement(edge);
                        }
                    }
                }
            }
        }

        Port GetPort(string guid, bool isOutput)
        {
            // get all ports
            List<Port> ports = this.ports.ToList();

            // loop through all ports
            foreach (Port port in ports)
            {
                string portguid = port.name;

                // check if port guid is equal to guid
                if (portguid == guid)
                {
                    // check if port is output
                    if (isOutput)
                    {
                        // check if port is output
                        if (port.direction == Direction.Output)
                        {
                            return port;
                        }
                    }
                    else
                    {
                        // check if port is input
                        if (port.direction == Direction.Input)
                        {
                            return port;
                        }
                    }
                }
            }

            return null;
        }

        internal void PopulateView(SceneCollection sceneCollection)
        {
            foreach (SceneInstance sceneInstance in sceneCollection.Scenes)
            {
                CreateNewNode(sceneInstance);
            }
        }

        public void CreateNewNode(SceneInstance sceneInstance, bool isNew=false)
        {
            if(isNew)
            {
                NodeView nodeView = new NodeView(sceneInstance);
                Vector2 center = this.contentViewContainer.WorldToLocal(this.layout.center);
                float width = 1.0f; // Replace with your desired width
                float height = 1.0f; // Replace with your desired height

                Rect centerRect = new Rect(center.x - width / 2.0f, center.y - height / 2.0f, width, height);
                nodeView.SetPosition(centerRect);
                nodeView.GoToScene(this);
                AddElement(nodeView);
            }
            else
            {
                NodeView nodeView = new NodeView(sceneInstance);
                nodeView.SetPosition(sceneInstance.position);
                AddElement(nodeView);
            }            
        }

        public void OnViewTransformChanged(GraphView view)
        {
            // save zoom and positions
            sceneCollection.position = view.contentViewContainer.transform.position;
            sceneCollection.zoom = view.contentViewContainer.transform.scale;
        }

        PortalData FindPortal(string uniqueID)
        {
            foreach (SceneInstance sceneInstance in sceneCollection.Scenes)
            {
                foreach (PortalData portal in sceneInstance.Portals)
                {
                    if (portal.UniqueID == uniqueID)
                    {
                        return portal;
                    }
                }
            } 

            return null;
        }

        SceneInstance FindInstance(string uniqueID)
        {
            foreach (SceneInstance sceneInstance in sceneCollection.Scenes)
            {
                foreach (PortalData portal in sceneInstance.Portals)
                {
                    if (portal.UniqueID == uniqueID)
                    {
                        return sceneInstance;
                    }
                }
            } 

            return null;
        }

        GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            VisualElement toSendBack = null;

            // Check if an edge was added
            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    // guid for portal
                    string guid = Guid.NewGuid().ToString();
                    
                    PortalData portalInput= FindPortal(edge.input.viewDataKey);
                    SceneInstance portalInputInstance = FindInstance(edge.input.viewDataKey); 
                    PortalData portalOutput = FindPortal(edge.output.viewDataKey);
                    SceneInstance portalOutputInstance = FindInstance(edge.output.viewDataKey);

                    // check if portal is null
                    if(portalInput == null || portalOutput == null || portalInputInstance == null || portalOutputInstance == null)
                    {
                        continue;
                    }

                    // set guid on portal
                    portalInput.GUID = guid;
                    portalInput.SceneTo = portalOutputInstance.sceneA;
                    portalOutput.GUID = guid;
                    portalOutput.SceneTo = portalInputInstance.sceneA;

                    // move to 
                    for(int i = 0; i < graphView.contentViewContainer.childCount; i++)
                    {
                        if(graphView.contentViewContainer.Children().ElementAt(i).childCount > 0)
                        {
                            if(graphView.contentViewContainer.Children().ElementAt(i).Children().First().GetClasses().First() == "graphElement")
                            {
                                toSendBack = graphView.contentViewContainer.Children().ElementAt(i);
                            }
                        }
                    }
                }

                toSendBack.SendToBack();
            }

            // Check if an edge was removed
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is UnityEditor.Experimental.GraphView.Edge edge)
                    {
                        PortalData portalInput= FindPortal(edge.input.viewDataKey);
                        PortalData portalOutput = FindPortal(edge.output.viewDataKey);
                      
                        // portalguid nulled
                        portalInput.GUID = "";
                        portalOutput.GUID = "";
                        portalInput.SceneTo = null;
                        portalOutput.SceneTo = null;

                    } else
                    if(element is NodeView nodeView)
                    {
                        // remove portals.guid
                        foreach(PortalData portal in nodeView.SceneInstance.Portals)
                        {
                            portal.GUID = "";
                        }

                        // remove node from scene collection
                        sceneCollection.Scenes.Remove(nodeView.SceneInstance);
                        AssetDatabase.RemoveObjectFromAsset(nodeView.SceneInstance);
                        AssetDatabase.SaveAssets();
                    } 
                }
            }

            return graphViewChange;
        }

        void InitializeGraph(Toolbar toolbar, ToolbarButton initializeButton)
        {
            // remove initialize button
            toolbar.Remove(initializeButton);

            // create scriptable object SceneCollection
            sceneCollection = ScriptableObject.CreateInstance<SceneCollection>();
            AssetDatabase.CreateAsset(sceneCollection, "Assets/Plugins/SceneGraph/Resources/SceneCollection.asset");
            AssetDatabase.SaveAssets();
        }

        void NewScene(string sceneName)
        {
            if(sceneName == "")
            {
                Debug.Log("Scene name is empty");
                return;
            }

            if(sceneCollection == null)
            {
                Debug.Log("SceneCollection is null");
                return;
            } else 
            {
                // check the prefabs
                if(sceneCollection.PortalManagerPrefab == null || sceneCollection.GridPrefab == null || sceneCollection.CamSystemPrefab == null || sceneCollection.CoreScene == null || sceneCollection.PortalPrefab == null)
                {
                    Debug.Log("Check the prefabs!");
                    return;
                }
            }

            // path
            string path = "Assets/Plugins/SceneGraph/Scenes/Maps/";

            // create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // save scene
            EditorSceneManager.SaveScene(scene, path + sceneName + ".unity");

            // refresh asset database
            AssetDatabase.Refresh();

            // create scene asset
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path + sceneName + ".unity");

            // add scene to build settings
            EditorBuildSettingsScene[] buildSettingsScenes = EditorBuildSettings.scenes;
            List<EditorBuildSettingsScene> newBuildSettingsScenes = new List<EditorBuildSettingsScene>(buildSettingsScenes);
            newBuildSettingsScenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true));
            EditorBuildSettings.scenes = newBuildSettingsScenes.ToArray();

            // create scene instance
            SceneInstance sceneInstance = ScriptableObject.CreateInstance<SceneInstance>();
            sceneInstance.name = sceneName;
            sceneInstance.MapName = sceneName;
            sceneInstance.sceneA = sceneAsset;
            sceneCollection.Scenes.Add(sceneInstance);
            AssetDatabase.AddObjectToAsset(sceneInstance, sceneCollection);
            AssetDatabase.SaveAssets();

            // instantiate portal manager prefab
            GameObject portalManager = sceneCollection.PortalManagerPrefab;
            GameObject portalManagerInstance = PrefabUtility.InstantiatePrefab(portalManager) as GameObject;
            portalManagerInstance.name = "PortalManager";
            portalManager.GetComponent<PortalManager>().SceneInstance = sceneInstance;

            // instantiate grid prefab
            GameObject grid = sceneCollection.GridPrefab;
            GameObject gridInstance = PrefabUtility.InstantiatePrefab(grid) as GameObject;
            gridInstance.name = "Grid";

            // instantiate camera system prefab
            GameObject camSystem = sceneCollection.CamSystemPrefab;
            GameObject camSystemInstance = PrefabUtility.InstantiatePrefab(camSystem) as GameObject;
            camSystemInstance.name = "CamSystem";

            // save scene collection
            EditorSceneManager.SaveScene(scene);
            EditorUtility.SetDirty(sceneCollection);
            AssetDatabase.SaveAssets();

            // create node view
            CreateNewNode(sceneInstance, true);
        }
    }
}

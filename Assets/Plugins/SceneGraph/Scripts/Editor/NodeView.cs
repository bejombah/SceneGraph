using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.Tilemaps;
using System.Linq.Expressions;
using System;

namespace SceneGraph.Editor
{
    public class NodeView : Node
    {
        [SerializeField] SceneInstance sceneInstance; public SceneInstance SceneInstance => sceneInstance;
        VisualElement grid;

        // for dragging
        bool isDragging = false;
        UnityEngine.Vector2 mousePosition;
        UnityEngine.Vector2 initialPortPosition;
        UnityEngine.Vector2 offset;

        VisualElement currentPortDragged;
        public NodeView(SceneInstance sceneInstance) : base("Assets/Plugins/SceneGraph/Scripts/Editor/NodeView.uxml")
        {
            // define the data key for the view
            this.sceneInstance = sceneInstance;
            this.viewDataKey = sceneInstance.GUID;

            // go to scene button
            Button goToSceneButton = new Button(() => { GoToScene(); })
            {
                text = "Go"
            };
            this.titleButtonContainer.Add(goToSceneButton);

            // add the scene name to the title
            Button scanSceneButton = new Button(() => { ScanScene(); })
            {
                text = "Scan"
            };
            this.titleButtonContainer.Add(scanSceneButton);

            // add visual element
            grid = new VisualElement();
            this.mainContainer.SendToBack();
            this.mainContainer.Add(grid);
            this.title = sceneInstance.MapName;

            // add image if there's any (load loading)
            if(sceneInstance.preview != null)
            {
                var image =  new StyleBackground(sceneInstance.preview.texture);
                grid.style.backgroundImage = image;

                grid.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.2f);

                // get image size
                var imageWidth = sceneInstance.preview.texture.width;
                var imageHeight = sceneInstance.preview.texture.height;

                grid.style.width = imageWidth;  
                grid.style.height = imageHeight;
            }

            // right click
            grid.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                // get mouse position
                UnityEngine.Vector2 mousePosition = evt.mousePosition;

                // mouse position to visual element position
                mousePosition = grid.WorldToLocal(mousePosition);

                // top left
                if(mousePosition.x < grid.layout.width / 2 && mousePosition.y < grid.layout.height / 2)
                {
                    // move the menu to the bottom
                    evt.menu.InsertAction(0, "Create Input Horizontal", (a) => { PopupNewPort(mousePosition, Direction.Input, Orientation.Horizontal); });
                    evt.menu.InsertAction(1, "Create Input Vertical", (a) => { PopupNewPort(mousePosition, Direction.Input, Orientation.Vertical ); });
                }

                // top right
                if(mousePosition.x > grid.layout.width / 2 && mousePosition.y < grid.layout.height / 2)
                {
                    // move the menu to the bottom
                    evt.menu.InsertAction(0, "Create Output Horizontal", (a) => { PopupNewPort(mousePosition, Direction.Output, Orientation.Horizontal); });
                    evt.menu.InsertAction(1, "Create Input Vertical", (a) => { PopupNewPort(mousePosition, Direction.Input, Orientation.Vertical); });
                }

                // bottom left
                if(mousePosition.x < grid.layout.width / 2 && mousePosition.y > grid.layout.height / 2)
                {
                    // move the menu to the bottom
                    evt.menu.InsertAction(0, "Create Input Horizontal", (a) => { PopupNewPort(mousePosition, Direction.Input, Orientation.Horizontal); });
                    evt.menu.InsertAction(1, "Create Output Vertical", (a) => { PopupNewPort(mousePosition, Direction.Output, Orientation.Vertical); });
                }

                // bottom right
                if(mousePosition.x > grid.layout.width / 2 && mousePosition.y > grid.layout.height / 2)
                {
                    // move the menu to the bottom
                    evt.menu.InsertAction(0, "Create Output Horizontal", (a) => { PopupNewPort(mousePosition, Direction.Output, Orientation.Horizontal); });
                    evt.menu.InsertAction(1, "Create Output Vertical", (a) => { PopupNewPort(mousePosition, Direction.Output, Orientation.Vertical); });
                }
            }));

            // populate view (ports)
            PopulateView();
        }

        void PopulateView()
        {
            // add all the ports
            foreach (PortalData port in sceneInstance.Portals)
            {
                CreatePort(port);
            }
        }
        void CreatePort(PortalData portalData)
        {
            // portal data
            UnityEngine.Vector2 position = portalData.position;
            PortalData _portal = portalData;

            // UI
            Direction direction = portalData.Direction;
            Orientation orientation = portalData.Orientation;

            // create port
            // Port port = Port.Create<Edge>(orientation, direction, Port.Capacity.Single, typeof(PortalData));
            Port port = InstantiatePort(orientation, direction, Port.Capacity.Single, typeof(Edge));
            Label text = new Label(portalData.Name);
            text.style.color = Color.white;

            // setting up the port orientation and direction (depends on the port position in the node view)
            if(orientation == Orientation.Horizontal)
            {
                port.portName = portalData.Name;
                if(direction == Direction.Input)
                {
                    port.style.flexDirection = FlexDirection.Row;
                }
                else
                {
                    port.style.flexDirection = FlexDirection.RowReverse;
                }
            }
            else
            {
                port.Add(text);
                port.RemoveAt(1);
                if(direction == Direction.Input)
                {
                    port.style.flexDirection = FlexDirection.Column;
                    port.style.height = StyleKeyword.Auto;
                    text.style.paddingTop = 6;
                    port.style.paddingTop = 6;
                    port.style.paddingBottom = 6;
                }
                else
                {
                    port.style.flexDirection = FlexDirection.ColumnReverse;
                    port.style.height = StyleKeyword.Auto;
                    text.style.paddingBottom = 6;
                    port.style.paddingTop = 6;
                    port.style.paddingBottom = 6;
                }
            }

            // insert data into port
            port.viewDataKey = portalData.UniqueID;
            port.name = portalData.GUID;
            port.style.position = Position.Absolute;
            port.style.left = position.x;
            port.style.top = position.y;
            port.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);

            port.contentContainer.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                if(evt.menu != null)
                {
                    for (int i = 0; i < evt.menu.MenuItems().Count; i++)
                    {
                        evt.menu.RemoveItemAt(i);
                    }
                }

                evt.menu.AppendAction("Delete Portal", (a) => { DeletePort(port); });
            }));

            // add drag handler
            VisualElement dragHandler = new VisualElement();
            dragHandler.style.position = Position.Relative;
            dragHandler.style.left = 0;
            dragHandler.style.top = 0;
            dragHandler.style.width = orientation == Orientation.Horizontal ? 35 : new Length(100, LengthUnit.Percent);
            dragHandler.style.height = orientation == Orientation.Horizontal ? new Length(100, LengthUnit.Percent) : 35;
            dragHandler.style.backgroundColor = new Color(0.1f, 1f, 0.1f, 1f);
            port.contentContainer.Add(dragHandler);

            grid.Add(port);
            
            dragHandler.RegisterCallback<MouseDownEvent>(evt =>
            {
                if(dragHandler == evt.currentTarget)
                {
                    isDragging = true;
                    currentPortDragged = port;

                    SceneGraphView graphView = this.GetFirstAncestorOfType<SceneGraphView>();
                    mousePosition = graphView.WorldToLocal(evt.mousePosition);

                    // Scale the mouse position by the inverse of the zoom level
                    mousePosition /= graphView.scale;

                    offset = currentPortDragged.layout.position - mousePosition;
                    
                    evt.StopImmediatePropagation();
                }
                else
                {
                    // evt.StopImmediatePropagation();
                }                
            });      

            this.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if(isDragging)
                {
                    SceneGraphView graphView = this.GetFirstAncestorOfType<SceneGraphView>();
                    UnityEngine.Vector2 mousePosition = graphView.WorldToLocal(evt.mousePosition);
                    
                    // Scale the mouse position by the inverse of the zoom level
                    mousePosition /= graphView.scale;

                    UnityEngine.Vector2 newPortPosition = mousePosition + offset;
                    
                    // Update the port position
                    currentPortDragged.style.left = newPortPosition.x;
                    currentPortDragged.style.top = newPortPosition.y;

                    // get the portal
                    foreach(PortalData portalData in sceneInstance.Portals)
                    {
                        if(portalData.UniqueID == currentPortDragged.viewDataKey)
                        {
                            _portal = portalData;
                            break;
                        }
                    }

                    _portal.position = newPortPosition;

                    evt.StopImmediatePropagation();
                }
                else
                {
                    evt.StopImmediatePropagation();
                }
            });

            port.parent.RegisterCallback<MouseUpEvent>(evt =>
            {
                // Rect curr = this.GetPosition();
                isDragging = false;
                currentPortDragged = null;
            });

            dragHandler.RegisterCallback<MouseUpEvent>(evt =>
            {
                // Rect curr = this.GetPosition();
                isDragging = false;
                currentPortDragged = null;
            });
        }

        void DeletePort(Port port)
        {
            // open scene first
            GoToScene();

            // disconnect port
            foreach(Edge edge in port.connections)
            {
                // check if port is input or output
                if(port.direction == Direction.Input)
                {
                    edge.output.Disconnect(edge);
                }
                else
                {
                    edge.input.Disconnect(edge);
                }

                edge.RemoveFromHierarchy();
            }

            // remove port
            grid.Remove(port);

            // modify the portal manager from the scene
            PortalManager portalManager = GameObject.FindObjectOfType<PortalManager>();

            // remove portal from portal manager
            foreach(GameObject go in portalManager.Portals)
            {
                if(go.GetComponent<Portal>().PortalData.UniqueID == port.viewDataKey)
                {
                    portalManager.Portals.Remove(go);
                    UnityEngine.Object.DestroyImmediate(go);

                    // save scene
                    EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(1));

                    break;
                }
            }

            // remove portal
            foreach(PortalData portalData in sceneInstance.Portals)
            {
                if(portalData.position.x == port.style.left.value.value && portalData.position.y == port.style.top.value.value)
                {
                    sceneInstance.Portals.Remove(portalData);

                    // save scene
                    EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(1));
                    
                    break;
                }
            }
        }

        public void GoToScene(SceneGraphView view = null)
        {
            // get scene instance's parent
            SceneGraphView sceneGraphView = this.GetFirstAncestorOfType<SceneGraphView>();

            if(view != null)
            {
                sceneGraphView = view;
            }            
            
            // get core scene asset
            SceneAsset core = sceneGraphView.SceneCollection.CoreScene;

            // check if core scene is active
            if(EditorSceneManager.GetActiveScene().name != core.name)
            {
                // Get the path of the SceneAsset
                string corePath = AssetDatabase.GetAssetPath(core);

                // Load the scene
                EditorSceneManager.OpenScene(corePath);
            }
            
            // check count of scenes loaded
            if(SceneManager.loadedSceneCount > 1)
            {
                // check if scene is already loaded
                if(!EditorSceneManager.GetSceneByName(sceneInstance.sceneA.name).isLoaded)
                {
                    // unload scene below core
                    EditorSceneManager.CloseScene(EditorSceneManager.GetSceneAt(1), true);
                }
            }

            // Get the path of the SceneAsset
            string path = AssetDatabase.GetAssetPath(sceneInstance.sceneA);

            // load another scene additively
            EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }

        void ScanScene()
        {
            // go to scene first
            GoToScene();

            // screenshot
            Screenshot(); 

            // enable player sprite renderer
            sceneInstance.preview = null;

            // add the screenshot to the scene instance
            sceneInstance.preview = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Plugins/SceneGraph/Scenes/Maps/Screenshot/"+ sceneInstance.sceneA.name + ".png");

            // save the scene instance
            EditorUtility.SetDirty(sceneInstance);

            // make the style null
            grid.style.backgroundImage = sceneInstance.preview.texture;

            // update the background size of the NodeView
            grid.style.width = sceneInstance.preview.texture.width;
            grid.style.height = sceneInstance.preview.texture.height;
        }

        void Screenshot()
        {
            // get second scene
            Scene activeScene = EditorSceneManager.GetSceneAt(1);

            // create a temporary cam
            Camera tempCamera = new GameObject("TempCamera").AddComponent<Camera>();
            tempCamera.orthographic = true;
            
            // position the camera to capture the entire scene
            Bounds bounds = CalculateSceneBounds(activeScene);
            tempCamera.transform.position = new UnityEngine.Vector3(bounds.center.x, bounds.center.y, -10);

            // get portal manager
            PortalManager portalManager = GameObject.FindObjectOfType<PortalManager>();

            // temp camera background color
            tempCamera.backgroundColor = new Color(0, 0, 0, 0);

            // define the padding (in world space) for the camera
            float padding = 5f;

            // adjust the bounds to include the padding
            bounds.extents += new UnityEngine.Vector3(padding, padding, 0);

            // save bounds
            sceneInstance.bounds = bounds;

            // calculate the aspect ratio of the bounds
            float aspectRatio = bounds.size.x / bounds.size.y;

            // Calculate the width and height of the RenderTexture
            int width, height;
            if (aspectRatio >= 1f)
            {
                // If the aspect ratio is greater than or equal to 1, set the width to the maximum texture size
                // and calculate the height based on the aspect ratio
                width = 8192;
                height = Mathf.RoundToInt(width / aspectRatio);
            }
            else
            {
                // If the aspect ratio is less than 1, set the height to the maximum texture size
                // and calculate the width based on the aspect ratio
                height = 8192;
                width = Mathf.RoundToInt(height * aspectRatio);
            }

            // Create a new RenderTexture with the calculated width and height
            RenderTexture renderTexture = new RenderTexture(width, height, 24);

            // Set the camera's target texture when rendering
            tempCamera.targetTexture = renderTexture;

            // Set the orthographic size of the camera based on the height of the RenderTexture
            // This ensures that the height of the bounds fits exactly within the camera's view
            if (aspectRatio >= 1f)
            {
                tempCamera.orthographicSize = bounds.size.y / 2;
            }
            else
            {
                tempCamera.orthographicSize = bounds.size.x / (2 * aspectRatio);
            }

            // Render the view of the temporary camera to the RenderTexture
            tempCamera.Render();

            // Create a new Texture2D and read the RenderTexture into it
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            // Convert the Texture2D to a PNG
            byte[] png = texture.EncodeToPNG();

            // create a new folder in the Maps folder
            if(!AssetDatabase.IsValidFolder("Assets/Plugins/SceneGraph/Scenes/Maps/Screenshot"))
            {
                AssetDatabase.CreateFolder("Assets/Plugins/SceneGraph/Scenes/Maps", "Screenshot");
            }

            // Save the PNG to a file
            File.WriteAllBytes("Assets/Plugins/SceneGraph/Scenes/Maps/Screenshot/" + activeScene.name + ".png", png);

            // Clean up
            RenderTexture.active = null; // Set the active RenderTexture to null
            tempCamera.targetTexture = null; // Set the Camera's targetTexture to null
            UnityEngine.Object.DestroyImmediate(tempCamera.gameObject); // Destroy the temporary camera
            UnityEngine.Object.DestroyImmediate(renderTexture); // Finally, destroy the RenderTexture

            // Refresh asset database
            AssetDatabase.Refresh();

            // Remove png from memory
            UnityEngine.Object.DestroyImmediate(texture);
        }

        Bounds CalculateSceneBounds(Scene scene)
        {
            // create a new bounds
            Bounds bounds = new Bounds(UnityEngine.Vector3.zero, UnityEngine.Vector3.zero);

            // get the tilemap
            Tilemap tilemap = UnityEngine.Object.FindObjectOfType<Tilemap>();

            // refresh the tilemap
            tilemap.CompressBounds();

            // get the tilemap bounds
            bounds = tilemap.localBounds;

            return bounds;
        }

        void PopupNewPort(Vector2 position, Direction direction, Orientation orientation)
        {
            // check if there is already a popup window
            if(this.GetFirstAncestorOfType<SceneGraphView>().Children().OfType<UnityEngine.UIElements.PopupWindow>().ToList().Count > 0)
            {
                return;
            }

            UnityEngine.UIElements.PopupWindow popupWindow = new UnityEngine.UIElements.PopupWindow();
            popupWindow.style.position = Position.Absolute;

            // get editor window
            UnityEngine.Rect editorWindow = EditorWindow.GetWindow<SceneGraphEditor>().position;

            // set popup window position
            popupWindow.style.left = editorWindow.width / 2 - 100;
            popupWindow.style.top = editorWindow.height / 2 - 50;

            // add close button
            Button closeButton = new Button(() => { popupWindow.RemoveFromHierarchy(); })
            {
                text = "Close"
            };
            popupWindow.Add(closeButton);

            // add label
            Label labelInteract = new Label("Is Interact?");
            labelInteract.style.color = Color.white;
            popupWindow.Add(labelInteract);

            // add toggle
            Toggle toggleInteract = new Toggle();
            toggleInteract.style.width = 200;
            toggleInteract.style.height = 20;
            popupWindow.Add(toggleInteract);

            // add label
            Label label = new Label();
            label.style.color = Color.white;
            popupWindow.Add(label);

            // add text field
            TextField textField = new TextField();
            textField.style.width = 200;
            textField.style.height = 20;
            textField.focusable = true;
            popupWindow.Add(textField);

            // add button
            Button button = new Button(() =>{
                if(textField.text == "")
                {
                    Debug.Log("Please enter a name");
                    return;
                }
                else
                {
                    // check if this is interact portal
                    AddNewPort(position, direction, orientation, textField.text, popupWindow, toggleInteract.value);
                    // AddNewPort(position, direction, orientation, textField.text, popupWindow);
                }
            })
            {
                text = "Create"
            };
            popupWindow.Add(button);

            // get the graph view
            SceneGraphView sceneGraphView = this.GetFirstAncestorOfType<SceneGraphView>();
            sceneGraphView.Add(popupWindow);

            // focus on the text field
            textField.Focus();
        }

        void AddNewPort(UnityEngine.Vector2 position, Direction direction, Orientation orientation, string name, UnityEngine.UIElements.PopupWindow popupWindow, bool isInteraction = false)
        {
            // GoToScene
            GoToScene();

            // data
            PortalData portalData = new PortalData();
            portalData.position = position;
            portalData.Direction = direction;
            portalData.Orientation = orientation;
            portalData.IsInteraction = isInteraction;
            portalData.Name = name;
            sceneInstance.Portals.Add(portalData);

            // set dirty scene instance
            EditorUtility.SetDirty(sceneInstance);

            // modify the portal manager from the scene
            PortalManager portalManager = GameObject.FindObjectOfType<PortalManager>();

            // get the bounds
            Bounds bounds = sceneInstance.bounds;

            // get center of image
            UnityEngine.Vector3 imgCenter = grid.layout.center;

            // get size of image
            UnityEngine.Vector3 imgSize = grid.layout.size;

            // get the portal position
            UnityEngine.Vector3 imgPortal = position;

            // scene center
            UnityEngine.Vector3 sceneCenter = bounds.center;

            // scene size
            UnityEngine.Vector3 sceneSize = bounds.size;

            // set the portal position relative to the scene center
            // Define the offset
            Vector3 offset = new Vector3(1.5f, -1f, 0);

            // Calculate the position and add the offset
            portalData.PortalPosition = new UnityEngine.Vector3((imgPortal.x - imgCenter.x) * (sceneSize.x / imgSize.x), -((imgPortal.y - imgCenter.y) * (sceneSize.y / imgSize.y)), 0) + sceneCenter + offset;

            // instantiate game object portal (parent is the portal manager)
            // get scene instance's parent
            SceneGraphView sceneGraphView = this.GetFirstAncestorOfType<SceneGraphView>();
            
            // get core scene asset
            GameObject go = PrefabUtility.InstantiatePrefab(sceneGraphView.SceneCollection.PortalPrefab, portalManager.transform) as GameObject;

            // set portal position
            go.transform.position = portalData.PortalPosition;

            // add to the list
            portalManager.Portals.Add(go);

            // set portal name
            go.name = portalData.UniqueID;
            go.GetComponent<Portal>().PortalData = portalData;

            // save scene
            EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(1));

            // close popup window
            popupWindow.RemoveFromHierarchy();
            
            // add port
            CreatePort(portalData);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            // update node position
            sceneInstance.position = newPos;
        }
    }
}

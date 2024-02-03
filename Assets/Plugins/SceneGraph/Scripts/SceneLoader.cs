using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneGraph
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance;
        string currentSceneName;
        SceneCollection sceneCollection;
        float HorizontalOffset = 3f;

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        public async void TransitionToScene(PortalData portal, Vector3 vec)
        {
            // fade out
            FadeOut();

            // adding delay for fade out
            await Task.Delay(200);

            // disable player collider 
            if(Player.Instance != null)
            {
                Player.Instance.GetComponent<BoxCollider2D>().enabled = false;
            }
            else
            {
                Debug.LogError("Controller is null");
            }

            // unload scene
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));

            // get scene instance
            PortalManager portalManager = GameObject.FindObjectOfType<PortalManager>();

            // get scene instance
            SceneInstance currentInstance = portalManager.SceneInstance;
            
            // load new scene
            await LoadSceneSync(currentInstance.Portals.Find(x => x.GUID == portal.GUID).SceneTo.name);

            // adding delay for load scene
            await Task.Delay(200);

            // offset position
            Vector3 offsetPosition = new Vector3(0, 0, 0);
                
            // define portal WIP
            if(portal.Orientation == Orientation.Horizontal)
            {
                // horizontal transition
                if(portal.Direction == Direction.Input)
                {
                    // left transition
                    offsetPosition = new Vector3(-HorizontalOffset, 0, 0);

                    // transition
                    TransitionHorizontal(portal, offsetPosition);
                }
                else
                {
                    // right transition
                    offsetPosition = new Vector3(HorizontalOffset, 0, 0);

                    // transition
                    TransitionHorizontal(portal, offsetPosition);
                }
            }
            else
            {
                // vertical transition
                if(portal.Direction == Direction.Input)
                {
                    // left transition
                    offsetPosition = new Vector3(0, 2.5f, 0);

                    // up transition
                    TransitionVertical(portal, offsetPosition, true, vec);
                }
                else
                {
                    // left transition
                    offsetPosition = new Vector3(0, -2.5f, 0);

                    // down transition
                    TransitionVertical(portal, offsetPosition, false, vec);
                }
            }

            // enable player collider
            if(Player.Instance != null)
            {
                Player.Instance.GetComponent<BoxCollider2D>().enabled = true;
            }
            else
            {
                Debug.LogError("Controller is null");
            }

            // wait until player is grounded
            if(portal.Orientation == Orientation.Horizontal)
            {
                // wait until player is grounded
                while(!Player.Instance.grounded)
                {
                    await Task.Delay(2);
                }

                // adding delay
                await Task.Delay(200);
            } else 
            {
                await Task.Delay(100);
            }

            // FadeIn
            FadeIn();
        }

        void TransitionHorizontal(PortalData portal, Vector3 offsetPosition)
        {
            // get arrival door in new map
            GameObject go = PortalManager.Instance.FindDoor(portal.GUID);

            // get position
            Vector3 pos = go.GetComponent<Portal>().Position + offsetPosition;

            // move player
            Player.Instance.transform.position = pos;
        }

        void TransitionVertical(PortalData portal, Vector3 offsetPosition, bool isUp, Vector3 portalPos)
        {
            // get arrival door in new map
            GameObject go = PortalManager.Instance.FindDoor(portal.GUID);

            float x = portalPos.x - Player.Instance.transform.position.x;

            offsetPosition = new Vector3(-x, offsetPosition.y, offsetPosition.z);

            // get position
            Vector3 pos = go.GetComponent<Portal>().Position + offsetPosition;

            // move player
            Player.Instance.transform.position = pos;

            // if going up
            if(isUp)
            {
                // set velocity
                Player.Instance.velocity = new Vector2(0, 4);
            }
        }

        public async void TransitionInteraction(PortalData portalData)
        {
            // fade out
            FadeOut();
                
            // wait for fade out
            await Task.Delay(200);

            // unload scene
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));

            // get scene instance
            PortalManager doorManager = GameObject.FindObjectOfType<PortalManager>();

            // get scene instance
            SceneInstance currentInstance = doorManager.SceneInstance;
            
            // load new scene
            await LoadSceneSync(currentInstance.Portals.Find(x => x.GUID == portalData.GUID).SceneTo.name);

            // adding delay
            await Task.Delay(200);

            // get arrival door in new map
            GameObject go = PortalManager.Instance.FindDoor(portalData.GUID);

            // get position
            Vector3 pos = go.GetComponent<PortalData>().position;

            // move player
            Player.Instance.transform.position = pos;

            // wait until player is grounded
            while(!Player.Instance.grounded)
            {
                await Task.Delay(2);
            }

            // adding delay
            await Task.Delay(200);

            // FadeIn
            FadeIn();
        }

        Task LoadSceneSync(string sceneName)
        {
            var tcs = new TaskCompletionSource<bool>();
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOperation.completed += _ => tcs.SetResult(true);
            return tcs.Task;
        }

        public void FadeOut()
        {
            LoadingUI.Instance.Hide();
            // change this into event based
        }

        public void FadeIn()
        {
            LoadingUI.Instance.Show();
            // change this into event based
        }
    }
}

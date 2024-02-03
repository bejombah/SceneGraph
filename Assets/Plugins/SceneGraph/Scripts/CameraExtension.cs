using UnityEngine;
using Cinemachine;

namespace SceneGraph
{
    public class CameraExtension : MonoBehaviour
    {
        // this will take cinemachine camera and set the settings via functions
        [SerializeField] CinemachineVirtualCamera _virtualCamera;

        // Set target
        void Start()
        {
            // set follow target
            if(Player.Instance != null)
            {
                _virtualCamera.Follow = Player.Instance.gameObject.transform;
            }
        }
    }
}
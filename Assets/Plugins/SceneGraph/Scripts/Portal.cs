using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SceneGraph
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] PortalData portalData; public PortalData PortalData { get => portalData; set => portalData = value; }
        [SerializeField] Vector3 position; public Vector3 Position { get => position; set => position = value; }
        Vector3 changedPosition => this.transform.position;

        [SerializeField] UnityEvent transitionEvenet = new UnityEvent();
        [SerializeField] BoxCollider2D boxCollider2D; public BoxCollider2D BoxCollider2D { get => boxCollider2D; set => boxCollider2D = value; }
        [SerializeField] TMP_Text textInteraction; public TMP_Text TextInteraction { get => textInteraction; set => textInteraction = value; }

        void Start()
        {
            boxCollider2D.enabled = true;
            if(portalData.IsInteraction)
            {
                textInteraction.enabled = false;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if(other.CompareTag("Player"))
            {
                // check if this interaction portal
                if(!portalData.IsInteraction)
                {
                    // SceneLoader.Instance.TransitionToScene(door, this.transform.position);
                }
                else
                {
                    textInteraction.enabled = true;
                }
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if(other.gameObject.tag == "Player")
            {
                // check if this interaction door
                if(portalData.IsInteraction)
                {
                    if(Input.GetKeyDown(KeyCode.E))
                    {
                        // SceneLoader.Instance.TransitionInteraction(door);
                    }
                }
            }    
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if(other.CompareTag("Player"))
            {
                if(portalData.IsInteraction)
                {
                    textInteraction.enabled = false;
                }
            }    
        }

        void Update()
        {
            #if UNITY_EDITOR
            if (changedPosition != position)
            {
                position = changedPosition;
            }
            #endif
        }

        void OnDrawGizmos()
        {
            // Draw the label at the GameObject's position
            Handles.Label(transform.position, portalData.Name);
        }
    }
}

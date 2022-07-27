using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using System;

namespace DracarysInteractive.Networked
{
    [RequireComponent(typeof(XRBaseInteractable))]
    [RequireComponent(typeof(PhotonView))]

    public class NetworkedInteractable : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
    {
        private XRBaseInteractable interactableBase;
        private Rigidbody rigidBody;

        public bool debug = false;
        private bool isBeingHeld;

        public bool useGravity = true;

        public LayerMask noninteractableStateLayer;
        public LayerMask interactableStateLayer;

        public UnityEvent activateEvent;
        public UnityEvent deactivateEvent;
        public UnityEvent selectEvent;
        public UnityEvent deselectEvent;

        #region Unity Entry Points
        void Awake()
        {
            Log("NetworkedInteractable: enter Awake");

            interactableBase = GetComponent<XRBaseInteractable>();
            rigidBody = GetComponent<Rigidbody>();

            interactableBase.selectEntered.AddListener(OnSelectEnter);
            interactableBase.selectExited.AddListener(OnSelectExit);
            interactableBase.activated.AddListener(OnActivate);
            interactableBase.deactivated.AddListener(OnDeactivate);
        }

        void Update()
        {
            if (isBeingHeld)
            {
                if (rigidBody)
                    rigidBody.isKinematic = true;

                gameObject.layer = noninteractableStateLayer.ToSingleLayer();
            }
            else
            {
                if (rigidBody && useGravity)
                    rigidBody.isKinematic = false;

                gameObject.layer = interactableStateLayer.ToSingleLayer();
            }
        }

        #endregion

        #region XRBaseInteractable events

        private void OnDeactivate(DeactivateEventArgs arg0)
        {
            Log("NetworkedInteractable: enter OnDeactivate");

            if (photonView && PhotonNetwork.InRoom)
                photonView.RPC("RpcOnDeactivate", RpcTarget.AllBuffered);
            else
                RpcOnDeactivate();
        }

        private void OnActivate(ActivateEventArgs arg0)
        {
            Log("NetworkedInteractable: enter OnActivate");

            if (photonView && PhotonNetwork.InRoom)
                photonView.RPC("RpcOnActivate", RpcTarget.AllBuffered);
            else
                RpcOnActivate();
        }

        private void OnSelectExit(SelectExitEventArgs arg0)
        {
            Log("NetworkedInteractable: enter OnSelectExit");

            if (photonView && PhotonNetwork.InRoom)
                photonView.RPC("RpcOnDeselect", RpcTarget.AllBuffered);
            else
                RpcOnDeselect();
        }

        private void OnSelectEnter(SelectEnterEventArgs arg0)
        {
            Log("NetworkedInteractable: enter OnSelectEnter");

            if (photonView && PhotonNetwork.InRoom)
            {
                photonView.RPC("RpcOnSelect", RpcTarget.AllBuffered);

                if (photonView.Owner != PhotonNetwork.LocalPlayer)
                    photonView.RequestOwnership();
            }
            else
                RpcOnSelect();
        }

        #endregion XRBaseInteractable events

        #region PUN Remote Procedure Calls

        [PunRPC]
        public void RpcOnSelect()
        {
            Log("NetworkedInteractable: enter RpcOnSelect");
            isBeingHeld = true;
            selectEvent.Invoke();
        }

        [PunRPC]
        public void RpcOnDeselect()
        {
            Log("NetworkedInteractable: enter RpcOnDeselect");
            isBeingHeld = false;
            deselectEvent.Invoke();
        }

        [PunRPC]
        public void RpcOnActivate()
        {
            Log("NetworkedInteractable: enter RpcOnActivate");
            activateEvent.Invoke();
        }

        [PunRPC]
        public void RpcOnDeactivate()
        {
            Log("NetworkedInteractable: enter RpcOnDeactivate");
            deactivateEvent.Invoke();
        }

        #endregion  PUN Remote Procedure Calls

        #region PUN Ownership Callbacks

        void IPunOwnershipCallbacks.OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
        {
            Debug.LogError("NetworkedInteractable: OnOwnershipTransferFailed!!!");
        }

        void IPunOwnershipCallbacks.OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
        {
            Log("NetworkedInteractable: enter OnOwnershipRequest");

            if (targetView == photonView)
            {
                Log("Ownership Requested for: " + targetView.name + " from " + requestingPlayer.NickName);
                photonView.TransferOwnership(requestingPlayer);
            }
        }

        void IPunOwnershipCallbacks.OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
        {
            Log("NetworkedInteractable: enter OnOwnershipTransfered");
            Log("Ownership Transfered. New Owner: " + targetView.Owner.NickName);
        }

        #endregion PUN Ownership Callbacks

        #region Private Utility Methods
        void Log(string msg)
        {
            if (debug)
                Debug.Log(msg);
        }
        #endregion
    }

    public static class LayerMaskExtensions
    {
        public static int ToSingleLayer(this LayerMask mask)
        {
            int value = mask.value;
            int layer = 0;

            for (layer = 1; layer < 32; layer++)
                if ((value & (1 << layer)) != 0)
                    return layer;

            return layer;
        }
    }
}

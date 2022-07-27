using Oculus.Avatar2;
using Oculus.Platform;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DracarysInteractive.Networked
{
    public class NetworkedAvatarManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] GameObject origin;
        [SerializeField] Transform[] spawnPoints;

        [SerializeField] GameObject avatarPrefab;
        [SerializeField] GameObject voicePrefab;
        [SerializeField] GameObject dynamicCubeSpawner;

        [SerializeField] UnityEvent readyToSpawn;

        [HideInInspector] public ulong userID = 0;

        bool userIsEntitled = false;

        private void Awake()
        {
            try
            {
                Core.AsyncInitialize();
                Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
            }
            catch (UnityException e)
            {
                Debug.LogError("Platform failed to initialize due to exception.");
                Debug.LogException(e);
            }
        }

        void EntitlementCallback(Message msg)
        {
            if (msg.IsError)
            {
                Debug.LogError("You are NOT entitled to use this app.");
                UnityEngine.Application.Quit();
            }
            else
            {
                Debug.Log("You are entitled to use this app.");
                GetTokens();
            }
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom: PhotonNetwork.LocalPlayer.ActorNumber=" + PhotonNetwork.LocalPlayer.ActorNumber);

            if (PhotonNetwork.LocalPlayer.ActorNumber <= spawnPoints.Length)
            {
                origin.transform.position = spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].transform.position;
                origin.transform.rotation = spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].transform.rotation;
            }
        }

        private void GetTokens()
        {
            Users.GetAccessToken().OnComplete(message =>
            {
                if (!message.IsError)
                {
                    OvrAvatarEntitlement.SetAccessToken(message.Data);

                    Users.GetLoggedInUser().OnComplete(message =>
                    {
                        if (!message.IsError)
                        {
                            userID = message.Data.ID;
                            userIsEntitled = true;
                            StartCoroutine(WaitToSpawn());
                        }
                        else
                            Debug.LogError(message.GetError());
                    });
                }
                else
                    Debug.LogError(message.GetError());
            });
        }

        IEnumerator WaitToSpawn()
        {
            while (!userIsEntitled || !OvrAvatarEntitlement.AccessTokenIsValid || PhotonNetwork.CurrentRoom == null)
                yield return null;

            readyToSpawn.Invoke();
        }

        public void OnReadyToSpawn()
        {
            GameObject avatarEntity = PhotonNetwork.Instantiate(avatarPrefab.name, origin.transform.position, origin.transform.rotation);

            avatarEntity.transform.SetParent(origin.transform);
            avatarEntity.transform.localPosition = Vector3.zero;
            avatarEntity.transform.localRotation = Quaternion.identity;

            avatarEntity.GetComponent<PhotonView>().RPC("RPC_SetOculusID", RpcTarget.AllBuffered, (long)userID);

            GameObject voiceSetup = PhotonNetwork.Instantiate(voicePrefab.name, origin.transform.position, origin.transform.rotation);

            voiceSetup.transform.SetParent(origin.transform);
            voiceSetup.transform.localPosition = Vector3.zero;
            voiceSetup.transform.localRotation = Quaternion.identity;

            avatarEntity.GetComponent<NetworkedAvatarEntity>().SetLipSync(voiceSetup.GetComponent<OvrAvatarLipSyncContext>());
            voiceSetup.GetComponent<OvrAvatarLipSyncContext>().CaptureAudio = true;

            OnJoinedRoom();
        }
    }
}

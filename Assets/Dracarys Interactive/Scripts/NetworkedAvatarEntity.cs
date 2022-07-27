using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Avatar2;
using Photon.Pun;
using System.Threading.Tasks;

namespace DracarysInteractive.Networked
{
    public class NetworkedAvatarEntity : OvrAvatarEntity
    {
        [SerializeField] StreamLOD streamLOD = StreamLOD.Medium;
        [SerializeField] float sendInterval = 0.05f;

        private PhotonView photonView;
        private List<byte[]> streamedData = new List<byte[]>();
        private int maxStreamedDataCount = 5;
        private float sendIntervalStartTime = 0;
        private bool skeletonLoaded = false;
        private bool userIDSet;

        protected override void Awake()
        {
            photonView = GetComponent<PhotonView>();
            ConfigureAvatar();
            base.Awake();

            if (photonView.IsMine)
                SetActiveView(CAPI.ovrAvatar2EntityViewFlags.FirstPerson);
            else
                SetActiveView(CAPI.ovrAvatar2EntityViewFlags.ThirdPerson);
            
            StartCoroutine(TryToLoadUser());
        }

        void ConfigureAvatar()
        {
            if (photonView.IsMine)
            {
                SetIsLocal(true);
                _creationInfo.features = CAPI.ovrAvatar2EntityFeatures.Preset_Default;

                SampleInputManager sampleInputManager = OvrAvatarManager.Instance.gameObject.GetComponent<SampleInputManager>();
                SetBodyTracking(sampleInputManager);

                OvrAvatarLipSyncContext lipSyncInput = FindObjectOfType<OvrAvatarLipSyncContext>();
                SetLipSync(lipSyncInput);

                gameObject.name = "LocalAvatar";
            }
            else
            {
                SetIsLocal(false);
                _creationInfo.features = CAPI.ovrAvatar2EntityFeatures.Preset_Remote;

                gameObject.name = "RemoteAvatar";
            }
        }

        IEnumerator TryToLoadUser()
        {
            while (!userIDSet || !OvrAvatarEntitlement.AccessTokenIsValid)
                yield return null;

            Task hasAvatarRequest = OvrAvatarManager.Instance.UserHasAvatarAsync(_userId);

            while (!hasAvatarRequest.IsCompleted)
                yield return null;

            LoadUser();
        }

        protected override void OnSkeletonLoaded()
        {
            base.OnSkeletonLoaded();
            skeletonLoaded = true;
        }

        private void LateUpdate()
        {
            if (!skeletonLoaded) return;

            float elapsedTime = Time.time - sendIntervalStartTime;

            if (elapsedTime > sendInterval)
            {
                if (IsLocal)
                {
                    byte[] bytes = RecordStreamData(streamLOD);
                    photonView.RPC("RPC_RecieveStreamData", RpcTarget.Others, bytes);
                }

                sendIntervalStartTime = Time.time;
            }
        }

        [PunRPC]
        public void RPC_RecieveStreamData(byte[] bytes)
        {
            if (streamedData.Count == maxStreamedDataCount)
            {
                Debug.LogWarning("RPC_RecieveStreamData: exceeded maxStreamedDataCount!");
                streamedData.RemoveAt(streamedData.Count - 1);
            }

            streamedData.Add(bytes);
        }

        [PunRPC]
        public void RPC_SetOculusID(long id)
        {
            _userId = (ulong)id;
            userIDSet = true;
        }

        private void Update()
        {
            if (!skeletonLoaded) return;

            if (streamedData.Count > 0)
            {
                if (!IsLocal)
                {
                    byte[] streamedDataBytes = streamedData[0];

                    if (streamedDataBytes != null)
                    {
                        ApplyStreamData(streamedDataBytes);
                        SetPlaybackTimeDelay(sendInterval / 2);
                    }

                    streamedData.RemoveAt(0);
                }
            }
        }
    }
}
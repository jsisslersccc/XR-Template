using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Avatar2;
using Photon.Pun;
using System.Threading.Tasks;

namespace DracarysInteractive.XR
{
    public class AvatarEntity : SampleAvatarEntity
    {
        /*
        public CAPI.ovrAvatar2EntityViewFlags view;
        private bool skeletonLoaded = false;
        private bool userIDSet;

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(TryToLoadUser());
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
        */
    }
}
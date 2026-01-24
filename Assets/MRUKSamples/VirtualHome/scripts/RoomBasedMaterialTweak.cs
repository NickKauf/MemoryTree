// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Threading.Tasks;
using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.VirtualHome
{
    [MetaCodeSample("MRUKSample-VirtualHome")]
    public class RoomBasedMaterialTweak : MonoBehaviour
    {
        private static readonly int DistanceCovered = Shader.PropertyToID("_DistanceCovered");

        private void OnEnable()
        {
            if (MRUK.Instance != null)
            {
                MRUK.Instance.SceneLoadedEvent.AddListener(StartTweakingCoroutine);
            }
        }

        private void OnDisable()
        {
            if (MRUK.Instance != null)
            {
                MRUK.Instance.SceneLoadedEvent.RemoveListener(StartTweakingCoroutine);
            }
        }

        public async void StartTweakingCoroutine()
        {
            try
            {
                await TweakDistanceBasedGradient();
            }
            catch (Exception e)
            {
                Debug.LogError("Something went wrong trying to tweak distance based gradient: " + e.Message);
            }
        }

        private Task TweakDistanceBasedGradient()
        {
            var roomBounds = MRUK.Instance.GetCurrentRoom().GetRoomBounds();
            var roomSize = Mathf.Max(roomBounds.size.x, roomBounds.size.z);
            var roomSizeVec = new Vector2(0, roomSize);
            var AllMeshes = FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            float t = 0;
            while (t < 1)
            {
                foreach (var mr in AllMeshes)
                {
                    if (mr.material.HasProperty(DistanceCovered))
                    {
                        mr.material.SetVector(DistanceCovered,
                            Vector2.Lerp(mr.material.GetVector(DistanceCovered), roomSizeVec, t));
                    }
                }

                t += Time.deltaTime;
            }

            return Task.CompletedTask;
        }
    }
}

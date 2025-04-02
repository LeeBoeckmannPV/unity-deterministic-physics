using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityS.Mathematics;

/* **************
   COPY AND PASTE
   **************
 * PostRotationEuler.cs and RotationEuler.cs are copy-and-paste.
 * Any changes to one must be copied to the other.
 * The only differences are:
 *   s/PostRotation/Rotation/g
*/

namespace UnityS.Transforms
{
    [Serializable]
    [WriteGroup(typeof(PostRotation))]
    public struct PostRotationEulerXYZ : IComponentData
    {
        public float3 Value;
    }

    [Serializable]
    [WriteGroup(typeof(PostRotation))]
    public struct PostRotationEulerXZY : IComponentData
    {
        public float3 Value;
    }

    [Serializable]
    [WriteGroup(typeof(PostRotation))]
    public struct PostRotationEulerYXZ : IComponentData
    {
        public float3 Value;
    }

    [Serializable]
    [WriteGroup(typeof(PostRotation))]
    public struct PostRotationEulerYZX : IComponentData
    {
        public float3 Value;
    }

    [Serializable]
    [WriteGroup(typeof(PostRotation))]
    public struct PostRotationEulerZXY : IComponentData
    {
        public float3 Value;
    }

    [Serializable]
    [WriteGroup(typeof(PostRotation))]
    public struct PostRotationEulerZYX : IComponentData
    {
        public float3 Value;
    }

    // PostRotation = PostRotationEulerXYZ
    // (or) PostRotation = PostRotationEulerXZY
    // (or) PostRotation = PostRotationEulerYXZ
    // (or) PostRotation = PostRotationEulerYZX
    // (or) PostRotation = PostRotationEulerZXY
    // (or) PostRotation = PostRotationEulerZYX
    public abstract partial class PostRotationEulerSystem : SystemBase
    {
        private EntityQuery m_Group;

        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PostRotation)
                },
                Any = new ComponentType[]
                {
                    ComponentType.ReadOnly<PostRotationEulerXYZ>(),
                    ComponentType.ReadOnly<PostRotationEulerXZY>(),
                    ComponentType.ReadOnly<PostRotationEulerYXZ>(),
                    ComponentType.ReadOnly<PostRotationEulerYZX>(),
                    ComponentType.ReadOnly<PostRotationEulerZXY>(),
                    ComponentType.ReadOnly<PostRotationEulerZYX>()
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });
        }

        [BurstCompile]
        struct PostRotationEulerToPostRotation : IJobChunk
        {
            public ComponentTypeHandle<PostRotation> PostRotationTypeHandle;
            [ReadOnly] public ComponentTypeHandle<PostRotationEulerXYZ> PostRotationEulerXyzTypeHandle;
            [ReadOnly] public ComponentTypeHandle<PostRotationEulerXZY> PostRotationEulerXzyTypeHandle;
            [ReadOnly] public ComponentTypeHandle<PostRotationEulerYXZ> PostRotationEulerYxzTypeHandle;
            [ReadOnly] public ComponentTypeHandle<PostRotationEulerYZX> PostRotationEulerYzxTypeHandle;
            [ReadOnly] public ComponentTypeHandle<PostRotationEulerZXY> PostRotationEulerZxyTypeHandle;
            [ReadOnly] public ComponentTypeHandle<PostRotationEulerZYX> PostRotationEulerZyxTypeHandle;
            public uint LastSystemVersion;

			public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
			{
                if (chunk.Has(ref PostRotationEulerXyzTypeHandle))
                {
                    if (!chunk.DidChange(ref PostRotationEulerXyzTypeHandle, LastSystemVersion))
                        return;

                    var chunkRotations = chunk.GetNativeArray(ref PostRotationTypeHandle);
                    var chunkPostRotationEulerXYZs = chunk.GetNativeArray(ref PostRotationEulerXyzTypeHandle);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        chunkRotations[i] = new PostRotation
                        {
                            Value = quaternion.EulerXYZ(chunkPostRotationEulerXYZs[i].Value)
                        };
                    }
                }
                else if (chunk.Has(ref PostRotationEulerXzyTypeHandle))
                {
                    if (!chunk.DidChange(ref PostRotationEulerXzyTypeHandle, LastSystemVersion))
                        return;

                    var chunkRotations = chunk.GetNativeArray(ref PostRotationTypeHandle);
                    var chunkPostRotationEulerXZYs = chunk.GetNativeArray(ref PostRotationEulerXzyTypeHandle);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        chunkRotations[i] = new PostRotation
                        {
                            Value = quaternion.EulerXZY(chunkPostRotationEulerXZYs[i].Value)
                        };
                    }
                }
                else if (chunk.Has(ref PostRotationEulerYxzTypeHandle))
                {
                    if (!chunk.DidChange(ref PostRotationEulerYxzTypeHandle, LastSystemVersion))
                        return;

                    var chunkRotations = chunk.GetNativeArray(ref PostRotationTypeHandle);
                    var chunkPostRotationEulerYXZs = chunk.GetNativeArray(ref PostRotationEulerYxzTypeHandle);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        chunkRotations[i] = new PostRotation
                        {
                            Value = quaternion.EulerYXZ(chunkPostRotationEulerYXZs[i].Value)
                        };
                    }
                }
                else if (chunk.Has(ref PostRotationEulerYzxTypeHandle))
                {
                    if (!chunk.DidChange(ref PostRotationEulerYzxTypeHandle, LastSystemVersion))
                        return;

                    var chunkRotations = chunk.GetNativeArray(ref PostRotationTypeHandle);
                    var chunkPostRotationEulerYZXs = chunk.GetNativeArray(ref PostRotationEulerYzxTypeHandle);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        chunkRotations[i] = new PostRotation
                        {
                            Value = quaternion.EulerYZX(chunkPostRotationEulerYZXs[i].Value)
                        };
                    }
                }
                else if (chunk.Has(ref PostRotationEulerZxyTypeHandle))
                {
                    if (!chunk.DidChange(ref PostRotationEulerZxyTypeHandle, LastSystemVersion))
                        return;

                    var chunkRotations = chunk.GetNativeArray(ref PostRotationTypeHandle);
                    var chunkPostRotationEulerZXYs = chunk.GetNativeArray(ref PostRotationEulerZxyTypeHandle);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        chunkRotations[i] = new PostRotation
                        {
                            Value = quaternion.EulerZXY(chunkPostRotationEulerZXYs[i].Value)
                        };
                    }
                }
                else if (chunk.Has(ref PostRotationEulerZyxTypeHandle))
                {
                    if (!chunk.DidChange(ref PostRotationEulerZyxTypeHandle, LastSystemVersion))
                        return;

                    var chunkRotations = chunk.GetNativeArray(ref PostRotationTypeHandle);
                    var chunkPostRotationEulerZYXs = chunk.GetNativeArray(ref PostRotationEulerZyxTypeHandle);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        chunkRotations[i] = new PostRotation
                        {
                            Value = quaternion.EulerZYX(chunkPostRotationEulerZYXs[i].Value)
                        };
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            var job = new PostRotationEulerToPostRotation()
            {
                PostRotationTypeHandle = GetComponentTypeHandle<PostRotation>(false),
                PostRotationEulerXyzTypeHandle = GetComponentTypeHandle<PostRotationEulerXYZ>(true),
                PostRotationEulerXzyTypeHandle = GetComponentTypeHandle<PostRotationEulerXZY>(true),
                PostRotationEulerYxzTypeHandle = GetComponentTypeHandle<PostRotationEulerYXZ>(true),
                PostRotationEulerYzxTypeHandle = GetComponentTypeHandle<PostRotationEulerYZX>(true),
                PostRotationEulerZxyTypeHandle = GetComponentTypeHandle<PostRotationEulerZXY>(true),
                PostRotationEulerZyxTypeHandle = GetComponentTypeHandle<PostRotationEulerZYX>(true),
                LastSystemVersion = LastSystemVersion
            }.ScheduleParallel(m_Group, Dependency);
        }
    }
}

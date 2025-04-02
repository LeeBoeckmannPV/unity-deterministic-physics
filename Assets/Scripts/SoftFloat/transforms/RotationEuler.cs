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
    [WriteGroup(typeof(Rotation))]
    public struct RotationEulerXYZ : IComponentData
    {
        public float3 Value;
    }

    [Serializable]
    [WriteGroup(typeof(Rotation))]
    public struct RotationEulerXZY : IComponentData
    {
        public float3 Value;
    }

    [Serializable]
    [WriteGroup(typeof(Rotation))]
    public struct RotationEulerYXZ : IComponentData
    {
        public float3 Value;
    }

    [Serializable]
    [WriteGroup(typeof(Rotation))]
    public struct RotationEulerYZX : IComponentData
    {
        public float3 Value;
    }

    [Serializable]
    [WriteGroup(typeof(Rotation))]
    public struct RotationEulerZXY : IComponentData
    {
        public float3 Value;
    }

    [Serializable]
    [WriteGroup(typeof(Rotation))]
    public struct RotationEulerZYX : IComponentData
    {
        public float3 Value;
    }

    // Rotation = RotationEulerXYZ
    // (or) Rotation = RotationEulerXZY
    // (or) Rotation = RotationEulerYXZ
    // (or) Rotation = RotationEulerYZX
    // (or) Rotation = RotationEulerZXY
    // (or) Rotation = RotationEulerZYX
    public abstract partial class RotationEulerSystem : SystemBase
    {
        private EntityQuery m_Group;

        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(Rotation)
                },
                Any = new ComponentType[]
                {
                    ComponentType.ReadOnly<RotationEulerXYZ>(),
                    ComponentType.ReadOnly<RotationEulerXZY>(),
                    ComponentType.ReadOnly<RotationEulerYXZ>(),
                    ComponentType.ReadOnly<RotationEulerYZX>(),
                    ComponentType.ReadOnly<RotationEulerZXY>(),
                    ComponentType.ReadOnly<RotationEulerZYX>()
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });
        }

        [BurstCompile]
        struct RotationEulerToRotation : IJobChunk
        {
            public ComponentTypeHandle<Rotation> RotationTypeHandle;
            [ReadOnly] public ComponentTypeHandle<RotationEulerXYZ> RotationEulerXyzTypeHandle;
            [ReadOnly] public ComponentTypeHandle<RotationEulerXZY> RotationEulerXzyTypeHandle;
            [ReadOnly] public ComponentTypeHandle<RotationEulerYXZ> RotationEulerYxzTypeHandle;
            [ReadOnly] public ComponentTypeHandle<RotationEulerYZX> RotationEulerYzxTypeHandle;
            [ReadOnly] public ComponentTypeHandle<RotationEulerZXY> RotationEulerZxyTypeHandle;
            [ReadOnly] public ComponentTypeHandle<RotationEulerZYX> RotationEulerZyxTypeHandle;
            public uint LastSystemVersion;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                if (chunk.Has(ref RotationEulerXyzTypeHandle))
                {
                    if (!chunk.DidChange(ref RotationEulerXyzTypeHandle, LastSystemVersion))
                        return;

                    var chunkRotations = chunk.GetNativeArray(ref RotationTypeHandle);
                    var chunkRotationEulerXYZs = chunk.GetNativeArray(ref RotationEulerXyzTypeHandle);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        chunkRotations[i] = new Rotation
                        {
                            Value = quaternion.EulerXYZ(chunkRotationEulerXYZs[i].Value)
                        };
                    }
                }
                else if (chunk.Has(ref RotationEulerXzyTypeHandle))
                {
                    if (!chunk.DidChange(ref RotationEulerXzyTypeHandle, LastSystemVersion))
                        return;

                    var chunkRotations = chunk.GetNativeArray(ref RotationTypeHandle);
                    var chunkRotationEulerXZYs = chunk.GetNativeArray(ref RotationEulerXzyTypeHandle);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        chunkRotations[i] = new Rotation
                        {
                            Value = quaternion.EulerXZY(chunkRotationEulerXZYs[i].Value)
                        };
                    }
                }
                else if (chunk.Has(ref RotationEulerYxzTypeHandle))
                {
                    if (!chunk.DidChange(ref RotationEulerYxzTypeHandle, LastSystemVersion))
                        return;

                    var chunkRotations = chunk.GetNativeArray(ref RotationTypeHandle);
                    var chunkRotationEulerYXZs = chunk.GetNativeArray(ref RotationEulerYxzTypeHandle);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        chunkRotations[i] = new Rotation
                        {
                            Value = quaternion.EulerYXZ(chunkRotationEulerYXZs[i].Value)
                        };
                    }
                }
                else if (chunk.Has(ref RotationEulerYzxTypeHandle))
                {
                    if (!chunk.DidChange(ref RotationEulerYzxTypeHandle, LastSystemVersion))
                        return;

                    var chunkRotations = chunk.GetNativeArray(ref RotationTypeHandle);
                    var chunkRotationEulerYZXs = chunk.GetNativeArray(ref RotationEulerYzxTypeHandle);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        chunkRotations[i] = new Rotation
                        {
                            Value = quaternion.EulerYZX(chunkRotationEulerYZXs[i].Value)
                        };
                    }
                }
                else if (chunk.Has(ref RotationEulerZxyTypeHandle))
                {
                    if (!chunk.DidChange(ref RotationEulerZxyTypeHandle, LastSystemVersion))
                        return;

                    var chunkRotations = chunk.GetNativeArray(ref RotationTypeHandle);
                    var chunkRotationEulerZXYs = chunk.GetNativeArray(ref RotationEulerZxyTypeHandle);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        chunkRotations[i] = new Rotation
                        {
                            Value = quaternion.EulerZXY(chunkRotationEulerZXYs[i].Value)
                        };
                    }
                }
                else if (chunk.Has(ref RotationEulerZyxTypeHandle))
                {
                    if (!chunk.DidChange(ref RotationEulerZyxTypeHandle, LastSystemVersion))
                        return;

                    var chunkRotations = chunk.GetNativeArray(ref RotationTypeHandle);
                    var chunkRotationEulerZYXs = chunk.GetNativeArray(ref RotationEulerZyxTypeHandle);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        chunkRotations[i] = new Rotation
                        {
                            Value = quaternion.EulerZYX(chunkRotationEulerZYXs[i].Value)
                        };
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            var job = new RotationEulerToRotation()
            {
                RotationTypeHandle = GetComponentTypeHandle<Rotation>(false),
                RotationEulerXyzTypeHandle = GetComponentTypeHandle<RotationEulerXYZ>(true),
                RotationEulerXzyTypeHandle = GetComponentTypeHandle<RotationEulerXZY>(true),
                RotationEulerYxzTypeHandle = GetComponentTypeHandle<RotationEulerYXZ>(true),
                RotationEulerYzxTypeHandle = GetComponentTypeHandle<RotationEulerYZX>(true),
                RotationEulerZxyTypeHandle = GetComponentTypeHandle<RotationEulerZXY>(true),
                RotationEulerZyxTypeHandle = GetComponentTypeHandle<RotationEulerZYX>(true),
                LastSystemVersion = LastSystemVersion
            }.ScheduleParallel(m_Group, Dependency);
        }
    }
}

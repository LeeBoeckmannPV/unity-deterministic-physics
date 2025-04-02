using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityS.Mathematics;

/* **************
   COPY AND PASTE
   **************
 * TRSLocalToWorldSystem and TRSLocalToParentSystem are copy-and-paste.
 * Any changes to one must be copied to the other.
 * The only differences are:
 *   - s/LocalToWorld/LocalToParent/g
 *   - Add variation for ParentScaleInverse
*/

namespace UnityS.Transforms
{
    // LocalToParent = Translation * Rotation * NonUniformScale
    // (or) LocalToParent = Translation * CompositeRotation * NonUniformScale
    // (or) LocalToParent = Translation * Rotation * Scale
    // (or) LocalToParent = Translation * CompositeRotation * Scale
    // (or) LocalToParent = Translation * Rotation * CompositeScale
    // (or) LocalToParent = Translation * CompositeRotation * CompositeScale
    // (or) LocalToParent = Translation * ParentScaleInverse * Rotation * NonUniformScale
    // (or) LocalToParent = Translation * ParentScaleInverse * CompositeRotation * NonUniformScale
    // (or) LocalToParent = Translation * ParentScaleInverse * Rotation * Scale
    // (or) LocalToParent = Translation * ParentScaleInverse * CompositeRotation * Scale
    // (or) LocalToParent = Translation * ParentScaleInverse * Rotation * CompositeScale
    // (or) LocalToParent = Translation * ParentScaleInverse * CompositeRotation * CompositeScale

    public abstract partial class TRSToLocalToParentSystem : SystemBase
    {
        private EntityQuery m_Group;

        [BurstCompile]
        struct TRSToLocalToParent : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<Rotation> RotationTypeHandle;
            [ReadOnly] public ComponentTypeHandle<CompositeRotation> CompositeRotationTypeHandle;
            [ReadOnly] public ComponentTypeHandle<Translation> TranslationTypeHandle;
            [ReadOnly] public ComponentTypeHandle<NonUniformScale> NonUniformScaleTypeHandle;
            [ReadOnly] public ComponentTypeHandle<Scale> ScaleTypeHandle;
            [ReadOnly] public ComponentTypeHandle<CompositeScale> CompositeScaleTypeHandle;
            [ReadOnly] public ComponentTypeHandle<ParentScaleInverse> ParentScaleInverseTypeHandle;
            public ComponentTypeHandle<LocalToParent> LocalToParentTypeHandle;
            public uint LastSystemVersion;

			public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
			{
                bool changed =
                    chunk.DidOrderChange(LastSystemVersion) ||
                    chunk.DidChange(ref TranslationTypeHandle, LastSystemVersion) ||
                    chunk.DidChange(ref RotationTypeHandle, LastSystemVersion) ||
                    chunk.DidChange(ref CompositeRotationTypeHandle, LastSystemVersion) ||
                    chunk.DidChange(ref ScaleTypeHandle, LastSystemVersion) ||
                    chunk.DidChange(ref NonUniformScaleTypeHandle, LastSystemVersion) ||
                    chunk.DidChange(ref CompositeScaleTypeHandle, LastSystemVersion) ||
                    chunk.DidChange(ref ParentScaleInverseTypeHandle, LastSystemVersion);
                if (!changed)
                {
                    return;
                }

                var chunkTranslations = chunk.GetNativeArray(ref TranslationTypeHandle);
                var chunkNonUniformScales = chunk.GetNativeArray(ref NonUniformScaleTypeHandle);
                var chunkScales = chunk.GetNativeArray(ref ScaleTypeHandle);
                var chunkCompositeScales = chunk.GetNativeArray(ref CompositeScaleTypeHandle);
                var chunkRotations = chunk.GetNativeArray(ref RotationTypeHandle);
                var chunkCompositeRotations = chunk.GetNativeArray(ref CompositeRotationTypeHandle);
                var chunkLocalToParent = chunk.GetNativeArray(ref LocalToParentTypeHandle);
                var chunkParentScaleInverses = chunk.GetNativeArray(ref ParentScaleInverseTypeHandle);
                var hasTranslation = chunk.Has(ref TranslationTypeHandle);
                var hasCompositeRotation = chunk.Has(ref CompositeRotationTypeHandle);
                var hasRotation = chunk.Has(ref RotationTypeHandle);
                var hasAnyRotation = hasCompositeRotation || hasRotation;
                var hasNonUniformScale = chunk.Has(ref NonUniformScaleTypeHandle);
                var hasScale = chunk.Has(ref ScaleTypeHandle);
                var hasCompositeScale = chunk.Has(ref CompositeScaleTypeHandle);
                var hasAnyScale = hasScale || hasNonUniformScale || hasCompositeScale;
                var hasParentScaleInverse = chunk.Has(ref ParentScaleInverseTypeHandle);
                var count = chunk.Count;

                // #todo jump table when burst supports function pointers

                if (hasParentScaleInverse)
                {
                    if (!hasAnyRotation)
                    {
                        // 00 = invalid (must have at least one)
                        // 01
                        if (!hasTranslation && hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var parentScaleInverse = chunkParentScaleInverses[i].Value;
                                var scale = hasNonUniformScale
                                    ? float4x4.Scale(chunkNonUniformScales[i].Value)
                                    : (hasScale
                                        ? float4x4.Scale(new float3(chunkScales[i].Value))
                                        : chunkCompositeScales[i].Value);

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(parentScaleInverse, scale)
                                };
                            }
                        }
                        // 10
                        else if (hasTranslation && !hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var parentScaleInverse = chunkParentScaleInverses[i].Value;
                                var translation = chunkTranslations[i].Value;

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(float4x4.Translate(translation), parentScaleInverse)
                                };
                            }
                        }
                        // 11
                        else if (hasTranslation && hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var parentScaleInverse = chunkParentScaleInverses[i].Value;
                                var scale = hasNonUniformScale
                                    ? float4x4.Scale(chunkNonUniformScales[i].Value)
                                    : (hasScale
                                        ? float4x4.Scale(new float3(chunkScales[i].Value))
                                        : chunkCompositeScales[i].Value);
                                var translation = chunkTranslations[i].Value;

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(math.mul(float4x4.Translate(translation), parentScaleInverse),
                                        scale)
                                };
                            }
                        }
                    }
                    else if (hasCompositeRotation)
                    {
                        // 00
                        if (!hasTranslation && !hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var parentScaleInverse = chunkParentScaleInverses[i].Value;
                                var rotation = chunkCompositeRotations[i].Value;

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(parentScaleInverse, rotation)
                                };
                            }
                        }
                        // 01
                        else if (!hasTranslation && hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var parentScaleInverse = chunkParentScaleInverses[i].Value;
                                var rotation = chunkCompositeRotations[i].Value;
                                var scale = hasNonUniformScale
                                    ? float4x4.Scale(chunkNonUniformScales[i].Value)
                                    : (hasScale
                                        ? float4x4.Scale(new float3(chunkScales[i].Value))
                                        : chunkCompositeScales[i].Value);

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(parentScaleInverse, math.mul(rotation, scale))
                                };
                            }
                        }
                        // 10
                        else if (hasTranslation && !hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var parentScaleInverse = chunkParentScaleInverses[i].Value;
                                var rotation = chunkCompositeRotations[i].Value;
                                var translation = chunkTranslations[i].Value;

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(math.mul(float4x4.Translate(translation), parentScaleInverse),
                                        rotation)
                                };
                            }
                        }
                        // 11
                        else if (hasTranslation && hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var parentScaleInverse = chunkParentScaleInverses[i].Value;
                                var rotation = chunkCompositeRotations[i].Value;
                                var translation = chunkTranslations[i].Value;
                                var scale = hasNonUniformScale
                                    ? float4x4.Scale(chunkNonUniformScales[i].Value)
                                    : (hasScale
                                        ? float4x4.Scale(new float3(chunkScales[i].Value))
                                        : chunkCompositeScales[i].Value);

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(
                                        math.mul(math.mul(float4x4.Translate(translation), parentScaleInverse),
                                            rotation), scale)
                                };
                            }
                        }
                    }
                    else // if (hasRotation) -- Only in same WriteGroup if !hasCompositeRotation
                    {
                        // 00
                        if (!hasTranslation && !hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var parentScaleInverse = chunkParentScaleInverses[i].Value;
                                var rotation = chunkRotations[i].Value;

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(parentScaleInverse, new float4x4(rotation, float3.zero))
                                };
                            }
                        }
                        // 01
                        else if (!hasTranslation && hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var parentScaleInverse = chunkParentScaleInverses[i].Value;
                                var rotation = chunkRotations[i].Value;
                                var scale = hasNonUniformScale
                                    ? float4x4.Scale(chunkNonUniformScales[i].Value)
                                    : (hasScale
                                        ? float4x4.Scale(new float3(chunkScales[i].Value))
                                        : chunkCompositeScales[i].Value);

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(parentScaleInverse,
                                        math.mul(new float4x4(rotation, float3.zero), scale))
                                };
                            }
                        }
                        // 10
                        else if (hasTranslation && !hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var parentScaleInverse = chunkParentScaleInverses[i].Value;
                                var rotation = chunkRotations[i].Value;
                                var translation = chunkTranslations[i].Value;

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(math.mul(float4x4.Translate(translation), parentScaleInverse),
                                        new float4x4(rotation, new float3(sfloat.Zero)))
                                };
                            }
                        }
                        // 11
                        else if (hasTranslation && hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var parentScaleInverse = chunkParentScaleInverses[i].Value;
                                var rotation = chunkRotations[i].Value;
                                var translation = chunkTranslations[i].Value;
                                var scale = hasNonUniformScale
                                    ? float4x4.Scale(chunkNonUniformScales[i].Value)
                                    : (hasScale
                                        ? float4x4.Scale(new float3(chunkScales[i].Value))
                                        : chunkCompositeScales[i].Value);

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(
                                        math.mul(math.mul(float4x4.Translate(translation), parentScaleInverse),
                                            new float4x4(rotation, new float3(sfloat.Zero))), scale)
                                };
                            }
                        }
                    }
                }
                else // (!hasParentScaleInverse)
                {
                    if (!hasAnyRotation)
                    {
                        // 00 = invalid (must have at least one)
                        // 01
                        if (!hasTranslation && hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var scale = hasNonUniformScale
                                    ? float4x4.Scale(chunkNonUniformScales[i].Value)
                                    : (hasScale
                                        ? float4x4.Scale(new float3(chunkScales[i].Value))
                                        : chunkCompositeScales[i].Value);

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = scale
                                };
                            }
                        }
                        // 10
                        else if (hasTranslation && !hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var translation = chunkTranslations[i].Value;

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = float4x4.Translate(translation)
                                };
                            }
                        }
                        // 11
                        else if (hasTranslation && hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var scale = hasNonUniformScale
                                    ? float4x4.Scale(chunkNonUniformScales[i].Value)
                                    : (hasScale
                                        ? float4x4.Scale(new float3(chunkScales[i].Value))
                                        : chunkCompositeScales[i].Value);
                                var translation = chunkTranslations[i].Value;

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(float4x4.Translate(translation), scale)
                                };
                            }
                        }
                    }
                    else if (hasCompositeRotation)
                    {
                        // 00
                        if (!hasTranslation && !hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var rotation = chunkCompositeRotations[i].Value;

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = rotation
                                };
                            }
                        }
                        // 01
                        else if (!hasTranslation && hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var rotation = chunkCompositeRotations[i].Value;
                                var scale = hasNonUniformScale
                                    ? float4x4.Scale(chunkNonUniformScales[i].Value)
                                    : (hasScale
                                        ? float4x4.Scale(new float3(chunkScales[i].Value))
                                        : chunkCompositeScales[i].Value);

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(rotation, scale)
                                };
                            }
                        }
                        // 10
                        else if (hasTranslation && !hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var rotation = chunkCompositeRotations[i].Value;
                                var translation = chunkTranslations[i].Value;

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(float4x4.Translate(translation), rotation)
                                };
                            }
                        }
                        // 11
                        else if (hasTranslation && hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var rotation = chunkCompositeRotations[i].Value;
                                var translation = chunkTranslations[i].Value;
                                var scale = hasNonUniformScale
                                    ? float4x4.Scale(chunkNonUniformScales[i].Value)
                                    : (hasScale
                                        ? float4x4.Scale(new float3(chunkScales[i].Value))
                                        : chunkCompositeScales[i].Value);

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(math.mul(float4x4.Translate(translation), rotation), scale)
                                };
                            }
                        }
                    }
                    else // if (hasRotation) -- Only in same WriteGroup if !hasCompositeRotation
                    {
                        // 00
                        if (!hasTranslation && !hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var rotation = chunkRotations[i].Value;

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = new float4x4(rotation, float3.zero)
                                };
                            }
                        }
                        // 01
                        else if (!hasTranslation && hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var rotation = chunkRotations[i].Value;
                                var scale = hasNonUniformScale
                                    ? float4x4.Scale(chunkNonUniformScales[i].Value)
                                    : (hasScale
                                        ? float4x4.Scale(new float3(chunkScales[i].Value))
                                        : chunkCompositeScales[i].Value);

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(new float4x4(rotation, float3.zero), scale)
                                };
                            }
                        }
                        // 10
                        else if (hasTranslation && !hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var rotation = chunkRotations[i].Value;
                                var translation = chunkTranslations[i].Value;

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = new float4x4(rotation, translation)
                                };
                            }
                        }
                        // 11
                        else if (hasTranslation && hasAnyScale)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var rotation = chunkRotations[i].Value;
                                var translation = chunkTranslations[i].Value;
                                var scale = hasNonUniformScale
                                    ? float4x4.Scale(chunkNonUniformScales[i].Value)
                                    : (hasScale
                                        ? float4x4.Scale(new float3(chunkScales[i].Value))
                                        : chunkCompositeScales[i].Value);

                                chunkLocalToParent[i] = new LocalToParent
                                {
                                    Value = math.mul(new float4x4(rotation, translation), scale)
                                };
                            }
                        }
                    }
                }
            }
        }

        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    typeof(LocalToParent)
                },
                Any = new ComponentType[]
                {
                    ComponentType.ReadOnly<NonUniformScale>(),
                    ComponentType.ReadOnly<Scale>(),
                    ComponentType.ReadOnly<Rotation>(),
                    ComponentType.ReadOnly<CompositeRotation>(),
                    ComponentType.ReadOnly<CompositeScale>(),
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<ParentScaleInverse>()
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });
        }

        protected override void OnUpdate()
        {
            var rotationType = GetComponentTypeHandle<Rotation>(true);
            var compositeRotationType = GetComponentTypeHandle<CompositeRotation>(true);
            var translationType = GetComponentTypeHandle<Translation>(true);
            var nonUniformScaleType = GetComponentTypeHandle<NonUniformScale>(true);
            var scaleType = GetComponentTypeHandle<Scale>(true);
            var compositeScaleType = GetComponentTypeHandle<CompositeScale>(true);
            var parentScaleInverseType = GetComponentTypeHandle<ParentScaleInverse>(true);
            var localToWorldType = GetComponentTypeHandle<LocalToParent>(false);
            var trsToLocalToParentJob = new TRSToLocalToParent()
            {
                RotationTypeHandle = rotationType,
                CompositeRotationTypeHandle = compositeRotationType,
                TranslationTypeHandle = translationType,
                ScaleTypeHandle = scaleType,
                NonUniformScaleTypeHandle = nonUniformScaleType,
                CompositeScaleTypeHandle = compositeScaleType,
                ParentScaleInverseTypeHandle = parentScaleInverseType,
                LocalToParentTypeHandle = localToWorldType,
                LastSystemVersion = LastSystemVersion
            }.ScheduleParallel(m_Group, Dependency);
        }
    }
}

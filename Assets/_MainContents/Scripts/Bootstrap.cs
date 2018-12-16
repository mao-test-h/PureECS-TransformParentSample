namespace MainContents
{
    using UnityEngine;

    using Unity.Entities;
    using Unity.Transforms;
    using Unity.Rendering;

    public struct ParentTag : IComponentData { }
    public struct ChildTag : IComponentData { }
    public struct GrandsonTag : IComponentData { }

    public sealed class Bootstrap : MonoBehaviour
    {
#pragma warning disable 0649

        // ------------------------------
        #region // Defines

        [System.Serializable]
        struct TransformStr
        {
            public Vector3 Position;
            public Vector3 Rotation;
        }

        #endregion // Defines

        // ------------------------------
        #region // Private Members(Editable)

        [Header("【MeshInstanceRenderer】")]
        [SerializeField] MeshInstanceRenderer _parentRenderer;
        [SerializeField] MeshInstanceRenderer _childRenderer;
        [SerializeField] MeshInstanceRenderer _grandsonRenderer;

        [Header("Transform】")]
        [SerializeField] TransformStr _parentTrs;
        [SerializeField] TransformStr _childTrs;
        [SerializeField] TransformStr _grandsonTrs;

        #endregion // Private Members(Editable)

        // ------------------------------
        #region // Private Members

        EntityManager _entityManager = null;
        Entity _parentEntity;
        Entity _childEntity;
        Entity _grandsonEntity;

        #endregion // Private Members

#pragma warning restore 0649


        // ----------------------------------------------------
        #region // Unity Events

        /// <summary>
        /// MonoBehaviour.Start
        /// </summary>
        void Start()
        {
            World.Active = new World("Sample World");
            this._entityManager = World.Active.CreateManager<EntityManager>();
            World.Active.CreateManager(typeof(EndFrameTransformSystem));
            World.Active.CreateManager(typeof(RenderingSystemBootstrap));
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);

            // ------------------------------------
            // 親アーキタイプ
            var parentArchetype = this._entityManager.CreateArchetype(
                // ComponentType.Create<Attach>(),  // ここに付けたら怒られた
                ComponentType.Create<ParentTag>(),
                ComponentType.Create<Prefab>(),
                ComponentType.Create<Position>(), ComponentType.Create<Rotation>(), ComponentType.Create<LocalToWorld>(),
                ComponentType.Create<MeshInstanceRenderer>());

            // 子アーキタイプ
            var childArchetype = this._entityManager.CreateArchetype(
                // ComponentType.Create<Attach>(),  // ここに付けたら怒られた
                ComponentType.Create<ChildTag>(),
                ComponentType.Create<Prefab>(),
                ComponentType.Create<Position>(), ComponentType.Create<Rotation>(), ComponentType.Create<LocalToWorld>(),
                ComponentType.Create<MeshInstanceRenderer>());

            // 孫アーキタイプ
            var grandsonArchetype = this._entityManager.CreateArchetype(
                ComponentType.Create<GrandsonTag>(),
                ComponentType.Create<Prefab>(),
                ComponentType.Create<Position>(), ComponentType.Create<Rotation>(), ComponentType.Create<LocalToWorld>(),
                ComponentType.Create<MeshInstanceRenderer>());

            // 親子構造構築用アーキタイプ
            var attachArchetype = this._entityManager.CreateArchetype(
                ComponentType.Create<Prefab>(), ComponentType.Create<Attach>());


            // ------------------------------------
            // Create Prefabs
            var parentPrefab = this._entityManager.CreateEntity(parentArchetype);
            var childPrefab = this._entityManager.CreateEntity(childArchetype);
            var grandsonPrefab = this._entityManager.CreateEntity(grandsonArchetype);
            var attachPrefab = this._entityManager.CreateEntity(attachArchetype);
            this._entityManager.SetSharedComponentData(parentPrefab, this._parentRenderer);
            this._entityManager.SetSharedComponentData(childPrefab, this._childRenderer);
            this._entityManager.SetSharedComponentData(grandsonPrefab, this._grandsonRenderer);


            // ------------------------------------
            // Create Entities
            this._parentEntity = this._entityManager.Instantiate(parentPrefab);
            this._childEntity = this._entityManager.Instantiate(childPrefab);
            this._grandsonEntity = this._entityManager.Instantiate(grandsonPrefab);

            // 親子構造の構築(親 -> 子)
            var attachEntity = this._entityManager.Instantiate(attachPrefab);
            this._entityManager.SetComponentData(attachEntity, new Attach
            {
                Parent = this._parentEntity,
                Child = this._childEntity,
            });
            // 親子構造の構築(子 -> 孫)
            attachEntity = this._entityManager.Instantiate(attachPrefab);
            this._entityManager.SetComponentData(attachEntity, new Attach
            {
                Parent = this._childEntity,
                Child = this._grandsonEntity,
            });
        }

        /// <summary>
        /// MonoBehaviour.Update
        /// </summary>
        void Update()
        {
            // 親Entity
            this._entityManager.SetComponentData(this._parentEntity, new Position { Value = this._parentTrs.Position });
            this._entityManager.SetComponentData(this._parentEntity, new Rotation { Value = Quaternion.Euler(this._parentTrs.Rotation) });

            // 子Entity
            this._entityManager.SetComponentData(this._childEntity, new Position { Value = this._childTrs.Position });
            this._entityManager.SetComponentData(this._childEntity, new Rotation { Value = Quaternion.Euler(this._childTrs.Rotation) });

            // 孫Entity
            this._entityManager.SetComponentData(this._grandsonEntity, new Position { Value = this._grandsonTrs.Position });
            this._entityManager.SetComponentData(this._grandsonEntity, new Rotation { Value = Quaternion.Euler(this._grandsonTrs.Rotation) });
        }

        /// <summary>
        /// MonoBehaviour.OnDestroy
        /// </summary>
        void OnDestroy()
        {
            World.DisposeAllWorlds();
        }

        #endregion // Unity Events
    }
}

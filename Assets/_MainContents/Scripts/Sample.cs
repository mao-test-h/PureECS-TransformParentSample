using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;

namespace MainContents
{
    public class Sample : MonoBehaviour
    {
        /// <summary>
        /// Entity情報
        /// </summary>
        [System.Serializable]
        public class EntityData
        {
            [System.Serializable]
            public struct TransformData
            {
                public Vector3 WorldPosition;
                public Vector3 LocalPosition;
                [SerializeField] Vector3 WorldRotationEuler;
                [SerializeField] Vector3 LocalRotationEuler;
                public Quaternion WorldRotation { get { return Quaternion.Euler(this.WorldRotationEuler); } }
                public Quaternion LocalRotation { get { return Quaternion.Euler(this.LocalRotationEuler); } }
            }

            /// <summary>
            /// Transform関連のComponentDataに渡す情報
            /// </summary>
            public TransformData TransformDataInstance;

            /// <summary>
            /// MeshInstanceRendererで設定するMaterial
            /// </summary>
            public Material Material;

            /// <summary>
            /// Entityの実態
            /// </summary>
            public Entity Entity;
        }

        // ------------------------------
        #region // Private Members(Editable)

        /// <summary>
        /// 表示するMesh
        /// </summary>
        [SerializeField] Mesh _mesh;

        /// <summary>
        /// Entity情報
        /// </summary>
        [SerializeField] EntityData[] _entityData;

        /// <summary>
        /// EntityManagerの参照
        /// </summary>
        EntityManager _entityManager;

        #endregion // Private Members(Editable)

        // ----------------------------------------------------
        #region // Unity Events

        /// <summary>
        /// MonoBehaviour.Start
        /// </summary>
        void Start()
        {
            this._entityManager = World.Active.GetOrCreateManager<EntityManager>();

            // ルートEntity アーキタイプ
            var rootArchetype = this._entityManager.CreateArchetype(
                typeof(Position),
                typeof(LocalPosition),
                typeof(Rotation),
                typeof(LocalRotation),
                typeof(TransformMatrix));

            // 子Entity アーキタイプ
            var childArchetype = this._entityManager.CreateArchetype(
                typeof(Position),
                typeof(LocalPosition),
                typeof(Rotation),
                typeof(LocalRotation),
                typeof(TransformParent),
                typeof(TransformMatrix));

            Entity parent = Entity.Null;
            for (int i = 0; i < this._entityData.Length; ++i)
            {
                var data = this._entityData[i];
                bool isRoot = (i == 0);

                // Entityの生成
                var archetype = isRoot ? rootArchetype : childArchetype;
                var entity = this._entityManager.CreateEntity(archetype);
                data.Entity = entity;

                // Transformの初期化
                this._entityManager.SetComponentData(entity, new Position { Value = Vector3.zero });
                this._entityManager.SetComponentData(entity, new LocalPosition { Value = Vector3.zero });
                this._entityManager.SetComponentData(entity, new Rotation { Value = quaternion.identity });
                this._entityManager.SetComponentData(entity, new LocalRotation { Value = quaternion.identity });

                // ルート以外は以前に生成されたEntityを親として割り当てる
                if (!isRoot)
                {
                    this._entityManager.SetComponentData(entity, new TransformParent { Value = parent });
                }
                parent = entity;

                // MeshInstanceRendererの設定
                var look = this.CreateMeshInstanceRenderer(data.Material);
                this._entityManager.AddSharedComponentData(entity, look);
            }
        }

        /// <summary>
        /// MonoBehaviour.Update
        /// </summary>
        void Update()
        {
            foreach (var data in this._entityData)
            {
                var entity = data.Entity;
                var trs = data.TransformDataInstance;
                // Inspector上から設定した座標情報をComponentDataに渡して動かす
                this._entityManager.SetComponentData(entity, new Position { Value = trs.WorldPosition });
                this._entityManager.SetComponentData(entity, new LocalPosition { Value = trs.LocalPosition });
                this._entityManager.SetComponentData(entity, new Rotation { Value = trs.WorldRotation });
                this._entityManager.SetComponentData(entity, new LocalRotation { Value = trs.LocalRotation });
            }
        }

        #endregion // Unity Events

        // ----------------------------------------------------
        #region // Private Functions

        /// <summary>
        /// MeshInstanceRendererの生成
        /// </summary>
        /// <returns>生成したMeshInstanceRenderer</returns>
        public MeshInstanceRenderer CreateMeshInstanceRenderer(Material mat)
        {
            var matInst = new Material(mat);
            var meshInstanceRenderer = new MeshInstanceRenderer();
            meshInstanceRenderer.mesh = this._mesh;
            meshInstanceRenderer.material = matInst;
            return meshInstanceRenderer;
        }

        #endregion // Private Functions
    }
}
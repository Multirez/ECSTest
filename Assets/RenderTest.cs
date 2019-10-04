using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;
using Unity.Entities;
using Unity.Transforms;

public class RenderTest : MonoBehaviour
{
    public class MovedObject
    {
        public Transform Transform;
        public float VerticalSpeed;
    }

    [SerializeField] bool _useJobs;
    [SerializeField] Transform _objPrefab;

    private List<MovedObject> _objList = new List<MovedObject>();
    private NativeArray<Entity> _entities;
    private EntityManager _entityManager;
    private RenderMesh _sharedRender;

    private void Start()
    {
        if(_useJobs)
            _sharedRender = CreateSharedRenderer();
        //SpawnObjects(3);
    }

    private void InstantiateObjects(int count)
    {
        _objList = _objList ?? new List<MovedObject>(count);
        for (int i = 0; i < count; i++)
        {
            Transform zombie = Instantiate(_objPrefab, UnityEngine.Random.insideUnitSphere, _objPrefab.rotation);
            _objList.Add(new MovedObject
            {
                Transform = zombie,
                VerticalSpeed = UnityEngine.Random.Range(0.1f, 0.5f)
            });
        }
    }

    public void SpawnObjects(int count)
    {
        Debug.Log("Try spawn " + count);
        if (_useJobs)
        {
            InitEntities(count);
        }
        else
        {
            InstantiateObjects(count);
        }
    }

    private void InitEntities(int count)
    {
        _entityManager = World.Active.EntityManager;
        var architype = _entityManager.CreateArchetype(
            typeof(Translation),
            typeof(Rotation), 
            typeof(LocalToWorld),
            typeof(RenderBounds),
            typeof(MeshRenderer),
            typeof(VerticalSpeed));

        int startIndex = 0;
        if(_entities.IsCreated)
        {
            startIndex = _entities.Length;
            count += startIndex;
            ResizeNativeArray(ref _entities, count);
        }
        else
        {
            _entities = new NativeArray<Entity>(count, Allocator.Persistent);
        }

        AABB renderBounds = _sharedRender.mesh.bounds.ToAABB();
        for (int i = startIndex; i < count; i++)
        {
            var entity = _entityManager.CreateEntity(architype);
            float speed = UnityEngine.Random.Range(0.1f, 0.4f);
            float3 position = UnityEngine.Random.insideUnitSphere;
            _entityManager.SetComponentData(entity, new Translation { Value = position });
            _entityManager.SetComponentData(entity, new Rotation { Value = quaternion.identity });
            _entityManager.SetComponentData(entity, new RenderBounds { Value = renderBounds });
            _entityManager.SetComponentData(entity, new VerticalSpeed { Value = speed });
            _entityManager.AddSharedComponentData(entity, _sharedRender);

            _entities[i] = entity;
        }
    }

    private void ResizeNativeArray<T>(ref NativeArray<T> array, int newCount) where T : struct
    {
        var entityList = new List<T>(newCount);
        entityList.AddRange(array);
        for (int i = array.Length; i < newCount; i++)
            entityList.Add(new T());
        array.Dispose();
        array = new NativeArray<T>(entityList.ToArray(), Allocator.Persistent);
    }

    private RenderMesh CreateSharedRenderer()
    {
        Mesh myMesh = _objPrefab.GetComponent<MeshFilter>().sharedMesh;
        Material myMaterial = _objPrefab.GetComponent<Renderer>().sharedMaterial;

        return new RenderMesh
        {
            mesh = myMesh,
            material = myMaterial,
            castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
            receiveShadows = true,
            subMesh = 0,
        };
    }

    private void Update()
    {
        UpdatePositions();
    }

    private void OnDestroy()
    {
        if(_entities.IsCreated)
        {
            _entities.Dispose();
        }
    }

    private void UpdatePositions()
    {
        if (!_useJobs)
        {
            Vector3 pos;
            foreach (var someObj in _objList)
            {
                pos = someObj.Transform.position;
                if (pos.y > 1f)
                    someObj.VerticalSpeed = -math.abs(someObj.VerticalSpeed);
                else if (pos.y < -1f)
                    someObj.VerticalSpeed = math.abs(someObj.VerticalSpeed);

                someObj.Transform.Translate(new Vector3(0f, someObj.VerticalSpeed * Time.deltaTime, 0f));
            }
        }        
    }

}



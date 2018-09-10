using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ShapeFactory : ScriptableObject
{
    [SerializeField] private Shape[] _prefabs;
    [SerializeField] private Material[] _materials;
    [SerializeField] private bool _recycle;

    private List<Shape>[] _pools;
    
    public Shape Get(int shapeId = 0, int materialId = 0)
    {
        Shape instance = null;
        var instantiateShape = true;

        if (_recycle)
        {
            if (_pools == null)
            {
                CreatePools();
            }

            var pool = _pools[shapeId];
            var lastIndex = pool.Count - 1;

            if (lastIndex >= 0)
            {
                instantiateShape = false;

                instance = pool[lastIndex];
                instance.gameObject.SetActive(true);
                pool.RemoveAt(lastIndex);
            }
        }

        if(instantiateShape)
        {
            instance = Instantiate(_prefabs[shapeId]);
            instance.ShapeId = shapeId;
        }

        instance.SetMaterial(_materials[materialId], materialId);

        return instance;
    }

    public Shape GetRandom()
    {
        return Get(
            Random.Range(0, _prefabs.Length),
            Random.Range(0, _materials.Length)
            );
    }

    public void Reclaim(Shape shapeToReclaim)
    {
        if (_recycle)
        {
            if (_pools == null)
            {
                CreatePools();
            }
            
            _pools[shapeToReclaim.ShapeId].Add(shapeToReclaim);
            shapeToReclaim.gameObject.SetActive(false);
        }
        else
        {
            Destroy(shapeToReclaim.gameObject);
        }
    }

    private void CreatePools()
    {
        _pools = new List<Shape>[_prefabs.Length];

        for (var i = 0; i < _pools.Length; i++)
        {
            _pools[i] = new List<Shape>();
        }
    }
}

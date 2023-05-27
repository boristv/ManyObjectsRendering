using UnityEngine;

public class Example1 : MonoBehaviour
{
    [SerializeField] private Transform _objectPrefab;

    private float[] _objectYOffsets;
    private Transform[] _spawnedObjects;

    private void Start()
    {
        var count = MovingController.GetCount;
        _spawnedObjects = new Transform[count];
        _objectYOffsets = new float[count];
        
        MovingController.SetPositions((i, p) =>
        {
            _objectYOffsets[i] = p.y;
            _spawnedObjects[i] = Instantiate(_objectPrefab, p, Quaternion.identity, transform);
        });
    }

    private void Update()
    {
        var time = Time.time;
        for (var i = 0; i < _spawnedObjects.Length; i++)
        {
            var obj = _spawnedObjects[i];

            var (pos, rot) = obj.position.CalculatePos(_objectYOffsets[i], time);
            obj.SetPositionAndRotation(pos, rot);
        }
    }
}
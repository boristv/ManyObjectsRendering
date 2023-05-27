using UnityEngine;

public class Example2 : MonoBehaviour
{
    [SerializeField] private Transform _objectPrefab;

    private float[] _objectYOffsets;
    private Transform[] _spawnedObjects;
    private Vector3[] _lastPositions;

    private void Start()
    {
        var count = MovingController.GetCount;
        _spawnedObjects = new Transform[count];
        _objectYOffsets = new float[count];
        _lastPositions = new Vector3[count];

        MovingController.SetPositions((i, p) =>
        {
            _objectYOffsets[i] = p.y;
            _lastPositions[i] = p;
            _spawnedObjects[i] = Instantiate(_objectPrefab, p, Quaternion.identity, transform);
        });
    }

    private void Update()
    {
        var time = Time.time;
        for (var i = 0; i < _spawnedObjects.Length; i++)
        {
            var (pos, rot) = _lastPositions[i].CalculatePos(_objectYOffsets[i], time);

            _lastPositions[i] = pos;

            _spawnedObjects[i].SetPositionAndRotation(pos, rot);
        }
    }
}
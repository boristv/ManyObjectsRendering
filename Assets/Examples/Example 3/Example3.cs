using UnityEngine;

public class Example3 : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;

    private float[] _objectYOffsets;
    private ParticleSystem.Particle[] _cloud;

    private void Start()
    {
        var count = MovingController.GetCount;
        _cloud = new ParticleSystem.Particle[count];
        _objectYOffsets = new float[count];
        
        MovingController.SetPositions((i, p) =>
        {
            _objectYOffsets[i] = p.y;
            _cloud[i].startSize = 1;
            _cloud[i].position = p;
            _cloud[i].velocity = Vector3.zero;
            _cloud[i].startColor = Color.white;
        });
        _particleSystem.SetParticles(_cloud, _cloud.Length);
    }

    private void Update()
    {
        var time = Time.time;
        for (var i = 0; i < _cloud.Length; i++)
        {
            var (pos, rot) = _cloud[i].position.CalculatePos(_objectYOffsets[i], time);
            _cloud[i].position = pos;
        }
        _particleSystem.SetParticles(_cloud);
    }
}
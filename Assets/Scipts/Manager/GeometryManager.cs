using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryManager : MonoBehaviour
{
    // Singleton Object
    public static GeometryManager Instance { get; private set; }

    [SerializeField]
    private Transform receiverTransform;
    [SerializeField]
    private Transform speakerTransform;
    [SerializeField]
    private Transform wallTransform;

    private float _sourceDistance;
    private float _wallDistance;
    private float _reflectionDistance; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else 
        {
            Instance = this;
        }

    }

    private void Start()
    {
        CalculateGeometry();

        float a = Mathf.Abs(receiverTransform.position.y - speakerTransform.position.y);
        //float b = Mathf.Abs(receiverTransform.position.z - speakerTransform.position.z);
        // Vector3 b = Vector3.Distance(receiverTransform.position - speakerTransform.position);
        
        // float distance = Vector3.Distance(cameraTransform.position, speakerTransform.position);

        // float b = 
    }

    private void CalculateSourceDistance()
    {
        if (receiverTransform.position.x == speakerTransform.position.x)
        {
            _sourceDistance = Mathf.Abs(receiverTransform.position.z - speakerTransform.position.z);
        }
        else if (receiverTransform.position.z == speakerTransform.position.z)
        {
            _sourceDistance = Mathf.Abs(receiverTransform.position.x - speakerTransform.position.x);
        }

        _wallDistance = Mathf.Abs(receiverTransform.position.z - wallTransform.position.z);
    }

    public void CalculateReflectionDistance(Transform receiver, Transform speaker)
    {
        // reflectionDistance = Vector3.Reflect(receiver, Vector3.Right()); 
    }

    public void CalculateGeometry() 
    {
        CalculateSourceDistance();
    }

    public string SourceDistance()
    {
        return _sourceDistance.ToString();
    }

    public string WallDistance()
    {
        return _wallDistance.ToString();
    }
}

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
    }

    private void CalculateSourceDistance()
    {
        // Calculate distance if objects are on the same axis 
        // or else use Pythagoras for triangle base (b) since we know (a) & (c)
        if (receiverTransform.position.x == speakerTransform.position.x)
        {
            print("On z axis");
            _sourceDistance = Mathf.Abs(receiverTransform.position.z - speakerTransform.position.z);
        }
        else if (receiverTransform.position.z == speakerTransform.position.z)
        {
            print("On z axis");
            _sourceDistance = Mathf.Abs(receiverTransform.position.x - speakerTransform.position.x);
        }
        else 
        {
            print("Use Pythagoras");
            // Pythagoras: b = sqrt(c^2 - a^2)
            float a = Mathf.Abs(receiverTransform.position.y - speakerTransform.position.y);
            float c = Vector3.Distance(receiverTransform.position, speakerTransform.position);
            float b = Mathf.Sqrt(Mathf.Pow(c, 2) - Mathf.Pow(a, 2));
            
            _sourceDistance = b;
        }

        _wallDistance = Mathf.Abs(receiverTransform.position.z - wallTransform.position.z);
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

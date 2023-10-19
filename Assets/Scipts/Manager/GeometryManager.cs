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

    private void CalculateSourceDistance()
    {
        // Calculate distance if objects are on the same axis 
        // or else use Pythagoras for finding triangle base (b) since we know (a) & (c)
        if (receiverTransform.position.x == speakerTransform.position.x)
        {
            _sourceDistance = Mathf.Abs(receiverTransform.position.z - speakerTransform.position.z);
        }
        else if (receiverTransform.position.z == speakerTransform.position.z)
        {
            _sourceDistance = Mathf.Abs(receiverTransform.position.x - speakerTransform.position.x);
        }
        else 
        {
            // Pythagoras: b = sqrt(c^2 - a^2)
            float a = Mathf.Abs(receiverTransform.position.y - speakerTransform.position.y);
            float c = Vector3.Distance(receiverTransform.position, speakerTransform.position);
            float b = Mathf.Sqrt(Mathf.Pow(c, 2) - Mathf.Pow(a, 2));
            
            _sourceDistance = b;
        }

        _wallDistance = Mathf.Abs(receiverTransform.position.z - wallTransform.position.z);
    }

    private void CalculateReflectionDistance()
    {
        // Get normal of the reflection
        float normal = Mathf.Abs(receiverTransform.position.x - speakerTransform.position.x);

        // Calculate (a) & (b) from positions and use pythagoras for hypotenuse (c);
        float aReceiver = Mathf.Abs(receiverTransform.position.z - wallTransform.position.z);
        float bReceiver = Mathf.Abs(receiverTransform.position.x - normal);
        float cReceiver = Mathf.Sqrt(Mathf.Pow(aReceiver, 2) + Mathf.Pow(bReceiver, 2));

        float aSpeaker = Mathf.Abs(speakerTransform.position.z - wallTransform.position.z);
        float bSpeaker = Mathf.Abs(speakerTransform.position.x - normal);
        float cSpeaker = Mathf.Sqrt(Mathf.Pow(aSpeaker, 2) + Mathf.Pow(bSpeaker, 2));

        _reflectionDistance = cReceiver + cSpeaker;
    }

    public void CalculateGeometry()
    {
        CalculateSourceDistance();

        CalculateReflectionDistance();
    }

    public string DistanceToSource()
    {
        return _sourceDistance.ToString("F2");
    }

    public string DistanceToWall()
    {
        return _wallDistance.ToString("F2");
    }

    public string DistanceOfReflection()
    {
        return _reflectionDistance.ToString("F2");
    }
}

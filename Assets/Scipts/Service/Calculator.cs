using System;
using UnityEngine;

struct Calculator
{
    public static float CalculateDistanceToReceiver(Transform receiverTransform, Transform speakerTransform)
    {
        // Calculate distance if objects are on the same axis 
        // or else use Pythagoras for finding triangle base (b) since we know (a) & (c)
        if (receiverTransform.position.x == speakerTransform.position.x)
        {
            return Mathf.Abs(receiverTransform.position.z - speakerTransform.position.z);
        }
        else if (receiverTransform.position.z == speakerTransform.position.z)
        {
            return Mathf.Abs(receiverTransform.position.x - speakerTransform.position.x);
        }
        else 
        {
            // Pythagoras: b = sqrt(c^2 - a^2)
            float a = Mathf.Abs(receiverTransform.position.y - speakerTransform.position.y);
            float c = Vector3.Distance(receiverTransform.position, speakerTransform.position);
            float b = Mathf.Sqrt(Mathf.Pow(c, 2) - Mathf.Pow(a, 2));
            
            return  b;
        }
    }

    public static float CalculateAzimuth(Transform receiverTransform, Transform speakerTransform)
    {
        // Retrieving the direction vector of the receiver
        Vector3 direction = speakerTransform.position - receiverTransform.position;

        // Calculating azimuth: atan(x0/z0)
        float azimuth = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        azimuth = (azimuth + 360) % 360;
     
        return  azimuth;
    }

    public static float CalculateElevation(Transform receiverTransform, Transform speakerTransform)
    {
        // Retrieving the direction vector of the receiver
        Vector3 direction = speakerTransform.position - receiverTransform.position;

        // Calculating distance by: sqrt(x^2 + z^2)
        float distance = (float)Math.Sqrt(Math.Pow(direction.x, 2) + (float)Math.Pow(direction.z, 2));

        // Calculating elevation: atan(y0/distance)
        float elevation = MathF.Atan2(direction.y, distance) * Mathf.Rad2Deg;

        elevation = (elevation + 360) % 360;
        
        return elevation;
    }

    private static float CalculateReflectionDistance(Transform receiverTransform, Transform speakerTransform, Transform wallTransform)
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

        return cReceiver + cSpeaker;
    }

    public static Vector3 CalculateNewPosition()
    {
        float randomX = UnityEngine.Random.Range(Dimensions.lowerThreshold.x, Dimensions.upperThreshold.x);
        float randomY = UnityEngine.Random.Range(Dimensions.lowerThreshold.y, Dimensions.upperThreshold.y);
        float randomZ = UnityEngine.Random.Range(Dimensions.lowerThreshold.z, Dimensions.upperThreshold.z);

        Vector3 newPosition = new(randomX, randomY, randomZ);

        return newPosition;
    }
}

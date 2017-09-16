using UnityEngine;

namespace Spyware
{
    public class Spyware_Handgun : Spyware_FireArm
    {
        //slideMin = default position, slideMax = pulledPoistion
        public Vector3 slideMin;
        public Vector3 slideMax;
        public Spyware_HandgunSlide Slide;


        public Vector3 ClosestPointOnLine(Vector3 startPoint, Vector3 endPoint, Vector3 tPoint)
        {
            Vector3 startPointTotPointVector = tPoint - startPoint;
            Vector3 startPointToEndPointVector = (endPoint - startPoint).normalized;

            float d = Vector3.Distance(startPoint, endPoint);
            float t = Vector3.Dot(startPointToEndPointVector, startPointTotPointVector);

            if (t <= 0)
                return startPoint;

            if (t >= d)
                return endPoint;

            Vector3 distanceAlongVector = startPointToEndPointVector * t;

            Vector3 closestPoint = startPoint + distanceAlongVector;

            return closestPoint;
        }
    }
}
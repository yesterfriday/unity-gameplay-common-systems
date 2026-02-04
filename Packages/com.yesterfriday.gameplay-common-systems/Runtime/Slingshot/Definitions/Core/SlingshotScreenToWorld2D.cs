using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Slingshot
{
    
    public static class SlingshotScreenToWorld2D
    {
        public static bool TryGetWorldOnOriginPlane(
            Camera camera,
            Vector2 screenPosition,
            Vector3 originWorld,
            out Vector3 worldPosition)
        {
            worldPosition = default;
            if (camera == null)
            {
                return false;
            }
            
            Plane plane = new Plane(Vector3.forward, originWorld);
            
            Ray ray = camera.ScreenPointToRay(screenPosition);

            if (!plane.Raycast(ray, out float distance))
            {
                return false;
            }
            
            worldPosition = ray.GetPoint(distance);
            worldPosition.z = originWorld.z;
            return true;
        }
    }   
}

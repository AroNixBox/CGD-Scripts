using UnityEngine;

namespace Common.Runtime.Extensions {
    public static class VectorMath {
        public static Vector3 ExtractDotVector(Vector3 vector, Vector3 direction) {
            direction.Normalize();
            return vector * Vector3.Dot(vector, direction);
        }
        public static float GetDotProduct(Vector3 vector, Vector3 direction) {
            return Vector3.Dot(vector, direction.normalized);
        }
        public static Vector3 RemoveDotVector(Vector3 vector, Vector3 direction) {
            direction.Normalize();
            return vector - direction * Vector3.Dot(vector, direction);
        }
    }
}
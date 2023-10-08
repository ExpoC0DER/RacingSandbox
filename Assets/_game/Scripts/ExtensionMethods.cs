using UnityEngine;

namespace _game.Scripts
{
    public static class ExtensionMethods
    {
        public static Vector3 RoundToMultiple(this Vector3 value, int multipleOf)
        {
            return new Vector3(
                RountToMultiple(value.x, multipleOf),
                RountToMultiple(value.y, multipleOf),
                RountToMultiple(value.z, multipleOf));
        }

        public static int RountToMultiple(float value, int multipleOf) { return (int)(Mathf.Round(value / multipleOf) * multipleOf); }

        public static Vector3 MultiplyBy(this Vector3 a, Vector3 b) { return new(a.x * b.x, a.y * b.y, a.z * b.z); }

        public static Vector3 To2D(this Vector3 a) { return a.MultiplyBy(new(1, 0, 1)); }
    }
}

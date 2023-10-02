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

        public static int RountToMultiple(float value, int multipleOf)
        {
            return (int)(Mathf.Round(value / multipleOf) * multipleOf);
        }
    }
}
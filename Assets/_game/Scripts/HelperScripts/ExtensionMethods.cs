using System.IO;
using UnityEngine;

namespace _game.Scripts.HelperScripts
{
    public static class ExtensionMethods
    {
        public static string TimeToString(this int milliseconds)
        {
            string output = "";
            output += (milliseconds / (60 * 1000)).ToString("D2");
            milliseconds %= (60 * 1000);
            output += ":";
            output += (milliseconds / 1000).ToString("D2");
            milliseconds %= 1000;
            output += ".";
            output += (milliseconds / 10).ToString("D2");
            return output;
        }

        public static Vector3 RoundToMultiple(this Vector3 value, int multipleOf)
        {
            return new Vector3(
                RoundToMultiple(value.x, multipleOf),
                RoundToMultiple(value.y, multipleOf),
                RoundToMultiple(value.z, multipleOf));
        }
        
        public static Texture2D LoadTexture(string filePath)
        {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D tex2D = new Texture2D(2, 2);
                if (tex2D.LoadImage(fileData)) // Load the imagedata into the texture (size is set automatically)
                    return tex2D; // If data = readable -> return texture
            }

            return null; // Return null if load failed
        }

        public static Vector3 Abs(this Vector3 vector3) { return new Vector3(Mathf.Abs(vector3.x), Mathf.Abs(vector3.y), Mathf.Abs(vector3.z)); }

        public static int RoundToMultiple(float value, int multipleOf) { return (int)(Mathf.Round(value / multipleOf) * multipleOf); }

        public static Vector3 MultiplyBy(this Vector3 a, Vector3 b) { return new(a.x * b.x, a.y * b.y, a.z * b.z); }

        public static Vector3 To2D(this Vector3 a) { return a.MultiplyBy(new(1, 0, 1)); }
        public static float Remap(this float value, float from1, float to1, float from2, float to2) { return (value - from1) / (to1 - from1) * (to2 - from2) + from2; }
    }
}

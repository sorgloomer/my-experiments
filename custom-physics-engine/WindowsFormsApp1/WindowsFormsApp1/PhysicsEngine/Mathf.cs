using System;

namespace WindowsFormsApp1.PhysicsEngine
{
    public static class Mathf
    {
        public const float EPS = 1e-7f;
        public const float LARGE_EPS = 5e-4f;
        public const float PI = (float)Math.PI;
        public const float TWO_PI = 2 * PI;
        public const float DEG_TO_RAD = PI / 180f;
        public const float RAD_TO_DEG = 180f / PI;
        
        public static float Clamp(float x, float minx, float maxx)
        {
            if (x <= minx) return minx;
            if (x >= maxx) return maxx;
            return x;
        }
        public static float Clamp01(float x)
        {
            if (x <= 0) return 0;
            if (x >= 1) return 1;
            return x;
        }
        public static float ClampSymmetric1(float x)
        {
            if (x <= -1) return -1;
            if (x >= 1) return 1;
            return x;
        }

        public static float EpsSign(float x)
        {
            return EpsSignI(x);
        }
        public static int EpsSignI(float x)
        {
            if (x < -EPS) return -1; 
            if (x > EPS) return 1;
            return 0;
        }

        public static float Abs(float x)
        {
            return Math.Abs(x);
        }

        public static float Sq(float x)
        {
            return x * x;
        }

        public static float Sqrt(float x)
        {
            return (float)Math.Sqrt(x);
        }

        public static float Floor(float x)
        {
            return (float)Math.Floor(x);
        }

        public static float TryInvertPositive(float x, out bool failed)
        {
            failed = x < EPS; 
            if (failed) return 0;
            return 1f / x;

        }

        public static float Pow(float x, float e)
        {
            return (float) Math.Pow(x, e);
        }

        public static float ClampSymmetric(float x, float max)
        {
            return Clamp(x, -max, max);
        }
    }
}
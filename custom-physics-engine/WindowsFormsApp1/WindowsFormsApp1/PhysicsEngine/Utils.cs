namespace WindowsFormsApp1.PhysicsEngine
{
    public static class MyUtils
    {
        public static bool With<T>(this T? optValie, out T value) where T : struct
        {
            value = optValie ?? default;
            return optValie.HasValue;
        }
    }
}
using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace Sphere;

static class Program
{
    private const int Scale = 4;
    private const int Width = 240;
    private const int Height = 240;
    private const int WindowWidth = Width * Scale;
    private const int WindowHeight = Height * Scale;
    
    static void Main()
    {
        InitWindow(WindowWidth, WindowHeight, "Sphere");
        SetTargetFPS(60);
        
        Image image = GenImageColor(Width, Height, Color.Black);
        Texture2D texture = LoadTextureFromImage(image);

        Span<Color> pixels;
        unsafe
        {
            pixels = new Span<Color>((Color*)image.Data, Width * Height);
        }

        Vector3 centre = new(Width / 2, Height / 2, 0);
        Vector3[] spherePoints = GenerateSphere(100, 40, 20);
        
        while (!WindowShouldClose())
        {
            ImageClearBackground(ref image, Color.Black);
            
            float time = (float)GetTime();

            Matrix4x4 rotX = Matrix4x4.CreateRotationX(time);
            Matrix4x4 rotY = Matrix4x4.CreateRotationY(time);
            Matrix4x4 rotZ = Matrix4x4.CreateRotationZ(time);
            Matrix4x4 rotationMatrix = rotY * rotX * rotZ;
            
            foreach (Vector3 p in spherePoints)
            {
                Vector3 point = Vector3.Transform(p, rotationMatrix);

                // apply simple perspective correction
                float perspective = 200f - point.Z;
                point.X /= perspective;
                point.Y /= perspective;

                point = centre + point * centre;

                // draw the points
                ImageDrawPixel(ref image, (int)point.X, (int)point.Y, Color.DarkGreen);
            }
            
            UpdateTexture<Color>(texture, pixels);
            BeginDrawing();
            DrawTextureEx(texture, Vector2.Zero, 0, Scale, Color.White);
            EndDrawing();
        }
    }
    
    // fill a Vector3 array with points for a sphere
    private static Vector3[] GenerateSphere(int radius, int numPointsHorizontal, int numPointsVertical)
    {
        Vector3[] points = new Vector3[numPointsHorizontal * numPointsVertical];
        float incrementHorizontal = MathF.Tau / numPointsHorizontal;
        float incrementVertical = MathF.PI / numPointsVertical;
        
        int index = 0;
        for (int i = 0; i < numPointsVertical; i++)
        {
            float phi = i * incrementVertical;
            for (int j = 0; j < numPointsHorizontal; j++)
            {
                float theta = j * incrementHorizontal;
                float x = radius * MathF.Cos(theta) * MathF.Sin(phi);
                float y = radius * MathF.Sin(theta) * MathF.Sin(phi);
                float z = radius * MathF.Cos(phi);
                
                points[index++] = new Vector3(x, y, z);
            }
        }

        return points;
    }
}
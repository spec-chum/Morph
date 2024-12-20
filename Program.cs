﻿using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace Morph;

readonly struct Shape(Vector3[] points, Vector4 color)
{
    public Vector3[] Points { get; init; } = points;
    public Vector4 Color { get; init; } = color;
}

static class Program
{
    const int Scale = 4;
    const int Width = 240;
    const int Height = 240;
    const int WindowWidth = Width * Scale;
    const int WindowHeight = Height * Scale;

    const float RotationSpeed = 0.015f;
    const float MorphSpeed = 0.005f;
    const float HoldIncrement = 0.01f;
    const float HoldDuration = 2f;

    static void Main()
    {
        InitWindow(WindowWidth, WindowHeight, "Morph");
        SetTargetFPS(60);

        Vector2 centre = new(Width / 2, Height / 2);

        Image image = GenImageColor(Width, Height, Color.Black);
        ImageFormat(ref image, PixelFormat.UncompressedR32G32B32A32);
        Texture2D texture = LoadTextureFromImage(image);
        UnloadImage(image);

        Span<Vector4> pixels = new Vector4[Width * Height];

        // Generate shapes
        Shape sphere = new(GenerateSphere(100, 40, 20), ColorNormalize(Color.DarkGreen));
        Shape torus = new(GenerateTorus(70, 30, 40, 20), ColorNormalize(Color.Maroon));

        // Pre-calc rotation quaternions
        Dictionary<float, Quaternion> quats = [];
        for (float t = 0; t < MathF.Tau; t += RotationSpeed)
        {
            quats.Add(t, Quaternion.CreateFromYawPitchRoll(t, t, t));
        }

        float time = 0;
        float morphTime = 0;
        float holdTime = 0;
        bool isMorphingToTorus = false;

        while (!WindowShouldClose())
        {
            pixels.Fill(Vector4.UnitW);

            morphTime += isMorphingToTorus ? MorphSpeed : -MorphSpeed;
            morphTime = Math.Clamp(morphTime, 0, 1);

            if (morphTime is 0 or 1)
            {
                holdTime += HoldIncrement;
                if (holdTime >= HoldDuration)
                {
                    isMorphingToTorus = !isMorphingToTorus;
                    holdTime = 0;
                }
            }

            Quaternion rotationQuat = quats[time];

            for (int i = 0; i < sphere.Points.Length; i++)
            {
                // transform points with rotation quaternion
                var transformedPoint = Vector3.Transform(Vector3.Lerp(sphere.Points[i], torus.Points[i], morphTime),
                    rotationQuat);

                // apply simple perspective correction and map to screen space
                float perspective = 200f - transformedPoint.Z;
                Vector2 screenPoint = new Vector2(transformedPoint.X, transformedPoint.Y) / perspective * centre;
                screenPoint += centre;

                // lerp colour and draw pixel
                var color = Vector4.Lerp(sphere.Color, torus.Color, morphTime);
                pixels[((int)screenPoint.Y * Width) + (int)screenPoint.X] = color;
            }

            UpdateTexture<Vector4>(texture, pixels);
            BeginDrawing();
            DrawTextureEx(texture, Vector2.Zero, 0, Scale, Color.White);
            EndDrawing();

            time += RotationSpeed;
            if (time >= MathF.Tau)
            {
                time = 0;
            }
        }

        // Clean up
        UnloadTexture(texture);
        CloseWindow();
    }

    private static Vector3[] GenerateSphere(int radius, int numPointsHorizontal, int numPointsVertical)
    {
        var points = new Vector3[numPointsHorizontal * numPointsVertical];

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

                points[index++] = new(x, y, z);
            }
        }

        return points;
    }

    private static Vector3[] GenerateTorus(float ringRadius, float tubeRadius, int numPointsHorizontal, int numPointsVertical)
    {
        var points = new Vector3[numPointsHorizontal * numPointsVertical];

        float incrementHorizontal = MathF.Tau / numPointsHorizontal;
        float incrementVertical = MathF.Tau / numPointsVertical;

        int index = 0;
        for (int i = 0; i < numPointsVertical; i++)
        {
            float phi = i * incrementVertical;
            for (int j = 0; j < numPointsHorizontal; j++)
            {
                float theta = j * incrementHorizontal;
                float x = (ringRadius + (tubeRadius * MathF.Cos(phi))) * MathF.Cos(theta);
                float y = (ringRadius + (tubeRadius * MathF.Cos(phi))) * MathF.Sin(theta);
                float z = tubeRadius * MathF.Sin(phi);

                points[index++] = new(x, y, z);
            }
        }

        return points;
    }
}
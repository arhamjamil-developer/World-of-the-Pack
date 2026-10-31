using System;
using UnityEngine;

namespace WorldOfThePack.Game
{
    public static class LaneSystem
    {
        public const int LaneCount = 5;
        public const float MinCorridorX = -1.8f;
        public const float MaxCorridorX = 1.8f;
        public const float LaneSpacing = 0.72f;

        private static readonly float[] LaneXPositions = new float[LaneCount]
        {
            -1.44f, // Lane 0 (Far Left)
            -0.72f, // Lane 1 (Mid Left)
             0.00f, // Lane 2 (Center - Alpha Lane)
             0.72f, // Lane 3 (Mid Right)
             1.44f  // Lane 4 (Far Right)
        };

        public static float GetLaneX(int laneIndex)
        {
            int clamped = Mathf.Clamp(laneIndex, 0, LaneCount - 1);
            return LaneXPositions[clamped];
        }

        public static int GetNearestLaneIndex(float worldX)
        {
            float clampedX = Mathf.Clamp(worldX, MinCorridorX, MaxCorridorX);
            int bestLane = 0;
            float bestDist = float.MaxValue;

            for (int i = 0; i < LaneCount; i++)
            {
                float dist = Mathf.Abs(clampedX - LaneXPositions[i]);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestLane = i;
                }
            }
            return bestLane;
        }

        public static float SnapToLane(float worldX)
        {
            return GetLaneX(GetNearestLaneIndex(worldX));
        }

        public static string GetLaneName(int laneIndex)
        {
            return laneIndex switch
            {
                0 => "Flank Left",
                1 => "Mid Left",
                2 => "Center (Alpha)",
                3 => "Mid Right",
                4 => "Flank Right",
                _ => $"Lane {laneIndex + 1}"
            };
        }
    }
}

using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

using Unity.Collections;

namespace _Modules.GE_Voxel.Utils
{
    public class GE_Math
    {
        struct GE_Bytes3
        {
            private byte x;
            private byte y;
            private byte z;
        }

        public static Vector3 hash3( Vector2 p ) // check this for more details -> https://www.shadertoy.com/view/4fGcWd
        {
            Vector3 q = new Vector3( Vector2.Dot(p,new Vector2(127.1f,311.7f)), 
                Vector2.Dot(p,new Vector2(269.5f, 183.3f)), 
                Vector2.Dot(p,new Vector2(419.2f,371.9f)));

            return new Vector3(
                math.frac(math.sin(q.X) * 43758.5453f),
                math.frac(math.sin(q.Y) * 43758.5453f),
                math.frac(math.sin(q.Z) * 43758.5453f)
            );
        }

        /*
         * Check GE_VoxelChunk to see how integrate this at BurstCompile
         */
        public static float Voronoise( in Vector2 p, float u, float v )
        {
            float k = 1.0f + 63.0f * (float)math.pow(1.0-v,6.0);

            Vector2 i = new Vector2((float)math.floor(p.X), (float)math.floor(p.Y));
            Vector2 f = new Vector2(math.frac(p.X), math.frac(p.Y)); // place add frac with vec2 :(
        
            Vector2 a = Vector2.Zero;
            for( int y=-2; y<=2; y++ )
                for( int x=-2; x<=2; x++ )
                {
                    Vector2  g = new Vector2( x, y );
                    Vector3  o = hash3( i + g )*new Vector3(u,u,1.0f);
                    Vector2  d = g - f + new Vector2(o.X,o.Y);
                    float w = math.pow( 1.0f - math.smoothstep(0.0f,1.414f, d.Length()), k); // (float)d.Length()), 
                    a += new Vector2(o.Z*w,w);
                }
            
            return a.X/a.Y;
        }


        public static bool IsRayIntersectingAABB(float3 rayOrigin, float3 rayDirection, float3 boxCenter, float3 boxSize)
        {
            float3 boxHalfSize = boxSize * 0.5f;

            float3 boxMin = boxCenter - boxHalfSize;
            float3 boxMax = boxCenter + boxHalfSize;

            float tmin = float.MinValue;
            float tmax = float.MaxValue;

            // Vérifier chaque dimension
            for (int i = 0; i < 3; i++)
            {
                if (rayDirection[i] == 0) // Rayon parallèle à cet axe
                {
                    if (rayOrigin[i] < boxMin[i] || rayOrigin[i] > boxMax[i])
                        return false; // Le rayon est en dehors de la boîte
                }
                else
                {
                    float t1 = (boxMin[i] - rayOrigin[i]) / rayDirection[i];
                    float t2 = (boxMax[i] - rayOrigin[i]) / rayDirection[i];

                    if (t1 > t2) Swap(ref t1, ref t2);

                    tmin = Mathf.Max(tmin, t1);
                    tmax = Mathf.Min(tmax, t2);

                    if (tmin > tmax)
                        return false;
                }
            }

            // Si nécessaire, exclure les intersections derrière l'origine du rayon
            return tmin >= 0;
        }

// Méthode d'échange pour t1 et t2
        static void Swap(ref float a, ref float b)
        {
            float temp = a;
            a = b;
            b = temp;
        }

    }
}
using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

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
                math.frac(Mathf.Sin(q.X) * 43758.5453f),
                math.frac(Mathf.Sin(q.Y) * 43758.5453f),
                math.frac(Mathf.Sin(q.Z) * 43758.5453f)
            );
        }


        public static float Voronoise( in Vector2 p, float u, float v )
        {
            float k = 1.0f + 63.0f * (float)Math.Pow(1.0-v,6.0);

            Vector2 i = new Vector2((float)Math.Floor(p.X), (float)Math.Floor(p.Y));
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


    }
}
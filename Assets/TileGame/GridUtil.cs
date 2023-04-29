using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TileGame {
    public static class GridUtil {

        public static void Round(ref this Vector2 v) {
            v.x = v.x < -0.001f ? v.x : 0.001f < v.x ? v.x : 0;
            v.y = v.y < -0.001f ? v.y : 0.001f < v.y ? v.y : 0;
        }

        public static void SetXMax(ref this Vector2 v, float value) {
            if (v.x < value) {
                v.x = value;
            }
        }

        public static void SetYMax(ref this Vector2 v, float value) {
            if (v.y < value) {
                v.y = value;
            }
        }

        public static void SetXMin(ref this Vector2 v, float value) {
            if (value < v.x) {
                v.x = value;
            }
        }

        public static void SetYMin(ref this Vector2 v, float value) {
            if (value < v.y) {
                v.y = value;
            }
        }

        public static Vector2Int ToVector2Int(this Vector2 vector2) => new Vector2Int((int) vector2.x, (int) vector2.y);

       
       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextGridUpRight(ref this Vector2Int currentGrid, ref Vector2 positionInGrid, float yRate) {
            var dy = 1 - positionInGrid.x;
            var remainY = yRate - positionInGrid.y;
            if (dy < remainY) {
                positionInGrid.y += dy;
                positionInGrid.x = 0;
                ++currentGrid.x;
            }
            else {
                positionInGrid.x += remainY;
                positionInGrid.y = 0;
                ++currentGrid.y;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextGridDownRight(ref this Vector2Int currentGrid, ref Vector2 positionInGrid, float yRate) {
            var dy = (positionInGrid.x - 1);
            if (-dy < positionInGrid.y) {
                positionInGrid.y += dy;
                positionInGrid.x = 0;
                ++currentGrid.x;
            }
            else {
                positionInGrid.x += positionInGrid.y;
                positionInGrid.y = yRate;
                --currentGrid.y;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextGridUpLeft(ref this Vector2Int currentGrid, ref Vector2 positionInGrid, float yRate) {
            var dy = positionInGrid.x;
            if (dy < 1 - positionInGrid.y) {
                positionInGrid.y += dy;
                positionInGrid.x = 1;
                --currentGrid.x;
            }
            else {
                positionInGrid.x -= yRate - positionInGrid.y;
                positionInGrid.y = 0;
                ++currentGrid.y;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextGridDownLeft(ref this Vector2Int currentGrid, ref Vector2 positionInGrid, float yRate) {
            var dy = -positionInGrid.x;
            if (-dy < positionInGrid.y) {
                positionInGrid.y += dy;
                positionInGrid.x = 1;
                --currentGrid.x;
            }
            else {
                positionInGrid.x -= (positionInGrid.y);
                positionInGrid.y = yRate;
                --currentGrid.y;
            }
        }

        private const float PADDING = 0.005f;

        public const float SQRT2 = 1.4142135623730951f;
        public const float SQRT1_2 = 0.707106781186547f;

//         public static bool SweepAABBHalfHorizontal(AABB aabb, Vector2 v, out Vector2 outVel) {
//            
//             float hitTime = 0.0f;
//             float outTime = 1.0f;
//             float r = aabb.Height ; //radius to Line
//             float boxProj =(0.5f - aabb.Center.y); //projected Line distance to N
//             float velProj = v.y; //projected Velocity to N
//
//             if(velProj < 0f) r *= -1f;
//             outVel = v;
//             hitTime = Mathf.Max( (boxProj - r ) / velProj, hitTime);
//             outTime = Mathf.Min( (boxProj + r ) / velProj, outTime);
//             // X axis overlap
//             if( v.x < 0 ) //Sweeping left
//             {
//                 if( aabb.Max.x < 0 ) return false;
//                 hitTime = Mathf.Max( (0.5f - aabb.Min.x) / v.x, hitTime);
//                 outTime = Mathf.Min( (0 - aabb.Max.x) / v.x, outTime);
//                 if( hitTime > outTime ) return false;
//             }
//             else if( v.x > 0 ) //Sweeping right
//             {
//                 if( aabb.Min.x > 0.5f ) return false;
//                 hitTime = Mathf.Max( (0 - aabb.Max.x) / v.x, hitTime);
//                 outTime = Mathf.Min( (0.5f - aabb.Min.x) / v.x, outTime);
//                 if( hitTime > outTime ) return false;
//             }
//             else
//             {
//                 if(0 > aabb.Max.x || 0.5f < aabb.Min.x) return false;
//             }
//
//             if( hitTime > outTime ) return false;
//
//             // Y axis overlap
//             if( v.y < 0 ) //Sweeping down
//             {
//                 if( aabb.Max.y < 0.5f ) return false;
//                 hitTime = Mathf.Max( (0.5f - aabb.Min.y) / v.y, hitTime);
//                 outTime = Mathf.Min( (0.5f - aabb.Max.y) / v.y, outTime);
//                 if( hitTime > outTime ) return false;
//             }
//             else if( v.y > 0 ) //Sweeping up
//             {
//                 if( aabb.Min.y > 0.5f ) return false;
//                 hitTime = Mathf.Max( (0.5f - aabb.Max.y) / v.y, hitTime);
//                 outTime = Mathf.Min( (0.5f - aabb.Min.y) / v.y, outTime);
//                 if( hitTime > outTime ) return false;
//             }
//             else
//             {
//                 if(0.5f > aabb.Max.y || 0.5f < aabb.Min.y) return false;
//             }
//             outVel = v * hitTime;
//
//             return true;
//         }
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public static bool SweepAABBUpRight(AABB a,  Vector2 v, out Vector2 outVel  )
//         {
//             
//             //Initialise out info
//             outVel = v;
//
//             float hitTime = 0.0f;
//             float outTime = 1.0f;
//             var normal = new Vector2(-SQRT1_2,SQRT1_2);
//
//             float r = a.Width * SQRT1_2 + a.Height * SQRT1_2; //radius to Line
//             float boxProj = Vector2.Dot(a.Center, normal); //projected Line distance to N
//             float velProj = Vector2.Dot(v, normal); //projected Velocity to N
//
//             if(velProj < 0f) r *= -1f;
//
//             hitTime = Mathf.Max( (boxProj - r ) / velProj, hitTime);
//             outTime = Mathf.Min( (boxProj + r ) / velProj, outTime);
// // X axis overlap
//             if( v.x < 0 ) //Sweeping left
//             {
//                 if( a.Max.x < 0 ) return false;
//                 hitTime = Mathf.Max( (1 - a.Min.x) / v.x, hitTime);
//                 outTime = Mathf.Min( (0 - a.Max.x) / v.x, outTime);
//                 if( hitTime > outTime ) return false;
//             }
//             else if( v.x > 0 ) //Sweeping right
//             {
//                 if( a.Min.x > 1 ) return false;
//                 hitTime = Mathf.Max( (0 - a.Max.x) / v.x, hitTime);
//                 outTime = Mathf.Min( (1 - a.Min.x) / v.x, outTime);
//                 if( hitTime > outTime ) return false;
//             }
//             else
//             {
//                 if(0 > a.Max.x || 1 < a.Min.x) return false;
//             }
//
//             if( hitTime > outTime ) return false;
//
//             // Y axis overlap
//             if( v.y < 0 ) //Sweeping down
//             {
//                 if( a.Max.y < 0 ) return false;
//                 hitTime = Mathf.Max( (1 - a.Min.y) / v.y, hitTime);
//                 outTime = Mathf.Min( (0 - a.Max.y) / v.y, outTime);
//                 if( hitTime > outTime ) return false;
//             }
//             else if( v.y > 0 ) //Sweeping up
//             {
//                 if( a.Min.y > 1 ) return false;
//                 hitTime = Mathf.Max( (0 - a.Max.y) / v.y, hitTime);
//                 outTime = Mathf.Min( (1 - a.Min.y) / v.y, outTime);
//                 if( hitTime > outTime ) return false;
//             }
//             else
//             {
//                 if(0 > a.Max.y || 1 < a.Min.y) return false;
//             }
//             outVel = v * hitTime;
//
//             return true;
//         }[MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public static bool SweepAABBUpLeft( AABB a,  Vector2 v, out Vector2 outVel  )
//         {
//             
//             //Initialise out info
//             outVel = v;
//
//             float hitTime = 0.0f;
//             float outTime = 1.0f;
//             var normal = new Vector2(SQRT1_2,SQRT1_2);
//
//             float r = a.Width * SQRT1_2 + a.Height * SQRT1_2; //radius to Line
//             float boxProj = Vector2.Dot(a.Center, normal); //projected Line distance to N
//             float velProj = Vector2.Dot(v, normal); //projected Velocity to N
//
//             if(velProj < 0f) r *= -1f;
//
//             hitTime = Mathf.Max( (boxProj - r ) / velProj, hitTime);
//             outTime = Mathf.Min( (boxProj + r ) / velProj, outTime);
// // X axis overlap
//             if( v.x < 0 ) //Sweeping left
//             {
//                 if( a.Max.x < 0 ) return false;
//                 hitTime = Mathf.Max( (1 - a.Min.x) / v.x, hitTime);
//                 outTime = Mathf.Min( (0 - a.Max.x) / v.x, outTime);
//                 if( hitTime > outTime ) return false;
//             }
//             else if( v.x > 0 ) //Sweeping right
//             {
//                 if( a.Min.x > 1 ) return false;
//                 hitTime = Mathf.Max( (0 - a.Max.x) / v.x, hitTime);
//                 outTime = Mathf.Min( (1 - a.Min.x) / v.x, outTime);
//                 if( hitTime > outTime ) return false;
//             }
//             else
//             {
//                 if(0 > a.Max.x || 1 < a.Min.x) return false;
//             }
//
//             if( hitTime > outTime ) return false;
//
//             // Y axis overlap
//             if( v.y < 0 ) //Sweeping down
//             {
//                 if( a.Max.y < 0 ) return false;
//                 hitTime = Mathf.Max( (1 - a.Min.y) / v.y, hitTime);
//                 outTime = Mathf.Min( (0 - a.Max.y) / v.y, outTime);
//                 if( hitTime > outTime ) return false;
//             }
//             else if( v.y > 0 ) //Sweeping up
//             {
//                 if( a.Min.y > 1 ) return false;
//                 hitTime = Mathf.Max( (0 - a.Max.y) / v.y, hitTime);
//                 outTime = Mathf.Min( (1 - a.Min.y) / v.y, outTime);
//                 if( hitTime > outTime ) return false;
//             }
//             else
//             {
//                 if(0 > a.Max.y || 1 < a.Min.y) return false;
//             }
//             outVel = v * hitTime;
//
//             return true;
//         }
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public static bool SweepBoxLine( AABB a, Line l, Vector2 v, out Vector2 outVel, out Vector2 hitNormal  )
//         {
//             Vector2 lineDir = l.End - l.Start;
//             Vector2 lineMin = new Vector2(0,0);
//             Vector2 lineMax = new Vector2(0,0);
//
//             if(lineDir.x > 0.0f) //right
//             {
//                 lineMin.x = l.Start.x;
//                 lineMax.x = l.End.x;
//             }
//             else //left
//             {
//                 lineMin.x = l.End.x;
//                 lineMax.x = l.Start.x;
//             }
//
//             if(lineDir.y > 0.0f) //up
//             {
//                 lineMin.y = l.Start.y;
//                 lineMax.y = l.End.y;
//             }
//             else //down
//             {
//                 lineMin.y = l.End.y;
//                 lineMax.y = l.Start.y;
//             }
//
//             var lineAABB = new AABB(lineMin, lineMax);
//             
//             if (!(a+v).Intersects(ref lineAABB)) {
//                 outVel = hitNormal = default;
//                 return false;
//             }
//             //Initialise out info
//             outVel = v;
//             hitNormal = new Vector2(0,0);
//
//             float hitTime = 0.0f;
//             float outTime = 1.0f;
//             var normal = segmentNormal(l.End, l.Start);
//
//             float r = a.Width * Mathf.Abs(normal.x) + a.Height * Mathf.Abs(normal.y); //radius to Line
//             float boxProj = Vector2.Dot(l.Start - a.Center, normal); //projected Line distance to N
//             float velProj = Vector2.Dot(v, normal); //projected Velocity to N
//
//             if(velProj < 0f) r *= -1f;
//
//             hitTime = Math.Max( (boxProj - r ) / velProj, hitTime);
//             outTime = Math.Min( (boxProj + r ) / velProj, outTime);
// // X axis overlap
//             if( v.x < 0 ) //Sweeping left
//             {
//                 if( a.Max.x < lineMin.x ) return false;
//                 hitTime = Mathf.Max( (lineMax.x - a.Min.x) / v.x, hitTime);
//                 outTime = Mathf.Min( (lineMin.x - a.Max.x) / v.x, outTime);
//                 if( hitTime > outTime ) return false;
//                 hitNormal.x = 1;
//             }
//             else if( v.x > 0 ) //Sweeping right
//             {
//                 if( a.Min.x > lineMax.x ) return false;
//                 hitTime = Mathf.Max( (lineMin.x - a.Max.x) / v.x, hitTime);
//                 outTime = Mathf.Min( (lineMax.x - a.Min.x) / v.x, outTime);
//                 if( hitTime > outTime ) return false;
//                 hitNormal.x = -1;
//             }
//             else
//             {
//                 if(lineMin.x > a.Max.x || lineMax.x < a.Min.x) return false;
//             }
//
//             if( hitTime > outTime ) return false;
//
//             // Y axis overlap
//             if( v.y < 0 ) //Sweeping down
//             {
//                 if( a.Max.y < lineMin.y ) return false;
//                 hitTime = Mathf.Max( (lineMax.y - a.Min.y) / v.y, hitTime);
//                 outTime = Mathf.Min( (lineMin.y - a.Max.y) / v.y, outTime);
//                 if( hitTime > outTime ) return false;
//                 hitNormal.y = 1;
//             }
//             else if( v.y > 0 ) //Sweeping up
//             {
//                 if( a.Min.y > lineMax.y ) return false;
//                 hitTime = Mathf.Max( (lineMin.y - a.Max.y) / v.y, hitTime);
//                 outTime = Mathf.Min( (lineMax.y - a.Min.y) / v.y, outTime);
//                 if( hitTime > outTime ) return false;
//                 hitNormal.y = -1;
//             }
//             else
//             {
//                 if(lineMin.y > a.Max.y || lineMax.y < a.Min.y) return false;
//             }
//             outVel = v * hitTime;
//
//             return true;
//         }
    
        public struct ContactData {
            public Vector2 Delta;
            public Vector2 Position;
            public Vector2 Normal;
        }

        public static Vector2 segmentNormal (Vector2 pos1,Vector2 pos2) {
            var dx = pos2.x - pos1.x;
            var dy = pos2.y - pos1.y;

            if (dx != 0)
                dx = -dx;

            var result = new Vector2(dy, dx);   // normals: [ -dy, dx ]  [ dy, -dx ]

            return result.normalized;
        }
    }
}
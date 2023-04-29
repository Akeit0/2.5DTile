using System;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace TileGame {
    public struct TileRay {
        public TileCollision CollisionType;
        public Vector2Int Grid;
        public Vector2Int GridMax;
        public Vector2 PositionInGrid;
        public Vector2 HitPositionInGrid;
        public Vector2 EndInGrid;
        public float DyDx;
        public float DxDy;
        public Vector2 Delta;
        public Vector2 HitNormal;
        public TileRay(Vector2 start, Vector2 end,Vector2Int gridMax) {
            Grid = new Vector2Int((int)(start.x), (int)start.y);
            PositionInGrid = new Vector2(start.x - Grid.x, start.y - Grid.y);
            EndInGrid = new Vector2(end.x - Grid.x, end.y - Grid.y);
            Delta = end - start;
            if (Delta.x == 0) 
                DyDx = 1000000f;
            else
                DyDx = Delta.y / Delta.x;
            if(Delta.y==0)
                DxDy= 1000000f;
            else 
                DxDy = Delta.x / Delta.y;
            CollisionType = TileCollision.None;
            HitPositionInGrid = default;
            GridMax= gridMax;
            HitNormal = default;
        }
        public bool MoveNext() {
            if (Delta.x > 0) 
                return Delta.y > 0 ? MoveNextUpRight() : MoveNextDownRight();
            return Delta.y > 0 ? MoveNextUpLeft() : MoveNextDownLeft();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNextUpRight() {
            var dy = DyDx*(1 - PositionInGrid.x);
            var remainY = 1 - PositionInGrid.y;
            if (dy < remainY) {
                EndInGrid.x -= 1f;
                PositionInGrid.x = 0;
                PositionInGrid.y +=dy;
                ++Grid.x;
                return Grid.x<GridMax.x&&0<=EndInGrid.x;
            }
            else {
                EndInGrid.y-=1f;
                PositionInGrid.x += remainY*DxDy;
                PositionInGrid.y =0;
                ++Grid.y;
                return 0<=Grid.y&&0<=EndInGrid.y;
            }
        }
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNextDownRight() {
            var dy = DyDx*(1 - PositionInGrid.x);
            if (-dy < PositionInGrid.y) {
                EndInGrid.x -= 1f;
                PositionInGrid.x = 0;
                PositionInGrid.y +=dy;
                ++Grid.x;
                return Grid.x<GridMax.x&&0<=EndInGrid.x;
            }
            else {
                EndInGrid.y+=1f;
                PositionInGrid.x -= PositionInGrid.y*DxDy;
                PositionInGrid.y =1;
                --Grid.y;
                return 0<=Grid.y&&EndInGrid.y<1;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        
        public bool  MoveNextUpLeft() {
            var dy = -DyDx*PositionInGrid.x;
            if (dy < 1 - PositionInGrid.y) {
                EndInGrid.x += 1f;
                PositionInGrid.x = 1;
                PositionInGrid.y +=dy;
                --Grid.x;
                return 0<=Grid.x&&EndInGrid.x<1;
            }
            else {
                EndInGrid.y-=1f;
                PositionInGrid.x += (1 - PositionInGrid.y)*DxDy;
                PositionInGrid.y =0;
                ++Grid.y;
                return Grid.y<GridMax.y&&0<=EndInGrid.y;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNextDownLeft() {
            var dy = -DyDx*PositionInGrid.x;
            if (-dy < PositionInGrid.y) {
                EndInGrid.x += 1f;
                PositionInGrid.x = 1;
                PositionInGrid.y +=dy;
                --Grid.x;
                return 0<=Grid.x&&EndInGrid.x<1;
            }
            else {
                EndInGrid.y+=1f;
                PositionInGrid.x -= PositionInGrid.y*DxDy;
                PositionInGrid.y =1;
                --Grid.y;
                return 0<=Grid.y&&EndInGrid.y<1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public  bool TryCollide() {
            switch (CollisionType) {
                case TileCollision.None:
                    return false;
                case TileCollision.Filled:
                    HitPositionInGrid = PositionInGrid;
                    return true;
                case TileCollision.BL:
                    if (PositionInGrid.x+PositionInGrid.y <=1) {
                        HitPositionInGrid = PositionInGrid;
                        return true;
                    }
                    if(EndInGrid.x+EndInGrid.y <=1) {
                        if(LineSegmentsIntersectionBackSlash(out HitPositionInGrid)){
                            return true;
                        }
                        return false;
                    }
                    return false;
                case TileCollision.BR:
                    if (PositionInGrid.x-PositionInGrid.y >=0) {
                        HitPositionInGrid = PositionInGrid;
                        return true;
                    }
                    if(EndInGrid.x-EndInGrid.y >=0) {
                        if(LineSegmentsIntersectionSlash(out HitPositionInGrid)){
                            return true;
                        }
                        return false;
                    }
                    return false;
                case TileCollision.TL:
                    if (PositionInGrid.x-PositionInGrid.y <=0) {
                        HitPositionInGrid = PositionInGrid;
                        return true;
                    }
                    if(EndInGrid.x-EndInGrid.y <=0) {
                        if(LineSegmentsIntersectionSlash(out HitPositionInGrid)){
                            return true;
                        }
                        return false;
                    }
                    return false;
                case TileCollision.TR:
                    if (PositionInGrid.x+PositionInGrid.y >=1) {
                        HitPositionInGrid = PositionInGrid;
                        return true;
                    }
                    if(EndInGrid.x+EndInGrid.y >=1) {
                        if(LineSegmentsIntersectionBackSlash(out HitPositionInGrid)){
                            return true;
                        }
                        return false;
                    }
                    return false;
                default:return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCollideWithNormal() {
            switch (CollisionType) {
                case TileCollision.None:
                    return false;
                case TileCollision.Filled:
                    HitPositionInGrid = PositionInGrid;
                    if (PositionInGrid.x == 0) {
                        HitNormal = new Vector2(-1, 0);
                        return true;
                    }if (PositionInGrid.x == 1) {
                        HitNormal = new Vector2(1, 0);
                        return true;
                    }if (PositionInGrid.y == 1) {
                        HitNormal = new Vector2(0, 1);
                        return true;
                    }
                    HitNormal = new Vector2(0, -1);
                    return true;
                case TileCollision.BL:
                    if (PositionInGrid.x + PositionInGrid.y <= 1) {
                        HitPositionInGrid = PositionInGrid;
                        if (PositionInGrid.x <PositionInGrid.y) {
                            HitNormal = new Vector2(-1, 0);
                            return true;
                        }
                        HitNormal = new Vector2(0, -1);
                        return true;
                    }

                    if(EndInGrid.x+EndInGrid.y <=1) {
                        if(LineSegmentsIntersectionBackSlash(out HitPositionInGrid)){
                            HitNormal = new Vector2(1.414f,1.414f);
                            return true;
                        }
                        return false;
                    }
                    return false;
                case TileCollision.BR:
                    if (PositionInGrid.x-PositionInGrid.y >=0) {
                        HitPositionInGrid = PositionInGrid;
                        if (1f<PositionInGrid.x+ PositionInGrid.y) {
                            HitNormal = new Vector2(1, 0);
                            return true;
                        }
                        HitNormal = new Vector2(0, -1);
                        return true;
                    }
                    if(EndInGrid.x-EndInGrid.y >=0) {
                        if(LineSegmentsIntersectionSlash(out HitPositionInGrid)){
                            HitNormal = new Vector2(-1.414f,1.414f);
                            return true;
                        }
                        return false;
                    }
                    return false;
                case TileCollision.TL:
                    if (PositionInGrid.x-PositionInGrid.y <=0) {
                        HitPositionInGrid = PositionInGrid;
                        if (PositionInGrid.x +PositionInGrid.y<1f) {
                            HitNormal = new Vector2(-1, 0);
                            return true;
                        }
                        HitNormal = new Vector2(0, 1);
                        return true;
                    }
                    if(EndInGrid.x-EndInGrid.y <=0) {
                        if(LineSegmentsIntersectionSlash(out HitPositionInGrid)){
                            HitNormal = new Vector2(1.414f, -1.414f);
                            return true;
                        }
                        return false;
                    }
                    return false;
                case TileCollision.TR:
                    if (PositionInGrid.x+PositionInGrid.y >=1) {
                        HitPositionInGrid = PositionInGrid;
                        if (PositionInGrid.x <PositionInGrid.y) {
                            HitNormal = new Vector2(0, 1);
                            return true;
                        }
                        HitNormal = new Vector2(1, 0);
                        return true;
                    }
                    if(EndInGrid.x+EndInGrid.y >=1) {
                        if(LineSegmentsIntersectionBackSlash(out HitPositionInGrid)){
                            HitNormal = new Vector2(-1.414f, -1.414f);
                            return true;
                        }
                        return false;
                    }
                    return false;
                default:return false;
            }
        } 
        
        public  bool LineSegmentsIntersection(Vector2 p3, Vector2 p4, out Vector2 intersection)
        {
            intersection = Vector2.zero;
            var d = (PositionInGrid.x - EndInGrid.x) * (p4.y - p3.y) - (PositionInGrid.y - EndInGrid.y) * (p4.x - p3.x);

            if (d == 0.0f)
            {
                return false;
            }
            var u = ((p3.x - EndInGrid.x) * (p4.y - p3.y) - (p3.y - EndInGrid.y) * (p4.x - p3.x)) / d;
            var v = ((p3.x - EndInGrid.x) * (PositionInGrid.y - EndInGrid.y) - (p3.y - EndInGrid.y) * (PositionInGrid.x - EndInGrid.x)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
            {
                return false;
            }
            intersection.x = EndInGrid.x + u * (PositionInGrid.x - EndInGrid.x);
            intersection.y = EndInGrid.y + u * (PositionInGrid.y - EndInGrid.y);
            return true;
        }
        public  bool LineSegmentsIntersectionBackSlash(out Vector2 intersection)
        {
            intersection = default;
            var dx = PositionInGrid.x - EndInGrid.x;
            var dy= PositionInGrid.y - EndInGrid.y;
            var d =-dx -dy;

            if (d == 0.0f)
            {
                return false;
            }
            var u = ( EndInGrid.x - (1f - EndInGrid.y)) / d;
            var v = ( - EndInGrid.x * dy - (1f - EndInGrid.y) * dx) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
            {
                return false;
            }
            intersection.x = EndInGrid.x + u * dx;
            intersection.y = EndInGrid.y + u * dy;
            return true;
        }
        
        public  bool LineSegmentsIntersectionSlash( out Vector2 intersection)
        {
            intersection = default;
            var dx = PositionInGrid.x - EndInGrid.x;
            var dy= PositionInGrid.y - EndInGrid.y;
            var d = dx  - dy ;
            if (d == 0.0f)
            {
                return false;
            }
            var u = (- EndInGrid.x + EndInGrid.y) / d;
            var v = ( - EndInGrid.x *dy + EndInGrid.y * dx) / d;
            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
            {
                return false;
            }
            intersection.x = EndInGrid.x + u * dx;
            intersection.y = EndInGrid.y + u * dy;
            return true;
        }
        
        
    }
}
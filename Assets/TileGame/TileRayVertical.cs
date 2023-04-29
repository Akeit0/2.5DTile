
using System.Runtime.CompilerServices;
using UnityEngine;
namespace TileGame {
    public struct TileRayUp {
        public TileCollision CollisionType;
        public Vector2Int Grid;
        public Vector2Int GridMax;
        public Vector2 PositionInGrid;
        public Vector2 HitPositionInGrid;
        public Vector2 EndInGrid;
        public Vector2 HitNormal;
        public float MaxX;

        public TileRayUp(Vector2 start, float deltaY, Vector2Int gridMax) {
            Grid = new Vector2Int((int) (start.x), (int) start.y);
            PositionInGrid = new Vector2(start.x - Grid.x, start.y - Grid.y);
            EndInGrid = new Vector2(start.x  - Grid.x, start.y+deltaY - Grid.y);
            CollisionType = TileCollision.None;
            HitPositionInGrid = default;
            GridMax = gridMax;
            HitNormal = default;
            MaxX=PositionInGrid.x+0.3f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() {
            EndInGrid.y -= 1f;
            PositionInGrid.y = 0;
            ++Grid.y;
            return Grid.y < GridMax.y && 0 <= EndInGrid.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCollideWithNormal() {
            switch (CollisionType) {
                case TileCollision.None:
                    return false;
                case TileCollision.Filled:
                    HitPositionInGrid = PositionInGrid;
                    HitNormal = new Vector2(0, -1);
                    return true;
                case TileCollision.BL:
                    if (PositionInGrid.x + PositionInGrid.y <= 1) {
                        HitPositionInGrid = PositionInGrid;
                        HitNormal = new Vector2(0,-1);
                        return true;
                    }
                    return false;
                case TileCollision.TL:
                    if (EndInGrid.x < EndInGrid.y) {
                        HitPositionInGrid = new Vector2(PositionInGrid.x, PositionInGrid.x);
                        HitNormal = new Vector2(1.414f,-1.414f);
                        return true;
                    }
                    return false;
                case TileCollision.BR:
                    if (PositionInGrid.x > PositionInGrid.y) {
                        HitPositionInGrid = PositionInGrid;
                        HitNormal = new Vector2(0,-1);
                        return true;
                    }
                    return false;
                case TileCollision.TR:
                    if (EndInGrid.x + EndInGrid.y >= 1) {
                        HitPositionInGrid = new Vector2(PositionInGrid.x,1- PositionInGrid.x);
                        HitNormal = new Vector2(-1.414f, -1.414f);
                        return true;
                    }

                    return false;
                default: return false;
            }
        }
    }

    public struct TileRayDown {
        public TileCollision CollisionType;
        public Vector2Int Grid;
        public Vector2 PositionInGrid;
        public Vector2 HitPositionInGrid;
        public Vector2 EndInGrid;
        public Vector2 HitNormal;
        public float MaxX;
        public TileRayDown(Vector2 start, float deltaY) {
            Grid = new Vector2Int((int)(start.x), (int)start.y);
            PositionInGrid = new Vector2(start.x - Grid.x, start.y - Grid.y);
            EndInGrid = new Vector2(start.x - Grid.x, start.y+deltaY - Grid.y);
            CollisionType = TileCollision.None;
            HitPositionInGrid = default;
            HitNormal = default;
            MaxX=PositionInGrid.x+0.3f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() {
            EndInGrid.y += 1f;
            PositionInGrid.y = 1;
            --Grid.y;
            return 0<=Grid.y&&EndInGrid.y<1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCollideWithNormal() {
            switch (CollisionType) {
                case TileCollision.None:
                    return false;
                case TileCollision.Filled:
                    HitPositionInGrid = PositionInGrid;
                    HitNormal = new Vector2(0, 1);
                    return true;
                case TileCollision.BL:
                    if (EndInGrid.x+EndInGrid.y <1f) {
                        HitPositionInGrid = new Vector2(PositionInGrid.x,1- PositionInGrid.x);
                        HitNormal = new Vector2(1.414f,1.414f);
                        return true;
                    }
                    return false;
                    
                case TileCollision.TL:
                    if (PositionInGrid.x + PositionInGrid.y <= 1) {
                        HitPositionInGrid = PositionInGrid;
                        HitNormal = new Vector2(0,-1);
                        return true;
                    }
                    return false;
                case TileCollision.BR:
                    if (EndInGrid.x > EndInGrid.y) {
                        HitPositionInGrid = new Vector2(PositionInGrid.x,PositionInGrid.x);
                        HitNormal = new Vector2(-1.414f, 1.414f);
                        return true;
                    }

                    return false;
                case TileCollision.TR:
                    if (PositionInGrid.x > PositionInGrid.y) {
                        HitPositionInGrid = PositionInGrid;
                        HitNormal = new Vector2(0,-1);
                        return true;
                    }
                    return false;
                default: return false;
            }
        } 
    }
}
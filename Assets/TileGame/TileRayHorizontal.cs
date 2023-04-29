
using System.Runtime.CompilerServices;
using UnityEngine;
namespace TileGame {
    public struct TileRayRight {
        public TileCollision CollisionType;
        public Vector2Int Grid;
        public Vector2Int GridMax;
        public Vector2 PositionInGrid;
        public Vector2 HitPositionInGrid;
        public Vector2 EndInGrid;
        public Vector2 HitNormal;
        public float MaxY;

        public TileRayRight(Vector2 start, float deltaX, Vector2Int gridMax) {
            Grid = new Vector2Int((int) (start.x), (int) start.y);
            PositionInGrid = new Vector2(start.x - Grid.x, start.y - Grid.y);
            EndInGrid = new Vector2(start.x + deltaX - Grid.x, start.y - Grid.y);
            CollisionType = TileCollision.None;
            HitPositionInGrid = default;
            GridMax = gridMax;
            HitNormal = default;
            MaxY=PositionInGrid.y+0.3f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() {
            EndInGrid.x -= 1f;
            PositionInGrid.x = 0;
            ++Grid.x;
            return Grid.x < GridMax.x && 0 <= EndInGrid.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCollideWithNormal() {
            switch (CollisionType) {
                case TileCollision.None:
                    return false;
                case TileCollision.Filled:
                    HitPositionInGrid = PositionInGrid;
                    HitNormal = new Vector2(-1, 0);
                    return true;
                case TileCollision.BL:
                    if (EndInGrid.x + EndInGrid.y <= 1) {
                        HitPositionInGrid = PositionInGrid;
                        HitNormal = new Vector2(-1, 0);
                        return true;
                    }
                    return false;
                case TileCollision.TL:
                    if (PositionInGrid.x <PositionInGrid.y ) {
                        HitNormal = new Vector2(-1, 0);
                        HitPositionInGrid = PositionInGrid;
                        return true;
                    }

                    return false;
                case TileCollision.BR:
                    if (EndInGrid.x > EndInGrid.y) {
                        HitNormal = new Vector2(-1.414f, 1.414f);
                        HitPositionInGrid = new Vector2(PositionInGrid.y, PositionInGrid.y);
                        return true;
                    }

                    return false;
                case TileCollision.TR:
                    if (1f < MaxY) {
                        HitNormal = new Vector2(1,0);
                        HitPositionInGrid =PositionInGrid;
                        return true;
                    }
                    if (EndInGrid.x + EndInGrid.y >= 1) {
                        HitPositionInGrid = new Vector2(1f-PositionInGrid.y, PositionInGrid.y);
                        HitNormal = new Vector2(-1.414f, -1.414f);
                        return true;
                    }

                    return false;
                default: return false;
            }
        }
    }

    public struct TileRayLeft {
        public TileCollision CollisionType;
        public Vector2Int Grid;
        public Vector2 PositionInGrid;
        public Vector2 HitPositionInGrid;
        public Vector2 EndInGrid;
        public Vector2 HitNormal;
        public float MaxY;
        public TileRayLeft(Vector2 start, float deltaX) {
            Grid = new Vector2Int((int)(start.x), (int)start.y);
            PositionInGrid = new Vector2(start.x - Grid.x, start.y - Grid.y);
            EndInGrid = new Vector2(start.x+deltaX - Grid.x, start.y - Grid.y);
            CollisionType = TileCollision.None;
            HitPositionInGrid = default;
            HitNormal = default;
            MaxY=PositionInGrid.y+0.3f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() {
            EndInGrid.x += 1f;
            PositionInGrid.x = 1;
            --Grid.x;
            return 0<=Grid.x&&EndInGrid.x<1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCollideWithNormal() {
            switch (CollisionType) {
                case TileCollision.None:
                    return false;
                case TileCollision.Filled: HitPositionInGrid = PositionInGrid;
                    HitNormal = new Vector2(1, 0);
                    return true;
                case TileCollision.BR:
                    if(EndInGrid.x>EndInGrid.y) {
                        HitNormal = new Vector2(1,0);
                        HitPositionInGrid = PositionInGrid;
                        return true;
                    }
                    return false;
                case TileCollision.TR:
                    if(PositionInGrid.x+PositionInGrid.y >1f) {
                        HitNormal = new Vector2(1,0);
                        HitPositionInGrid =PositionInGrid;
                        return true;
                    }
                    return false;
                
                case TileCollision.BL:
                    if(EndInGrid.x+EndInGrid.y <1f) {
                        HitNormal = new Vector2(1.414f,1.414f);
                        HitPositionInGrid = new Vector2(1f-PositionInGrid.y, PositionInGrid.y);
                        return true;
                    }
                    return false;
                case TileCollision.TL:
                    if (1f < MaxY) {
                        HitNormal = new Vector2(1,0);
                        HitPositionInGrid =PositionInGrid;
                        return true;
                    }
                    if(EndInGrid.x<EndInGrid.y) {
                        HitNormal = new Vector2(1.414f, -1.414f);
                        HitPositionInGrid = new Vector2(PositionInGrid.y, PositionInGrid.y);
                        return true;
                    }
                    return false;
                default:return false;
            }
        } 
    }
}
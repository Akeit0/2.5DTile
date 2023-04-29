using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
namespace TileGame {
    
    public enum TileCollision :byte{
        None=0,
        Filled,
        BL,BR,TL,TR
    }
    public struct Vert4 {
        public Vector2 BottomLeft;
        public Vector2 BottomRight;
        public Vector2 TopRight;
        public Vector2 TopLeft;
        public Vert4(Vector2 bottomLeft, Vector2 bottomRight, Vector2 topRight, Vector2 topLight) {
            BottomLeft=bottomLeft;BottomRight=bottomRight;TopRight=topRight; TopLeft = topLight;
        }
        public static Vert4 Box(Vector2 bottomLeft) => new Vert4(bottomLeft, bottomLeft + new Vector2(1, 0),
            bottomLeft + new Vector2(1, 1), bottomLeft + new Vector2(0, 1));
        public static Vert4 Box(float bottomLeftX,float bottomLeftY) => new Vert4(new Vector2(bottomLeftX,bottomLeftY), new Vector2(bottomLeftX+1, bottomLeftY),
             new Vector2(bottomLeftX+1,bottomLeftY+ 1),  new Vector2(bottomLeftX, bottomLeftY+1));
        public void SetRight(float x) {
            BottomRight.x = x;
            TopRight.x = x;
        }
        public void SetLeft(float x) {
            BottomLeft.x = x;
            TopLeft.x = x;
        }
        public int2 GetGrid() {
            return new int2((int)BottomLeft.x,(int)BottomLeft.y);
        }

        public int GetIntWidth() => (int) BottomRight.x - (int) BottomLeft.x;
    }
    public struct Vert3 {
        public Vector2 First;
        public Vector2 Second;
        public Vector2 Third;
        public Vert3(Vector2 first, Vector2 second, Vector2 third) {
            First=first;Second=second;Third=third;
        }

        public static Vert3 Get(float bottomLeftX, float bottomLeftY, TileCollision collision) {
            switch (collision) {
                case TileCollision.BL: return BottomLeft(bottomLeftX, bottomLeftY);
                case TileCollision.BR: return BottomRight(bottomLeftX, bottomLeftY);
                case TileCollision.TL: return TopLeft(bottomLeftX, bottomLeftY);
                case TileCollision.TR: return TopRight(bottomLeftX, bottomLeftY);
                default:
                    throw new ArgumentOutOfRangeException(nameof(collision), collision, null);
            }
        }
        public static Vert3 BottomRight(float bottomLeftX, float bottomLeftY)
            => new Vert3(new Vector2(bottomLeftX, bottomLeftY), new Vector2(bottomLeftX + 1, bottomLeftY),
                new Vector2(bottomLeftX+1, bottomLeftY+1));
        public static Vert3 BottomLeft(float bottomLeftX, float bottomLeftY)
            => new Vert3(new Vector2(bottomLeftX, bottomLeftY), new Vector2(bottomLeftX + 1, bottomLeftY),
                new Vector2(bottomLeftX, bottomLeftY+1));
        public static Vert3 TopRight(float bottomLeftX, float bottomLeftY)
            => new Vert3(new Vector2(bottomLeftX+1, bottomLeftY), new Vector2(bottomLeftX , bottomLeftY+1),
                new Vector2(bottomLeftX+ 1, bottomLeftY+1));
        public static Vert3 TopLeft(float bottomLeftX, float bottomLeftY)
            => new Vert3(new Vector2(bottomLeftX, bottomLeftY), new Vector2(bottomLeftX, bottomLeftY+1),
                new Vector2(bottomLeftX + 1, bottomLeftY+1));

        public int2 GetGrid() {
            var min = Vector2.Min(Vector2.Min(First, Second), Third);
            return new int2((int)min.x,(int)min.y);
        }
    }


    public struct ChunkDataBox :IDisposable{
        readonly CustomCollider2D collider2D;
        public NativeList<PhysicsShape2D> Shape2Ds;
        public NativeList<Vert4> Vertices;
        private int _cacheCount ;
        public int Length => Shape2Ds.Length-_cacheCount;
        public int CacheCount => _cacheCount;
        public ChunkDataBox(CustomCollider2D collider2D, int capacity) {
            this.collider2D = collider2D;
            Shape2Ds = new NativeList<PhysicsShape2D>(capacity,Allocator.Persistent);
            Vertices = new NativeList<Vert4>(capacity,Allocator.Persistent);
            _cacheCount = 0;
        }

        public void Apply(int index) {
            collider2D.SetCustomShape(Shape2Ds.AsArray(), Vertices.AsArray().Reinterpret<Vector2>(32),index,index);
        }
        public void Apply() {
            collider2D.SetCustomShapes(Shape2Ds.AsArray(), Vertices.AsArray().Reinterpret<Vector2>(32));
        }
        public void SetRight(int index, float x) {
            Vertices.ElementAt(index).SetRight(x);
            collider2D.SetCustomShape(Shape2Ds.AsArray(), Vertices.AsArray().Reinterpret<Vector2>(32),index,index);
        } 
        public void SetLeft(int index, float x) {
            Vertices.ElementAt(index).SetLeft(x);
            collider2D.SetCustomShape(Shape2Ds.AsArray(), Vertices.AsArray().Reinterpret<Vector2>(32),index,index);
        }
        public void Add(int x, int y) {
            if(_cacheCount==0) {
                Shape2Ds.Add(GetShape(Vertices.Length * 4));
                Vertices.Add(Vert4.Box(x, y));
                collider2D.SetCustomShapes(Shape2Ds.AsArray(), Vertices.AsArray().Reinterpret<Vector2>(32));
                return;
            }
            var cacheIndex = Vertices.Length - _cacheCount;
            Vertices[cacheIndex]=Vert4.Box(x, y);
            collider2D.SetCustomShape(Shape2Ds.AsArray(), Vertices.AsArray().Reinterpret<Vector2>(32),cacheIndex,cacheIndex);
            --_cacheCount;
        }  public void SetRightTemp(int index, float x) {
            Vertices.ElementAt(index).SetRight(x);
        } 
        public void SetLeftTemp(int index, float x) {
            Vertices.ElementAt(index).SetLeft(x);
        }
        public void AddTemp(int x, int y) {
            if(_cacheCount==0) {
                Shape2Ds.Add(GetShape(Vertices.Length * 4));
                Vertices.Add(Vert4.Box(x, y));
                return;
            }
            var cacheIndex = Vertices.Length - _cacheCount;
            Vertices[cacheIndex]=Vert4.Box(x, y);
            --_cacheCount;
        } 
        public int InsertInLineAndGetRightWidth(int leftIndex, int rightIndex) {
            Vertices.ElementAt(leftIndex).SetRight(Vertices[rightIndex].BottomRight.x);
            var rightWidth = Vertices[rightIndex].GetIntWidth();
            
            ++_cacheCount;
            var sArray = Shape2Ds.AsArray();
            var vArray = Vertices.AsArray().Reinterpret<Vector2>(32);
            var last = sArray.Length -_cacheCount;
            if (last == rightIndex) {
                Vertices[rightIndex]=Vert4.Box(0,-1000);
                collider2D.SetCustomShape(sArray, vArray,rightIndex,rightIndex);
                return rightWidth;
            }
            return 0;
        }
        public int RemoveInLineAndGetRightWidth(int index, float x) {
            var target=Vertices[index];
            target.SetLeft(x+1);
            Vertices.ElementAt(index).SetRight(x);
            if (_cacheCount == 0) {
                Shape2Ds.Add(GetShape(Vertices.Length*4));
                Vertices.Add(target);
                collider2D.SetCustomShapes(Shape2Ds.AsArray(), Vertices.AsArray().Reinterpret<Vector2>(32));
                return target.GetIntWidth();
            }else {
                var cacheIndex = Vertices.Length - _cacheCount;
                Vertices[cacheIndex]=target;
                var sArray = Shape2Ds.AsArray();
                var vArray = Vertices.AsArray().Reinterpret<Vector2>(32);
                collider2D.SetCustomShape(sArray,vArray,index,index);
                collider2D.SetCustomShape(sArray,vArray,cacheIndex,cacheIndex);
                --_cacheCount;
                return target.GetIntWidth();
            }
           
        }
        public bool RemoveAtSwapBack(int index) {
            ++_cacheCount;
            var sArray = Shape2Ds.AsArray();
            var vArray = Vertices.AsArray().Reinterpret<Vector2>(32);
            var last = sArray.Length -_cacheCount;
            if (last == index) {
                Vertices[index]=Vert4.Box(0,-1000);
                collider2D.SetCustomShape(sArray, vArray,index,index);
                return false;
            }
            if (last < index) {
                --_cacheCount;
                throw new ArgumentOutOfRangeException($"last={last}, index={index}");
            }
            Vertices.Copy(index,last);
            Vertices[last]=Vert4.Box(0,-1000);
            collider2D.SetCustomShape(sArray,vArray,index,index);
            collider2D.SetCustomShape(sArray,vArray,last,last);
            return true;
        }
        private static PhysicsShape2D GetShape(int vertexStart) => new PhysicsShape2D() 
            {shapeType = PhysicsShapeType2D.Polygon, radius = 0, vertexStartIndex = vertexStart, vertexCount = 4};

        public void Clear() {
            Shape2Ds.Clear();
            Vertices.Clear();
            _cacheCount = 0;
        }
        public void Dispose() {
            Shape2Ds.DisposeIfCreated();
            Vertices.DisposeIfCreated();
        }
    }
    public struct ChunkDataTriangle :IDisposable{
        readonly CustomCollider2D collider2D;
        public NativeList<PhysicsShape2D> Shape2Ds;
        public NativeList<Vert3> Vertices;
        private int _cacheCount ;
        public int CacheCount => _cacheCount;
        public int Length => Shape2Ds.Length-_cacheCount;
        public ChunkDataTriangle(CustomCollider2D collider2D, int capacity) {
            this.collider2D = collider2D;
            Shape2Ds = new NativeList<PhysicsShape2D>(capacity,Allocator.Persistent);
            Vertices = new NativeList<Vert3>(capacity,Allocator.Persistent);
            _cacheCount = 0;
        }
        public void Add(int x, int y,TileCollision collision) {
            if(_cacheCount==0) {
                Shape2Ds.Add(GetShape(Vertices.Length * 3));
                Vertices.Add(Vert3.Get(x, y,collision));
                collider2D.SetCustomShapes(Shape2Ds.AsArray(), Vertices.AsArray().Reinterpret<Vector2>(24));
                return;
            }
            var index = Vertices.Length - _cacheCount;
            Vertices[index]=Vert3.Get(x, y,collision);
            var sArray = Shape2Ds.AsArray();
            var vArray = Vertices.AsArray().Reinterpret<Vector2>(24);
            collider2D.SetCustomShape(sArray,vArray,index,index);
            --_cacheCount;
        }
        public void AddTemp(int x, int y,TileCollision collision) {
            if(_cacheCount==0) {
                Shape2Ds.Add(GetShape(Vertices.Length * 3));
                Vertices.Add(Vert3.Get(x, y,collision));
                return;
            }
            var index = Vertices.Length - _cacheCount;
            Vertices[index]=Vert3.Get(x, y,collision);
            --_cacheCount;
        }

        public void Apply() {
            if(Length!=0)
                collider2D.SetCustomShapes(Shape2Ds.AsArray(), Vertices.AsArray().Reinterpret<Vector2>(24));
        }

        public bool RemoveAtSwapBack(int index) {
            ++_cacheCount;
            var sArray = Shape2Ds.AsArray();
            var vArray = Vertices.AsArray().Reinterpret<Vector2>(24);
            var last = sArray.Length -_cacheCount;
            if (last == index) {
                Vertices[index]=Vert3.BottomRight(0,-1000);
                collider2D.SetCustomShape(sArray, vArray,index,index);
                return false;
            }
            if (last < index) {
                --_cacheCount;
                throw new ArgumentOutOfRangeException(index.ToString());
            }
            Vertices.Copy(index,last);
            Vertices[last]=Vert3.BottomRight(0,-1000);
            collider2D.SetCustomShape(sArray,vArray,index,index);
            collider2D.SetCustomShape(sArray,vArray,last,last);
            return true;
        }
        private static PhysicsShape2D GetShape(int vertexStart) => new PhysicsShape2D() 
            {shapeType = PhysicsShapeType2D.Polygon, radius = 0, vertexStartIndex = vertexStart, vertexCount = 3};
       
        public void Dispose() {
            Shape2Ds.DisposeIfCreated();
            Vertices.DisposeIfCreated();
        }
    }
    public class TileCollider :IDisposable{
        public readonly int X; 
        public readonly int Y;
        public ChunkDataBox Boxes;
        public ChunkDataTriangle Triangles;
        private NativeArray<int> _positionToIndex;

        public int ColliderCount => Boxes.Length + Triangles.Length;
        public int CacheCount => Boxes.CacheCount + Triangles.CacheCount;
        
        public  TileCollider(CustomCollider2D boxes,CustomCollider2D triangles,int x,int y,int initialSize) {
            X = x;
            Y = y;
            Boxes = new ChunkDataBox(boxes, initialSize);
            Triangles = new ChunkDataTriangle(triangles, initialSize);
            _positionToIndex = new NativeArray<int>(x * y,Allocator.Persistent);
            _positionToIndex.SetAll(-1);
        }

        public bool Add(int x, int y, TileCollision collision) {
            switch (collision) {
                case TileCollision.None: return false;
                case TileCollision.Filled: return AddBox(x, y);
            }
            return AddTriangle(x, y, collision);
        } 
        public bool AddTemp(int x, int y, TileCollision collision) {
            switch (collision) {
                case TileCollision.None: return false;
                case TileCollision.Filled: return AddBoxTemp(x, y);
            }
            return AddTriangleTemp(x, y, collision);
        }
        public bool AddBox(int x, int y) {
            if (IsNotValid(x,y)) return false;
            var index = x + y * X;
            var qIndex = GetId(x, y);
            if (qIndex != -1) return false;
            var left = GetId(x - 1, y);
            if (0 <= left&&left<X*Y) {
                Boxes.SetRight(left,x+1);
                _positionToIndex[index] = left;
                return true;
            }
            var right = GetId(x + 1, y);
            if (0 <= right&&right<X*Y) {
                Boxes.SetLeft(right,x);
                _positionToIndex[index] = right;
                return true;
            }
            _positionToIndex[index] = Boxes.Length;
            Boxes.Add(x,y);
            return true;
        } 
        public bool AddBoxTemp(int x, int y) {
            if (IsNotValid(x,y)) return false;
            var index = x + y * X;
            var qIndex = GetId(x, y);
            if (qIndex != -1) return false;
            var left = GetId(x - 1, y);
            if (0 <= left&&left<X*Y) {
                Boxes.SetRightTemp(left,x+1);
                _positionToIndex[index] = left;
                return true;
            }
            var right = GetId(x + 1, y);
            if (0 <= right&&right<X*Y) {
                Boxes.SetLeftTemp(right,x);
                _positionToIndex[index] = right;
                return true;
            }
            _positionToIndex[index] = Boxes.Length;
            Boxes.AddTemp(x,y);
            return true;
        } 
         public bool AddNewBoxTemp(int x, int y) {
            if (IsNotValid(x,y)) return false;
            var index = x + y * X;
            _positionToIndex[index] = Boxes.Length;
            Boxes.AddTemp(x,y);
            return true;
        } 
        
        public void SetLonger(int x, int y) {
            var index = x + y * X;
            var endIndex = Boxes.Length - 1;
            _positionToIndex[index] = endIndex;
            Boxes.SetRightTemp(endIndex,x+1);
        }
        public bool AddTriangle(int x, int y,TileCollision collision) {
            if (IsNotValid(x,y)) return false;
            var index = x + y * X;
            var qIndex = GetId(x, y);
            if (qIndex != -1) return false;
            _positionToIndex[index] = X*Y+Triangles.Length;
            Triangles.Add(x,y,collision);
            return true;
        }
        public bool AddTriangleTemp(int x, int y,TileCollision collision) {
            if (IsNotValid(x,y)) return false;
            var index = x + y * X;
            var qIndex = GetId(x, y);
            if (qIndex != -1) return false;
            _positionToIndex[index] = X*Y+Triangles.Length;
            Triangles.AddTemp(x,y,collision);
            return true;
        }

        public void Apply() {
            Boxes.Apply();
            Triangles.Apply();
        }
        public bool Remove(int x, int y) {
            if (IsNotValid(x,y)) return false;
            var index = x + y * X;
            var qIndex =  _positionToIndex[index];
            if (qIndex == -1) return false;
            _positionToIndex[index] = -1;
            if (X * Y <= qIndex) {
                var swapId = qIndex - X * Y;
                if(Triangles.RemoveAtSwapBack(swapId)) {
                   var grid = Triangles.Vertices[swapId].GetGrid();
                   _positionToIndex[grid.x + grid.y * X] = qIndex;
                }
                return true;
            }
            var left = GetId(x - 1, y);
            var right = GetId(x + 1, y);
            if (left != qIndex) {
                if (right != qIndex) {
                    if (!Boxes.RemoveAtSwapBack(qIndex)) return true;
                    ref var v4 = ref Boxes.Vertices.ElementAt(qIndex);
                    var grid = v4.GetGrid();
                    var gridIndex = grid.x + grid.y * X;
                    var gridWidth = v4.GetIntWidth();
                    for (int i = 0; i < gridWidth; i++) {
                        _positionToIndex[gridIndex + i] = qIndex;
                    }
                }
                else {
                    Boxes.SetLeft(qIndex, x + 1);
                }
            }
            else {
                if (right == qIndex) {
                    var width = Boxes.RemoveInLineAndGetRightWidth(qIndex, x);
                    var boxLastIndex = Boxes.Length - 1;
                    for (int i = 1; i <= width; i++) {
                        _positionToIndex[index + i] = boxLastIndex;
                    }
                }
                else {
                    Boxes.SetRight(qIndex, x);
                }
            }
            return true;
        }

        private bool IsValid(int x, int y) => (0<=x  &&  x<X && 0<=y && y < Y);
        private bool IsNotValid(int x, int y) => (x < 0 || X <= x || y < 0 || Y <= y);
        private int GetId(int x, int y){
            if (IsNotValid(x,y)) return  -1;
            var index = x + y * X;
            return _positionToIndex[index];
        }
        public void Clear() {
            Boxes.Clear();
            _positionToIndex.SetAll(-1);
        }

        public void Dispose() {
            Boxes.Dispose();
            Triangles.Dispose();
            _positionToIndex.Dispose();
        }
    }
  
}
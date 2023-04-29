using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
namespace TileGame {
    public class ChunkMesh3 {
        public struct Layer {
            public Mesh Mesh;
            readonly int X; 
            readonly int Y;
            UnsafeList<Quad> _quads;
            NativeArray<ushort> _positionToIndex;
            public bool Changed;
            public int Count => _quads.Length;
            public Layer(int xSize,int ySize) {
                X = xSize;
                Y = ySize;
                _quads = new UnsafeList<Quad>(X*Y,Allocator.Persistent);
                _positionToIndex = new NativeArray<ushort>(X*Y,Allocator.Persistent);
                _positionToIndex.SetAll(ushort.MaxValue);
                const float max = float.MaxValue;
                var bounds = new Bounds(default, new Vector3(max,max, max));
                Mesh = new Mesh {
                    indexFormat = IndexFormat.UInt16,bounds = bounds
                };
                Mesh.MarkDynamic();
                Changed = false;
            }
            public void Add(int x,int y,uint id) {
                if (x < 0 || X <= x || y < 0 || Y <= y) return;
                var index = x + y * X;
                var qIndex=_positionToIndex[index];
                if (qIndex != ushort.MaxValue) return;
                _positionToIndex[index] =(ushort) _quads.Length;
                _quads.Add(new Quad((ushort)x,(ushort)y,id));
                Changed = true;
            }
        
            public void Delete(int2 position) {
                if (position.x < 0 || X <= position.x || position.y < 0 || Y <= position.y) return;
                var index = position.x + position.y * X;
                var qIndex=_positionToIndex[index];
                if (qIndex != ushort.MaxValue) {
                    _positionToIndex[index] = ushort.MaxValue;
                    var last = _quads.Length - 1;
                    if (last != qIndex) {
                        var lastQuad = _quads[last];
                        var lastIndex = (int)(lastQuad.v0.x) +  (int)(lastQuad.v2.y) * X;
                        _positionToIndex[lastIndex] = qIndex;
                    }
                    _quads.RemoveAtSwapBack(qIndex);
                    Changed = true;
                }
            }
            public void Build(NativeArray<ushort> indices) {
                if(!Changed)return;
                Changed= false;
                if (_quads.Length == 0) {
                    Mesh.Clear(true);
                    return;
                }
                var quadCount = _quads.Length;
                var vertexCount = quadCount * 4;
                Mesh.SetVertexBufferParams
                    (vertexCount,_attributes);
                Mesh.SetVertexBufferData(_quads.AsArray(), 0, 0, quadCount);
                Mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt16);
                Mesh.SetIndexBufferData(indices.GetSubArray(0,vertexCount), 0, 0, vertexCount);
                var meshDesc = new SubMeshDescriptor(0, vertexCount, MeshTopology.Quads);
                Mesh.SetSubMesh(0, meshDesc,MeshUpdateFlags.DontRecalculateBounds);
            }
            public void Clear() {
                _quads.Length = 0;
                _positionToIndex.SetAll(ushort.MaxValue);
            }
            public void Dispose() {
                if(_quads.IsCreated)
                    _quads.Dispose();
                if(_positionToIndex.IsCreated)
                    _positionToIndex.Dispose();
            }
        }

        public Layer Front;
        public Layer Block;
        public Layer Placeable;
        public Layer Back;
        public ChunkMesh3(int xSize,int ySize) {
            Front = new Layer(xSize,ySize);
            Block = new Layer(xSize,ySize);
            Placeable = new Layer(xSize,ySize);
            Back = new Layer(xSize,ySize);
            _indices=new NativeArray<ushort>(xSize*ySize*4,Allocator.Persistent);
            var span = _indices.AsSpan();
            for (int i = 0; i < span.Length; i++) {
                span[i] = (ushort) i;
            }
        }
        
        NativeArray<ushort> _indices;
        public void Add(int layer,int x,int y,uint id) {
            switch (layer) {
                case 0:
                    Back.Add(x,y,id);
                    break;
                case 1:
                    Block.Add(x,y,id);
                    break;
                case 2:
                    Front.Add(x,y,id);
                    break;
            }
        }
        public void AddPlaceable(int x,int y,uint id) {
            Placeable.Add(x,y,id);
        }
        
        public void Delete(int layer,int2 position) {
            switch (layer) {
                case 0:
                    Back.Delete(position);
                    break;
                case 1:
                    Block.Delete(position);
                    break;
                case 2:
                    Front.Delete(position);
                    break;
            }
        }
        static readonly VertexAttributeDescriptor[] _attributes= new VertexAttributeDescriptor[] {
            new (VertexAttribute.Position,
                VertexAttributeFormat.UInt16, 2),
            new (VertexAttribute.TexCoord0,
                VertexAttributeFormat.UInt32, 1)};

        public void Build() {
            Front.Build(_indices);
            Block.Build(_indices);
            Placeable.Build(_indices);
            Back.Build(_indices);
        }

        public void Clear() {
            Front.Clear();
            Block.Clear();
            Placeable.Clear();
            Back.Clear();
        }
        public void Dispose() {
            Front.Dispose();
            Block.Dispose();
            Placeable.Dispose();
            Back.Dispose();
            _indices.Dispose();
        }
        
        public readonly struct Quad
        {
            public readonly Vert v0, v1, v2, v3;

            public Quad(ushort x,ushort y,uint id)
            {
                v0 =new Vert(x, (ushort)(y + 1), id*4);
                v1 = new Vert((ushort)(x + 1), (ushort)(y + 1),id*4+1);
                v2 = new Vert((ushort)(x + 1), y , id*4+2);
                v3 = new Vert(x,y , id*4+3);
            }
        }

        public  struct Vert {
            public ushort x;
            public ushort y;
            public uint uv_id;
            public Vert(ushort x, ushort y, uint id) {
                this.x = x;
                this.y = y;
                this.uv_id = id;
            }
           
        }
    }
}
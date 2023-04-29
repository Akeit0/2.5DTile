using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
namespace TileGame {
    public  struct ChunkMesh :IDisposable {
        public readonly int X; 
        public readonly int Y; 
        private NativeList<Quad> _quads;
        private NativeArray<ushort> _positionToIndex;
        private NativeArray<ushort> _indices;
        
        public int Length =>
            _quads.Length;
      
        public ChunkMesh(int xSize,int ySize,int initialSize) {
            X = xSize;
            Y = ySize;
            _quads = new NativeList<Quad>(initialSize,Allocator.Persistent);
            _positionToIndex = new NativeArray<ushort>(X*Y,Allocator.Persistent);
            _positionToIndex.SetAll(ushort.MaxValue);
            _indices=new NativeArray<ushort>(_positionToIndex.Length*4,Allocator.Persistent);
            new IndexConstruction16Job(_indices).Run(_indices.Length);
        }

        public void Add(int2 position) {
            if (position.x < 0 || X <= position.x || position.y < 0 || Y <= position.y) return;
            var index = position.x + position.y * X;
            var qIndex=_positionToIndex[index];
            if (qIndex != ushort.MaxValue) return;
            _positionToIndex[index] = (ushort)_quads.Length;
            _quads.Add(new Quad(position,1));
        }
        public void Add(int2 position,float4 uv) {
            if (position.x < 0 || X <= position.x || position.y < 0 || Y <= position.y) return;
            var index = position.x + position.y * X;
            var qIndex=_positionToIndex[index];
            if (qIndex !=ushort.MaxValue) return;
            _positionToIndex[index] =(ushort) _quads.Length;
            _quads.Add(new Quad(position,1,uv));
        }
        public void Add(int x,int y,float4 uv) {
            if (x < 0 || X <= x || y < 0 || Y <= y) return;
            var index = x + y * X;
            var qIndex=_positionToIndex[index];
            if (qIndex != ushort.MaxValue) return;
            _positionToIndex[index] =(ushort) _quads.Length;
            _quads.Add(new Quad(new float2(x,y), 1,uv));
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
                    var lastPosition = lastQuad.Position;
                    var lastIndex = (int)lastPosition.x +  (int)lastPosition.y * X;
                    _positionToIndex[lastIndex] = qIndex;
                }
                _quads.RemoveAtSwapBack(qIndex);
            }
        }
        public void Clear() {
            _quads.Clear();
            _positionToIndex.SetAll(ushort.MaxValue);
        }
        private  static readonly VertexAttributeDescriptor[] _attributes= new VertexAttributeDescriptor[] {
            new (VertexAttribute.Position,
                VertexAttributeFormat.Float32, 2),
            new (VertexAttribute.TexCoord0,
                VertexAttributeFormat.Float32, 2)};
        public bool Build(Mesh mesh) {
            if (_quads.Length == 0) return false;
            var quadCount = _quads.Length;
            var vertexCount = quadCount * 4;
            // Vertex buffer
            mesh.SetVertexBufferParams
            (vertexCount,_attributes);
            mesh.SetVertexBufferData(_quads.AsArray(), 0, 0, quadCount);
            // Index buffer
            mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt16);
            mesh.SetIndexBufferData(_indices.GetSubArray(0,vertexCount), 0, 0, vertexCount);
            // Submesh definition
            var meshDesc = new SubMeshDescriptor(0, vertexCount, MeshTopology.Quads);
            mesh.SetSubMesh(0, meshDesc,MeshUpdateFlags.DontRecalculateBounds);
            return true;
        }
        public void Dispose() {
            if(_quads.IsCreated)
                _quads.Dispose();
            if(_positionToIndex.IsCreated)
                _positionToIndex.Dispose();
            if(_indices.IsCreated)
             _indices.Dispose();
        }
        public readonly struct Quad
        {
            readonly float4 _v0, _v1, _v2, _v3;
            public  float2 Position => math.float2(_v0.x, _v2.y);
            public Quad(float2 position, float size)
            {
                _v0 = math.float4(position.x, position.y + size, 0, 1);
                _v1 = math.float4(position.x + size, position.y + size, 1, 1);
                _v2 = math.float4(position.x + size, position.y , 1, 0);
                _v3 = math.float4(position.x , position.y , 0, 0);
            }

            public Quad(float2 position, float size,float4 uv)
            {
                _v0 = math.float4(position.x, position.y + size,  uv.x, uv.y+uv.w);
                _v1 = math.float4(position.x + size, position.y + size, uv.xy+uv.zw);
                _v2 = math.float4(position.x + size, position.y , uv.x+uv.z, uv.y);
                _v3 = math.float4(position.xy , uv.xy);
            }
        }
        [BurstCompile]
        struct IndexConstruction16Job : IJobParallelFor
        {
            [WriteOnly] NativeArray<ushort> _output;

            public IndexConstruction16Job(NativeArray<ushort> output)
                => _output = output;

            public void Execute(int i)
                => _output[i] = (ushort)i;
        }
    }
}
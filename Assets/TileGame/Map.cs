using System;
using System.Collections.Generic;
using System.IO;
using TileGame.Light;
using Stopwatch = System.Diagnostics.Stopwatch;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace TileGame {
    public class Map : MonoBehaviour {
        public int X;
        public int Y;

        public CelledTile CelledTiles;
        UVBuffer _uvBuffer;
        [SerializeField] Material _material = null;
        [SerializeField] Material _dupMaterial = null;
        [SerializeField] Material _frontMaterial = null;
        ChunkMesh3 _chunkMesh;
        TileCollider _tileCollider;
        [SerializeField] CustomCollider2D _boxes;
        [SerializeField] CustomCollider2D _triangles;
        [SerializeField] Camera _camera;

        MaterialPropertyBlock _propertyBlock;


        // private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        Array2D<Tile> _tiles;
        Unity.Mathematics.Random _random = new(123);
        public Vector3 DeltaPosition;
        [Range(1, 20)] public ushort TileID;
        [Range(1, 10)] public int LayerCount;
        RGBLightMap _simpleLightMap;
        Texture2D _lightTex;
        static readonly int BrightnessID = Shader.PropertyToID("_Brightness");
        static readonly int LightMapID = Shader.PropertyToID("_GlobalLightMap");
        [Range(0, 1)] public float BrightNess = 0.1f;

        public Color SkyLight = Color.white;
        public Color TorchLight  = new Color(1,0.8f,0.8f,1f);
        [Range(0,255)]
        public byte PlayerLight = 255;
        
        private void Start() {
            Application.targetFrameRate = 60;
            _uvBuffer = new UVBuffer(CelledTiles,0.305f);
            _propertyBlock = new MaterialPropertyBlock();
            _propertyBlock.SetFloat(BrightnessID, BrightNess);
            var path = Application.persistentDataPath + "/" + MapPath;
            ReCreate();
            if (MapData != null) {
                LoadMap(MapData);
            }
            else LoadMap(path);
         
            Instance = this;
            _lightWasOn = LightOn;
        }

        public static Map Instance { get; private set; }
        void ReCreate() {
            _tiles = new Array2D<Tile>(X, Y);

            _simpleLightMap = new RGBLightMap(X, Y);
            _simpleLightMap.Medium.SetAll((byte) 232);
            _lightTex = new Texture2D(X, Y, TextureFormat.RGB24, false)
                {filterMode =FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp};
            Shader.SetGlobalTexture(LightMapID,_lightTex);
            _tileCollider = new TileCollider(_boxes, _triangles, X, Y, X * Y / 2);
            _chunkMesh = new ChunkMesh3(X, Y);
            _camera ??= Camera.main;
        }

        public unsafe void SaveMap(string path) {
            using var wfs = new FileStream(path, FileMode.Create);
            using var encoder = new System.IO.Compression.BrotliEncoder(9, 16);
            ReadOnlySpan<byte> header = stackalloc byte[2] {(byte) X, (byte) Y};
            wfs.Write(header);
            fixed (Tile* t = _tiles) {
                var src = new ReadOnlySpan<byte>((byte*) t, X * Y * sizeof(Tile));
                var ptr = UnsafeUtility.Malloc(X * Y * sizeof(Tile), UnsafeUtility.AlignOf<byte>(), Allocator.Temp);
                var bytes = new Span<byte>(ptr, X * Y * sizeof(Tile));
             
                var status = encoder.Compress(src, bytes, out var bytesConsumed, out var bytesWritten, true);
                wfs.Write(bytes.Slice(0, bytesWritten));
                UnsafeUtility.Free(ptr, Allocator.Temp);
            }
        }

        public unsafe void LoadMap(string path) {
            if(!File.Exists(path))return;
            using var fs = new FileStream(path, FileMode.Open);
            using   var decoder = new System.IO.Compression.BrotliDecoder();
            var x = fs.ReadByte();
            var y = fs.ReadByte();
            if (X != x || Y != y) {
                _chunkMesh.Dispose();
                _tileCollider.Dispose();
                _simpleLightMap.Dispose();
                X = x;
                Y = y;
                ReCreate();
            }
            fixed (Tile* t = _tiles) {
                var dst = new Span<byte>((byte*) t, x * y * sizeof(Tile));
                var ptr = UnsafeUtility.Malloc(dst.Length, UnsafeUtility.AlignOf<byte>(), Allocator.Temp);
                var bytes = new Span<byte>(ptr, X * Y * sizeof(Tile));
                var count = fs.Read(bytes);
                bytes = bytes.Slice(0, count);
                var status = decoder.Decompress(bytes, dst, out var bytesConsumed, out var bytesWritten);
                UnsafeUtility.Free(ptr, Allocator.Temp);
            }

            Fill();
        }
        public unsafe void LoadMap(TextAsset asset) {
             var assetBytes = asset.bytes.AsSpan();
            using   var decoder = new System.IO.Compression.BrotliDecoder();
            var x = assetBytes[0];
            var y = assetBytes[1];
            if (X != x || Y != y) {
                _chunkMesh.Dispose();
                _tileCollider.Dispose();
                _simpleLightMap.Dispose();
                X = x;
                Y = y;
                ReCreate();
            }
            fixed (Tile* t = _tiles) {
                var dst = new Span<byte>((byte*) t, x * y * sizeof(Tile));
                var ptr = UnsafeUtility.Malloc(dst.Length, UnsafeUtility.AlignOf<byte>(), Allocator.Temp);
                var bytes = new Span<byte>(ptr, assetBytes.Length-2);
                assetBytes[2..].CopyTo(bytes);
                var status = decoder.Decompress(bytes, dst, out var bytesConsumed, out var bytesWritten);
                UnsafeUtility.Free(ptr, Allocator.Temp);
            }

            Fill();
        }

        void Fill() {
            _chunkMesh.Clear();
            _tileCollider.Clear();
            _simpleLightMap.Medium.SetAll((byte) 232);
            for (int y = 0; y < Y; y++) {
                for (int x = 0; x < X; x++) {
                    var tile = _tiles[x, y];
                    if (tile.Wall != 0)
                        _chunkMesh.Back.Add(x, y, tile.Wall);
                    if (tile.Block != 0) {
                        if (tile.Collision != 0) {
                            _chunkMesh.Block.Add(x, y, tile.Block);
                            _simpleLightMap.GetMedium(x, y) = 170;
                            _tileCollider.AddTemp(x, y, tile.Collision);
                        }
                        else {
                            _chunkMesh.Placeable.Add(x, y, tile.Block);
                        }
                    }

                    if (tile.FrontWall != 0)
                        _chunkMesh.Front.Add(x, y, tile.FrontWall);
                }
            }

            _tileCollider.Apply();
        }


        public bool RayCastToTile(Vector2 start, Vector2 end, out Vector2 hitPos) {
            hitPos = end;
            if (start.x < 0 || X <= start.x || start.y < 0 && Y < start.y) return false;
            var ray = new TileRay(start, end, new Vector2Int(X, Y));
            ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
            if (ray.CollisionType != 0 && ray.TryCollide()) {
                hitPos = ray.HitPositionInGrid + ray.Grid;
                return true;
            }

            while (ray.MoveNext()) {
                ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
                if (ray.CollisionType != 0 && ray.TryCollide()) {
                    hitPos = ray.HitPositionInGrid + ray.Grid;
                    return true;
                }
            }

            return false;
        }

        public bool RayCastToTile(Vector2 start, Vector2 end, out Vector2 hitPos, out Vector2 normal) {
            hitPos = end;
            normal = default;
            if (start.x < 0 || X <= start.x || start.y < 0 && Y < start.y) return false;
            var ray = new TileRay(start, end, new Vector2Int(X, Y));
            ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
            if (ray.CollisionType != 0 && ray.TryCollideWithNormal()) {
                hitPos = ray.HitPositionInGrid + ray.Grid;
                normal = ray.HitNormal;
                return true;
            }

            while (ray.MoveNext()) {
                ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
                if (ray.CollisionType != 0 && ray.TryCollideWithNormal()) {
                    hitPos = ray.HitPositionInGrid + ray.Grid;
                    normal = ray.HitNormal;
                    return true;
                }
            }

            return false;
        }

        public bool RayCastToTile(Vector2 start, Vector2 delta, out float distance, out Vector2 normal) {
            distance = delta.magnitude;
            var end = start + delta;
            normal = default;
            if (start.x < 0 || X <= start.x || start.y < 0 && Y < start.y) return false;
            var ray = new TileRay(start, end, new Vector2Int(X, Y));
            ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
            if (ray.CollisionType != 0 && ray.TryCollideWithNormal()) {
                distance = (ray.HitPositionInGrid + ray.Grid - start).magnitude;
                normal = ray.HitNormal;
                return true;
            }

            while (ray.MoveNext()) {
                ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
                if (ray.CollisionType != 0 && ray.TryCollideWithNormal()) {
                    distance = (ray.HitPositionInGrid + ray.Grid - start).magnitude;
                    normal = ray.HitNormal;
                    return true;
                }
            }

            return false;
        }

        public bool RayCastToTileHorizontal(Vector2 start, float delta, out float distance, out Vector2 normal) {
            distance = delta;
            normal = default;
            if (delta == 0 || start.x < 0 || X <= start.x || start.y < 0 || Y < start.y) return false;
            if (0 < delta) {
                var ray = new TileRayRight(start, delta, new Vector2Int(X, Y));
                ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
                if (ray.CollisionType != 0 && ray.TryCollideWithNormal()) {
                    distance = ray.HitPositionInGrid.x + ray.Grid.x - start.x;
                    normal = ray.HitNormal;
                    return true;
                }

                while (ray.MoveNext()) {
                    ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
                    if (ray.CollisionType != 0 && ray.TryCollideWithNormal()) {
                        distance = ray.HitPositionInGrid.x + ray.Grid.x - start.x;
                        normal = ray.HitNormal;
                        return true;
                    }
                }

                return false;
            }

            {
                var ray = new TileRayLeft(start, delta);
                ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
                if (ray.CollisionType != 0 && ray.TryCollideWithNormal()) {
                    distance = -(ray.HitPositionInGrid.x + ray.Grid.x - start.x);
                    normal = ray.HitNormal;
                    return true;
                }

                while (ray.MoveNext()) {
                    ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
                    if (ray.CollisionType != 0 && ray.TryCollideWithNormal()) {
                        distance = -(ray.HitPositionInGrid.x + ray.Grid.x - start.x);
                        normal = ray.HitNormal;
                        return true;
                    }
                }

                return false;
            }
        }

        public bool RayCastToTileVertical(Vector2 start, float delta, out float distance, out Vector2 normal) {
            distance = delta;
            normal = default;
            if (delta == 0 || start.x < 0 || X <= start.x || start.y < 0 || Y < start.y) return false;
            if (0 < delta) {
                var ray = new TileRayUp(start, delta, new Vector2Int(X, Y));
                ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
                if (ray.CollisionType != 0 && ray.TryCollideWithNormal()) {
                    distance = ray.HitPositionInGrid.y + ray.Grid.y - start.y;
                    normal = ray.HitNormal;
                    return true;
                }

                while (ray.MoveNext()) {
                    ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
                    if (ray.CollisionType != 0 && ray.TryCollideWithNormal()) {
                        distance = ray.HitPositionInGrid.y + ray.Grid.y - start.y;
                        normal = ray.HitNormal;
                        return true;
                    }
                }

                return false;
            }

            {
                var ray = new TileRayDown(start, delta);
                ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
                if (ray.CollisionType != 0 && ray.TryCollideWithNormal()) {
                    distance = -(ray.HitPositionInGrid.y + ray.Grid.y - start.y);
                    normal = ray.HitNormal;
                    return true;
                }

                while (ray.MoveNext()) {
                    ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
                    if (ray.CollisionType != 0 && ray.TryCollideWithNormal()) {
                        distance = -(ray.HitPositionInGrid.y + ray.Grid.y - start.y);
                        normal = ray.HitNormal;
                        return true;
                    }
                }

                return false;
            }
        }

        public bool RayCastToTileSimple(Vector2 start, Vector2 end, out Vector2 hitPos) {
            hitPos = end;
            if (start.x < 0 || X <= start.x || start.y < 0 && Y < start.y) return false;
            var ray = new TileRay(start, end, new Vector2Int(X, Y));
            ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
            if (ray.CollisionType != 0) {
                hitPos = ray.PositionInGrid + ray.Grid;
                return true;
            }

            while (ray.MoveNext()) {
                ray.CollisionType = _tiles[ray.Grid.x, ray.Grid.y].Collision;
                if (ray.CollisionType != 0) {
                    hitPos = ray.PositionInGrid + ray.Grid;
                    return true;
                }
            }

            return false;
        }

       
        public TextAsset MapData;
        public string MapPath;
        
        public void Update() {
            if (!_simpleLightMap.IsCreated) return;

            if (Input.GetKeyDown(KeyCode.K)) {
                var path = Application.persistentDataPath + "/" + MapPath;
                Debug.Log("Save " + path);
                SaveMap(path);
            }

            if (Input.GetKeyDown(KeyCode.L)) {
                var path = Application.persistentDataPath + "/" + MapPath;
                Debug.Log("Load " + path);
                LoadMap(path);
            }


            if (Input.GetKeyDown(KeyCode.Alpha0)) {
                TileID = 10;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1)) {
                TileID = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                TileID = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                TileID = 3;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4)) {
                TileID = 4;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5)) {
                TileID = 5;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6)) {
                TileID = 6;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7)) {
                TileID = 7;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8)) {
                TileID = 8;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9)) {
                TileID = 9;
            }

            if (Input.mouseScrollDelta.y != 0) {
                TileID = (ushort) ((Input.mouseScrollDelta.y > 0 ? TileID + 1 : TileID + -1));
            }

            TileID = (ushort) math.clamp(TileID, 1, CelledTiles.TileDataArray.Length - 1);


            var layer = Input.GetKey(KeyCode.LeftShift) ? 0 : Input.GetKey(KeyCode.LeftControl) ? 2 : 1;
            if (_camera.TryGetMousePositionOnPlane(out var vector2)) {
                if (vector2.x >= 0 && vector2.x < X && vector2.y >= 0 && vector2.y < Y) {
                    var x = (int) vector2.x;
                    var y = (int) vector2.y;
                    if (Input.GetMouseButton(0)) {
                        ref var tile = ref _tiles[x, y];
                        if (tile[layer] == 0) {
                            tile[layer] = TileID;
                            if (layer == 1) {
                                var t = CelledTiles.TileDataArray[TileID];
                                if (t.Collision != 0) {
                                    _chunkMesh.Add(layer, x, y, TileID);
                                    tile.Collision = t.Collision;
                                    _simpleLightMap.GetMedium(x, y) = 170;
                                    _tileCollider.Add(x, y, t.Collision);
                                }
                                else {
                                    _chunkMesh.AddPlaceable(x, y, TileID);
                                }
                            }
                            else {
                                _chunkMesh.Add(layer, x, y, TileID);
                            }
                        }
                    }

                    if (Input.GetMouseButton(1)) {
                        ref var tile = ref _tiles[x, y];
                        if (tile[layer] != 0) {
                            tile[layer] = default;
                            if (layer == 1) {
                                if (tile.Collision != 0) {
                                    _chunkMesh.Delete(layer, math.int2(x, y));
                                    _simpleLightMap.GetMedium(x, y) = 232;
                                    _tileCollider.Remove(x, y);
                                    tile.Collision = default;
                                }
                                else {
                                    _chunkMesh.Placeable.Delete(math.int2(x, y));
                                }
                            }
                            else {
                                _chunkMesh.Delete(layer, math.int2(x, y));
                            }
                        }
                    }

                    _chunkMesh.Build();
                }
            }
            var pos = LightBall.position;
            if(LightOn) {
                Profiler.BeginSample("SetLightMap");
                _simpleLightMap.Clear();
                unsafe {
                    var skyLight = new Color24(SkyLight);
                    var torchLight = new Color24(TorchLight);
                    fixed (Tile* tiles = _tiles) {
                        var map = _simpleLightMap.Colors.GetPtr();
                        var length = X * Y;
                        var darkLength = X * (int) (Y * 0.66f);
                        for (int i = 0; i < darkLength; i++) {
                            if (tiles[i].Block == 11) {
                                map[i] = torchLight;
                            }
                        }

                        for (int i = darkLength; i < length; i++) {
                            var t = tiles[i];
                            if (t.Block == 11) {
                                map[i] = torchLight;
                            }
                            else if (t.Wall == 0 && t.Block == 0) {
                                map[i] = skyLight;
                            }
                        }
                    }
                }
                {
                    if (pos.x < 0 || X <= pos.x || pos.y < 0 && Y < pos.y) {
                        var newPos = new Vector2(math.clamp(pos.x, 0, X - 0.1f), math.clamp(pos.y, 0, Y - 0.1f));
                        LightBall.position = pos = newPos;
                    }

                    var x = math.clamp((int) pos.x, 0, X - 1);
                    var y = math.clamp((int) pos.y, 0, Y - 1);
                    var c = new Color24(PlayerLight, PlayerLight, PlayerLight);
                    _simpleLightMap[x, y] = Color24.Max(_simpleLightMap[x, y],c);
                }
                Profiler.EndSample();
                if(ThreadOn)
                 _simpleLightMap.Blur();
                else _simpleLightMap.BlurRun();
                _simpleLightMap.CopyTo(_lightTex.GetRawTextureData<Color24>());
             
                _lightTex.Apply();
                if(!_lightWasOn) {
                    Shader.SetGlobalFloat("_GlobalLightBloom", 0.2f);  
                    _lightWasOn = true;
                }
            }else if(_lightWasOn) {
                _lightTex.GetRawTextureData<byte>().SetAll<byte>(255);
                _lightTex.Apply();
                Shader.SetGlobalFloat("_GlobalLightBloom", 0);
                _lightWasOn = false;
            }


            if (0 < _chunkMesh.Back.Count) {
                var mesh = _chunkMesh.Back.Mesh;
                mesh.Draw(Matrix4x4.TRS(DeltaPosition*1.0001f, Quaternion.identity, Vector3.one), _material);
            }

            if (0 < _chunkMesh.Placeable.Count) {
                var mesh = _chunkMesh.Placeable.Mesh;
                mesh.Draw(Matrix4x4.TRS(DeltaPosition * 0.4f, Quaternion.identity, Vector3.one), _material);
            }

            if (0 < _chunkMesh.Block.Count) {
                var mesh = _chunkMesh.Block.Mesh;
                Vector3 delta = DeltaPosition * (2f / (LayerCount - 1));
                var current = -DeltaPosition;
                mesh.Draw(current, _material);
                for (int i = 0; i < LayerCount - 2; i++) {
                    current += delta;
                    mesh.Draw(current, _dupMaterial);
                }
                if (1 < LayerCount)
                    mesh.Draw(DeltaPosition, _dupMaterial);
            }

            if (ShowFront && 0 < _chunkMesh.Front.Count) {
                var mesh = _chunkMesh.Front.Mesh;
                if (TransRadius == 0) {
                    mesh.Draw(-DeltaPosition, _material);
                }
                else {
                    _frontMaterial.SetVector(TransparentPosID, new Vector4(pos.x, pos.y, 0, 0));
                    _frontMaterial.SetFloat(RadiusID, TransRadius);

                    mesh.Draw(-DeltaPosition * 1.001f, _frontMaterial);
                }
            }
        }

        public bool ThreadOn;
        public bool LightOn;
        bool _lightWasOn;
        public bool ShowFront;
        [Range(0, 10)] public float TransRadius;
        public Transform LightBall;
        static readonly int TransparentPosID = Shader.PropertyToID("_TransparentPos");
        static readonly int RadiusID = Shader.PropertyToID("_Radius");

        void OnDestroy() {
            Instance = null;
            _chunkMesh.Dispose();
            _tileCollider.Dispose();
            _simpleLightMap.Dispose();
            _uvBuffer.Dispose();
        }

        public void PostProcess(Bloom bloom) {
            bloom.skipIterations.value=6;
        }
        // public FastNoiseLite.NoiseType noiseType=FastNoiseLite.NoiseType.Cellular;
        // public FastNoiseLite.FractalType fractalType=FastNoiseLite.FractalType.FBm;
        // public FastNoiseLite.CellularDistanceFunction cellularDistanceFunction=FastNoiseLite.CellularDistanceFunction.Euclidean;
        // public FastNoiseLite.CellularReturnType cellularReturnType=FastNoiseLite.CellularReturnType.CellValue;
        // [Range(0.1f,0.3f)]
        // public float NoiseFreq;
        // [Range(0,2)]
        // public float Threshold;
        // [Range(0.05f,0.2f)]
        // public float ZoneNoiseFreq;
        // [Range(0,2)]
        // public float ZoneThreshold;
        //
        //  void NoiseLevel() {
        //      _chunkMesh.Clear();
        //      _tileCollider.Clear();
        //     Array.Clear(_tiles,0,_tiles.Length); 
        //     noise1.SetSeed(_random.NextInt(1,1000));
        //     noise1.SetFrequency(NoiseFreq);
        //     noise1.SetNoiseType(noiseType);
        //     noise1.SetFractalType(fractalType);
        //     noise1.SetCellularDistanceFunction(cellularDistanceFunction);
        //     noise1.SetCellularReturnType(cellularReturnType);
        //     noise2.SetSeed(_random.NextInt(1,1000));
        //     noise2.SetFrequency(ZoneNoiseFreq);
        //     noise2.SetNoiseType(noiseType);
        //     noise2.SetFractalType(fractalType);
        //     noise2.SetCellularDistanceFunction(cellularDistanceFunction);
        //     noise2.SetCellularReturnType(cellularReturnType);
        //     Profiler.BeginSample("Noise");
        //     for (int y = 0; y < Y; y++) {
        //         for (int x = 0; x < X; x++) {
        //             if (CheckNoise(x, y)) {
        //                 _tiles[x, y] = new Tile(1);
        //             }
        //         }
        //     }
        //     Profiler.EndSample();
        //     Profiler.BeginSample("Fill");
        //     var uv = TileObject.Sprite.GetUV();
        //     for (int y = 0; y < Y; y++) {
        //         bool lastWasFilled = false;
        //         for (int x = 0; x < X; x++) {
        //             if (_tiles[x, y].Value == 1) {
        //                 _chunkMesh.Add(x, y, uv);
        //                 if (lastWasFilled) {
        //                     _tileCollider.SetLonger(x, y);
        //                 }
        //                 else {
        //                     _tileCollider.AddNewBoxTemp(x, y);
        //                 }
        //                 lastWasFilled = true;
        //             }
        //             else {
        //                 lastWasFilled = false;
        //             }
        //         }
        //     }
        //     Profiler.EndSample();
        //     _chunkMesh.Build(_mesh);
        //     _tileCollider.Boxes.Apply();
        // }
        //
        // FastNoiseLite noise1 = new FastNoiseLite();
        // FastNoiseLite noise2 = new FastNoiseLite();

        //
        //
        // public bool CheckNoise(int x,int y)
        // {
        //     if ( ZoneThreshold == 0f ||
        //          noise2.GetNoise(x, y)+1>ZoneThreshold)
        //     {
        //         if (Threshold == 0f)
        //             return true;
        //         return  noise1.GetNoise(x, y)+1>Threshold;
        //     }
        //     return false;
        // }
    }
}
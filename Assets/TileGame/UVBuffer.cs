using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using System;

namespace TileGame {
    public class UVBuffer :IDisposable {
        public GraphicsBuffer Buffer;
        public NativeArray<float2x4> UvArray;
        public int Length => UvArray.Length;
        static readonly int UVBufferID = Shader.PropertyToID("uv_buffer");
        public UVBuffer(TileObject[]tiles) {
            UvArray = new NativeArray<float2x4>(tiles.Length, Allocator.Persistent);
            var t = tiles[1].Sprite.texture;
            var padding = new float2(0.1f/t.width,0.1f/t.height);
            for (int i = 1; i < tiles.Length; i++) {
                var uv=tiles[i].Sprite.GetUV(padding);
                UvArray[i]=new float2x4(float2(uv.x, uv.y + uv.w),uv.xy+uv.zw,float2(uv.x+uv.z, uv.y),uv.xy);
            }
            Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, UvArray.Length*8, sizeof(float));
            Buffer.SetData(UvArray);
            
        }
     public UVBuffer(CelledTile tiles,float paddingRate=0.5f) {
             var data = tiles.TileDataArray;
            UvArray = new NativeArray<float2x4>(data.Length, Allocator.Persistent);
            var t = tiles.Texture;
            var padding = new float2(paddingRate/t.width,paddingRate/t.height);
            var uvWidth= (float)tiles.PixelPerUnit / t.width;
            var uvWidthWithPadding=uvWidth-2*padding.x;
            var uvHeight= (float)tiles.PixelPerUnit / t.height;
            var uvHeightWithPadding=uvHeight-2*padding.y;
            var x = padding.x;
            var y=1f-uvHeight+padding.y;
            for (int i = 1; i < data.Length; i++) {
                UvArray[i]=new float2x4(float2(x, y + uvHeightWithPadding),float2(x+uvWidthWithPadding, y + uvHeightWithPadding),float2(x+uvWidthWithPadding, y),float2(x,y));
                x+=uvWidth;
                if (1f < x) {
                    x = padding.x;
                    y -= uvHeight;
                }
            }
            Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, UvArray.Length*8, sizeof(float));
            Buffer.SetData(UvArray);
            Shader.SetGlobalBuffer(UVBufferID,Buffer);
        }
     
        public void Dispose() {
            Buffer?.Dispose();
            UvArray.Dispose();
        }
    }
   
}
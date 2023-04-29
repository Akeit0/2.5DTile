using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
namespace TileGame {
    public struct Color24 {
        public byte R;
        public byte G;
        public byte B;
        public Color24(byte r, byte g, byte b) {
            R = r;
            G = g;
            B = b;
        }
        public Color24(Color color) {
            R = (byte) (color.r * 255);
            G = (byte) (color.g * 255);
            B = (byte) (color.b * 255);
        }
        public static Color24 Max(Color24 lhs, Color24 rhs) {
            return new Color24(Math.Max(lhs.R, rhs.R), Math.Max(lhs.G, rhs.G), Math.Max(lhs.B, rhs.B));
        }
        public  void SetMax(Color24 rhs) {
            R = Math.Max(R, rhs.R);
            G = Math.Max(G, rhs.G);
            B = Math.Max(B, rhs.B);
        }
        
    }
    [BurstCompatible]
    public class RGBLightMap : IDisposable {
        NativeArray<Color24> _colors;
        public NativeArray<Color24> Colors => _colors;

        public void CopyTo(NativeArray<Color24> array) {
            _colors.CopyTo(array);
        }

        NativeArray<byte> _medium;
        public NativeArray<byte> Medium => _medium;

        public int Width { get; set; }

        public int Height { get; set; }
        public bool IsCreated => _colors.IsCreated;

        public unsafe ref Color24 this[int x, int y] => ref ((Color24*) _colors.GetUnsafePtr())[IndexOfWithCheck(x, y)];
        public unsafe ref byte GetMedium(int x, int y) => ref ((byte*) _medium.GetUnsafePtr())[IndexOfWithCheck(x, y)];

        public RGBLightMap(int width, int height) {
            this.Width = width;
            this.Height = height;
            this._colors = new NativeArray<Color24>(width * height, Allocator.Persistent);
            this._medium = new NativeArray<byte>(width * height, Allocator.Persistent);
        }



        public void Clear() {
            if (_colors.IsCreated)
                _colors.Clear();
        }


        public void Blur() {
            BlurHV(BlurHV(default)).Complete();
        }


        public void BlurRun() {
            new BlurLineJob(_colors, _medium, Width, Width, 1).Run(Height);
            new BlurLineJob(_colors, _medium, 1, Height, Width).Run(Width);
            new BlurLineJob(_colors, _medium, Width, Width, 1).Run(Height);
            new BlurLineJob(_colors, _medium, 1, Height, Width).Run(Width);
            
        }



        JobHandle BlurHV(JobHandle jobHandle) {
            var job = new BlurLineJob(_colors, _medium, Width, Width, 1);
            jobHandle = job.Schedule(Height, 1, jobHandle);
            job = new BlurLineJob(_colors, _medium, 1, Height, Width);
            return job.Schedule(Width, 1, jobHandle);
        }



        [BurstCompile(CompileSynchronously = true,DisableSafetyChecks = true,OptimizeFor = OptimizeFor.Performance)]
        unsafe struct BlurLineJob : IJobParallelFor {
            //[NativeDisableParallelForRestriction] NativeArray<Color24> colorArray;
            [NativeDisableUnsafePtrRestriction] Color24* colorArray;

           // [ReadOnly, NativeDisableParallelForRestriction]
           //  NativeArray<byte> mediumArray;
            [NativeDisableUnsafePtrRestriction] byte* mediumArray;
           int startIndexFactor;
           int perIterationCount;
           int stride;

            public BlurLineJob(NativeArray<Color24> colorArray, NativeArray<byte> mediumArray,
                int startIndexFactor, int perIterationCount, int stride) {
                //this.colorArray = colorArray;
                this.colorArray = colorArray.GetPtr();
                //this.mediumArray = mediumArray;
                this.mediumArray = mediumArray.GetPtr();
                this.startIndexFactor = startIndexFactor;
                this.perIterationCount = perIterationCount;
                this.stride = stride;
            }

            public void Execute(int index) {
                Color24 lastColor = colorArray[index * startIndexFactor];
                {
                    var m = mediumArray[index * startIndexFactor + stride];
                    lastColor.R = (byte) ((lastColor.R * m) >> 8);
                    lastColor.G = (byte) ((lastColor.G * m) >> 8);
                    lastColor.B = (byte) ((lastColor.B * m) >> 8);
                }
                for (int i = 1; i < perIterationCount - 1; ++i) {
                    var realIndex = index * startIndexFactor + i * stride;
                    var currentColor = colorArray[realIndex];
                    var update= false;
                    if (lastColor.R <= currentColor.R)
                        lastColor.R = currentColor.R;
                    else 
                        update=true;
                    if (lastColor.G <= currentColor.G)
                        lastColor.G = currentColor.G;
                    else 
                        update=true;
                    if (lastColor.B <= currentColor.B)
                        lastColor.B = currentColor.B;
                    else 
                        update=true;
                    if(update)
                        colorArray[realIndex]=  lastColor ;
                    int m = mediumArray[realIndex + stride];
                    lastColor.R = (byte) ((lastColor.R * m) >> 8);
                    lastColor.G = (byte) ((lastColor.G * m) >> 8);
                    lastColor.B = (byte) ((lastColor.B * m) >> 8);
                }

                var lastIndex = index * startIndexFactor + (perIterationCount - 1) * stride;

                var c = colorArray[lastIndex];
                lastColor.SetMax(c);
                colorArray[lastIndex] = lastColor;
                {
                    var m = mediumArray[lastIndex - stride];
                    lastColor.R = (byte) ((lastColor.R * m) >> 8);
                    lastColor.G = (byte) ((lastColor.G * m) >> 8);
                    lastColor.B = (byte) ((lastColor.B * m) >> 8);
                }
                for (int i = perIterationCount-2;0< i; --i) {
                    var realIndex = index * startIndexFactor + i * stride;
                    var currentColor = colorArray[realIndex];
                    var update= false;
                    if (lastColor.R <= currentColor.R)
                        lastColor.R = currentColor.R;
                    else 
                        update=true;
                    if (lastColor.G <= currentColor.G)
                        lastColor.G = currentColor.G;
                    else 
                        update=true;
                    if (lastColor.B <= currentColor.B)
                        lastColor.B = currentColor.B;
                    else 
                        update=true;
                    if(update)  colorArray[realIndex]=  lastColor ;
                    int m = mediumArray[realIndex - stride];
                    lastColor.R = (byte) ((lastColor.R * m) >> 8);
                    lastColor.G = (byte) ((lastColor.G * m) >> 8);
                    lastColor.B = (byte) ((lastColor.B * m) >> 8);
                }
                var firstIndex = index * startIndexFactor;
                c = colorArray[firstIndex];
                lastColor.SetMax(c);
                colorArray[firstIndex] = lastColor;
            }
        }



        int IndexOf(int x, int y) {
            return x + this.Width * y;
        }

        int IndexOfWithCheck(int x, int y) {
            if (x < 0 || y < 0 || x >= Width || y >= Height) {
                throw new IndexOutOfRangeException();
            }

            return x + this.Width * y;
        }

        public void Dispose() {

            _colors.DisposeIfCreated();
            _medium.DisposeIfCreated();
        }
    }
}
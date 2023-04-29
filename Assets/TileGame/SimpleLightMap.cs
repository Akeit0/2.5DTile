using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
namespace TileGame.Light {
    

    [BurstCompatible]
    public unsafe class SimpleLightMap : IDisposable {
        NativeArray<byte> _colors;
        public NativeArray<byte> Colors => _colors;
        public void CopyTo(NativeArray<byte> array) {
            _colors.CopyTo(array);
        }

        NativeArray<byte> _medium;
        public NativeArray<byte> Medium => _medium;

        public int Width { get; set; }

        public int Height { get; set; }
        public bool IsCreated => _colors.IsCreated;

        public unsafe ref byte this[int x, int y] => ref ((byte*) _colors.GetUnsafePtr())[IndexOfWithThrow(x, y)];
        public unsafe ref byte GetMedium(int x, int y)=> ref ((byte*) _medium.GetUnsafePtr())[IndexOfWithThrow(x, y)];

        public SimpleLightMap(int width, int height) {
            this.Width = width;
            this.Height = height;
            this._colors = new NativeArray<byte>(width * height, Allocator.Persistent);
            this._medium = new NativeArray<byte>(width * height, Allocator.Persistent);
        }

        

        public void Clear() {
            if(_colors.IsCreated)
             _colors.Clear();
        }

        
        public void Blur() {
              BlurHV(BlurHV(default)).Complete();
           // BlurD(BlurU(BlurR(BlurL(BlurD(BlurU(BlurR(BlurL(default)))))))).Complete();
        }
      
       public void Blur1() {
           BlurHV(default).Complete();
           // BlurD(BlurU(BlurR(BlurL(BlurD(BlurU(BlurR(BlurL(default)))))))).Complete();
        }
      
      
        
        JobHandle BlurHV(JobHandle jobHandle) {
            var job = new BlurLineJob(_colors, _medium, Width, Width, 1);
            jobHandle=job.Schedule(Height, 1, jobHandle);
            job = new BlurLineJob(_colors, _medium, 1, Height, Width);
            return job.Schedule(Width,1,jobHandle);
        }


      
        [BurstCompile(CompileSynchronously = true)]
        struct BlurLineJob : IJobParallelFor {
            [NativeDisableParallelForRestriction]
            NativeArray<byte> colorArray;

            [ReadOnly,NativeDisableParallelForRestriction]
           NativeArray<byte> mediumArray;

            [ReadOnly] int StartIndexFactor;
            [ReadOnly] int PerIterationCount;
            [ReadOnly] int Stride;

            public BlurLineJob(NativeArray<byte> colorArray, NativeArray<byte> mediumArray,
                int startIndexFactor, int perIterationCount, int stride) {
                this.colorArray = colorArray;
                this.mediumArray = mediumArray;
                StartIndexFactor = startIndexFactor;
                PerIterationCount = perIterationCount;
                Stride = stride;
            }
            
            public void Execute(int index) {
                byte lastColor =  colorArray[ index * StartIndexFactor];
                for (int i = 1; i < PerIterationCount-1; ++i) {
                    var realIndex = index * StartIndexFactor + i * Stride;
                    var currentColor =colorArray[realIndex];
                    if (currentColor+lastColor==0) {
                        continue;
                    }
                    if (lastColor <= currentColor) 
                        lastColor = currentColor;
                    else  
                        colorArray[realIndex] = lastColor;
                    
                    lastColor = (byte) ((lastColor * mediumArray[realIndex+Stride]) >> 8);
                    
                }
                var lastIndex = index * StartIndexFactor + (PerIterationCount-1) * Stride;
                if (lastColor >= colorArray[lastIndex]) {
                    colorArray[lastIndex] = lastColor;
                }
                    
                
                lastColor = (byte) (colorArray[lastIndex]*mediumArray[lastIndex-Stride]>>8);
                for (int i = PerIterationCount-2;0< i; --i) {
                    var realIndex = index * StartIndexFactor + i * Stride;
                    var currentColor =colorArray[realIndex];
                    if (currentColor+lastColor==0) {
                        continue;
                    }
                    if (lastColor <= currentColor)
                        lastColor = currentColor;
                    
                    else 
                        colorArray[realIndex] = lastColor;
                    
                    lastColor = (byte) ((lastColor * mediumArray[realIndex-Stride]) >> 8);
                }
                var firstIndex = index * StartIndexFactor;
                if (lastColor >= colorArray[firstIndex]) {
                    colorArray[firstIndex] = lastColor;
                }
            }
        }
        


        int IndexOf(int x, int y) {
            return x +this.Width *y;
        }
        int IndexOfWithThrow(int x, int y) {
            if (x < 0 || y < 0 || x >= Width || y >= Height) {
                throw new IndexOutOfRangeException();
            }
            return x +this.Width *y;
        }

        public void Dispose() {
            
            _colors.DisposeIfCreated();
            _medium.DisposeIfCreated();
        }
    }
}
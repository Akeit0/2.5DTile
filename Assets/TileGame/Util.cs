using System;
using Unity.Mathematics;
using UnityEngine;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using static Unity.Mathematics.math;
namespace TileGame {
    public static class Util {
        public static void CheckIndexInRange(int index, int length) {
            if (index < 0)
                throw new IndexOutOfRangeException($"Index {index} must be positive.");

            if (index >= length)
                throw new IndexOutOfRangeException($"Index {index} is out of range in container of '{length}' Length.");
        }
        public static unsafe  void Copy<T>(ref this NativeList<T> list, int dstIndex,int srcIndex) where T : unmanaged {
            if (dstIndex == srcIndex) return;
            var unsafeList = list.GetUnsafeList();
            CheckIndexInRange(dstIndex, unsafeList->m_length);
            CheckIndexInRange(srcIndex, unsafeList->m_length);
            var sizeOf = sizeof(T);
            void* dst = (byte*)unsafeList->Ptr + dstIndex * sizeOf;
            void* src = (byte*)unsafeList->Ptr + srcIndex * sizeOf;
            UnsafeUtility.MemCpy(dst, src, sizeOf);
        }
        
        public static unsafe ref T ElementAt<T>(in this NativeArray<T> array,int index)where T :unmanaged {

            var ptr=(T*)array.GetUnsafePtr();
            return ref ptr[index];
        }
        public static unsafe  T* GetPtr<T>(in this NativeArray<T> array,int index=0)where T :unmanaged {
            var ptr=(T*)array.GetUnsafePtr();
            return  ptr+index;
        } public static unsafe  T* GetPtrWithoutChecks<T>(in this NativeArray<T> array,int index)where T :unmanaged {
            var ptr=(T*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array);
            return  ptr+index;
        }
        public static unsafe  T* GetReadOnlyPtr<T>(in this NativeArray<T> array,int index)where T :unmanaged {
            var ptr=(T*)array.GetUnsafeReadOnlyPtr();
            return  ptr+index;
        }
        public static void DisposeIfCreated<T>(ref this NativeArray<T> nativeArray)where T :struct {
            if (nativeArray.IsCreated) nativeArray.Dispose();
        }
        public static void DisposeIfCreated<T>(ref this NativeList<T> nativeList)where T :unmanaged {
            if (nativeList.IsCreated) nativeList.Dispose();
        }
         public static unsafe void SetAll<T>(this NativeArray<T> nativeArray,T value)where T :unmanaged {
             if (nativeArray.IsCreated) {
                 UnsafeUtility.MemCpyReplicate(nativeArray.GetUnsafePtr(),&value,sizeof(T),nativeArray.Length);
             }
        } 
         public static unsafe void SetAll<T>(this NativeSlice<T> slice,T value)where T :unmanaged {
           
             UnsafeUtility.MemCpyReplicate(slice.GetUnsafePtr(),&value,sizeof(T),slice.Length);
             
        } 
         
         public static unsafe void Clear<T>(this NativeArray<T> nativeArray)where T :unmanaged {
             if (nativeArray.IsCreated) {
                 UnsafeUtility.MemClear(nativeArray.GetUnsafePtr(),sizeof(T)*nativeArray.Length);
             }
        }
         public static unsafe Span<T> AsSpan<T>(this NativeArray<T> nativeArray)where T :unmanaged {
             return new Span<T>(nativeArray.GetUnsafePtr(),nativeArray.Length);
             
        }
          public static unsafe NativeArray<T> AsArray<T>(this UnsafeList<T> list)where T :unmanaged {
              var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(list.Ptr, list.m_length,list.Allocator.ToAllocator);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle ());
#endif
              return array;
             
        }
         
         
         public static unsafe  NativeArray<T> PtrToNativeArray<T>(T* ptr, int length)where T:unmanaged
         {
             var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, length, Allocator.Invalid);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle ());
#endif
             return array;
         } 
        public static void Draw(this Mesh mesh, Material material, MaterialPropertyBlock propertyBlock=null) {
            Graphics.DrawMesh(mesh, Matrix4x4.identity, material, 0,null,0,propertyBlock);
        }
        public static void Draw(this Mesh mesh,Matrix4x4 matrix4X4, Material material, MaterialPropertyBlock propertyBlock=null) {
            
            Graphics.DrawMesh(mesh, matrix4X4, material, 0,null,0,propertyBlock);
        }
        
         public static void Draw(this Mesh mesh,Vector3 pos, Material material, MaterialPropertyBlock propertyBlock=null) {
            
            Graphics.DrawMesh(mesh, Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one), material, 0,null,0,propertyBlock);
        }
        
        
        public static void Draw(this Mesh mesh, Material material, Camera camera) {
            Graphics.DrawMesh(mesh, Matrix4x4.identity, material, 0,camera,0,null);
        }
        public static void Draw(this Mesh mesh, Material material, Camera camera,int layer) {
            Graphics.DrawMesh(mesh, Matrix4x4.identity, material, layer,camera,0,null);
        }
        
        
        public static bool TryGetMouseButtonDownPosition(this Camera camera,int mouse, out Vector2 position) {
            if (Input.GetMouseButtonDown(mouse)) {

                var mousePosition = Input.mousePosition;
                mousePosition.z = -10;
                position=(Vector2) camera.ScreenToWorldPoint(mousePosition);
                return true;
            }
            position = default;
            return false;
        } 
        public static bool TryGetMouseButtonPosition(this Camera camera,int mouse, out Vector2 position) {
            if (Input.GetMouseButton(mouse)) {

                var mousePosition = Input.mousePosition;
                if(camera.orthographic)
                    mousePosition.z = -10f;
                else  mousePosition.z = -camera.transform.position.z;
                position=(Vector2) camera.ScreenToWorldPoint(mousePosition);
                return true;
            }
            position = default;
            return false;
        }
        public static bool TryGetMouseButtonPositionOnPlane(this Camera camera,int mouse, out Vector2 position) {
            if (Input.GetMouseButton(mouse)) {
                var ray = camera.ScreenPointToRay(Input.mousePosition);
                var plane = new Plane(Vector3.forward, 0);
                if (plane.Raycast(ray, out var distance))
                {
                    position = ray.GetPoint(distance);
                    return true;
                }
            }
            position = default;
            return false;
        }
        public static bool TryGetMousePositionOnPlane(this Camera camera, out Vector2 position) {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.forward, 0);
            if (plane.Raycast(ray, out var distance))
            {
                position = ray.GetPoint(distance);
                return true;
            }
            position = default;
            return false;
        }

        public static float4 GetUV(this  Sprite sprite) {
            var texture = sprite.texture;
            var w = texture.width;
            var h = texture.height;
            var rect = sprite.rect;
            float tilingX = rect.width/ w ;
            float tilingY = rect.height/ h;
            float OffsetX = (rect.x / w);
            float OffsetY = (rect.y / h);
            return new float4(OffsetX,OffsetY,tilingX,tilingY);
            
        }
        public static float4 GetUV(this  Sprite sprite,float2 padding) {
            var texture = sprite.texture;
            var w = texture.width;
            var h = texture.height;
            var rect = sprite.rect;
            float tilingX = rect.width/ w -2*padding.x;
            float tilingY = rect.height/ h-2*padding.y;
            float OffsetX = (rect.x / w)+padding.x;
            float OffsetY = (rect.y / h)+padding.y;
            return new float4(OffsetX,OffsetY,tilingX,tilingY);
            
        }
        // public static bool TryGetMouseButtonPosition(this Camera camera,int mouse, out Vector3 position) {
        //     if (Input.GetMouseButton(mouse)) {
        //         var mousePosition = Input.mousePosition;
        //         position= camera.ScreenToWorldPoint(mousePosition);
        //         return true;
        //     }
        //     position = default;
        //     return false;
        // }
        public static void Clamp0(ref this int2 input,int2 max) {
            if (input.x < 0) input.x = 0;
            else if (max.x<input.x) input.x = max.x;
            if (input.y < 0) input.y = 0;
            else if (max.y<input.y) input.y = max.y;
        } 
    }
}
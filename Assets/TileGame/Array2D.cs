using System;

namespace TileGame {
    public  class Array2D<T>where T:unmanaged{
        public readonly T[] Array;
        public readonly int Width;
        public readonly int Height;
        public Array2D(int width, int height) {
            Width = width;
            Height = height;
            Array = new T[width * height];
        }
        public ref T GetPinnableReference() => ref Array[0];
        public ref T this[int x, int y] => ref Array[IndexOfWithThrow(x, y)];
        int IndexOfWithThrow(int x, int y) {
            if (x < 0 || y < 0 || x >= Width || y >= Height) {
                throw new IndexOutOfRangeException();
            }
            return x +this.Width *y;
        }
    }
}
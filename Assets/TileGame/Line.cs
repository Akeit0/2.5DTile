using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileGame {
    [Serializable]
    public struct Line {
        public Vector2 Start;
        public Vector2 End;

        public Line(Vector2 start, Vector2 end) {
            Start = start;
            End = end;
        }

        public Line(Vector2 start, Vector2 direction, float length) {
            Start = start;
            End = start + direction.normalized * length;
        }

        public Vector2 Vector => End - Start;
        public float Dx => End.x - Start.x;
        public float Dy => End.y - Start.y;

        public float Dydx {
            get {
                var v = Vector;
                return v.y / v.x;
            }
        }

        public float Dxdy {
            get {
                var v = Vector;
                return v.x / v.y;
            }
        }

        public static Line operator +(Line left, Vector2 v) {
            return new Line(left.Start + v, left.End + v);
        }

        public static Line operator -(Line left, Vector2 v) {
            return new Line(left.Start - v, left.End - v);
        }


    }
}
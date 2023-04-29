/*The MIT License (MIT)

Copyright (c) 2015 Sebastian

Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace TileGame {
    public class Controller2D : MonoBehaviour {
        public LayerMask collisionMask;

        const float skinWidth = .015f;
        public int horizontalRayCount = 4;
        public int verticalRayCount = 4;

        float maxClimbAngle = 80;

        float horizontalRaySpacing;
        float verticalRaySpacing;

        public Vector2 BaseSize = new Vector2(1, 1);
        public Vector2 Size => BaseSize * (Vector2) transform.localScale;


        RaycastOrigins raycastOrigins;
        public CollisionInfo collisions;

        void Start() {
            CalculateRaySpacing();
        }

        public void Move(Vector3 velocity) {
            UpdateRaycastOrigins();
            collisions.Reset();

            if (velocity.x != 0) {
               
                if (UseMapCollision) {
                    Profiler.BeginSample("HorizontalCollisionsMap");
                    HorizontalCollisionsMap(ref velocity);
                    Profiler.EndSample();
                }
                else {
                    Profiler.BeginSample("HorizontalCollisions");
                    HorizontalCollisions(ref velocity);
                    Profiler.EndSample();
                }
               
            }

            if (velocity.y != 0) {
                if (UseMapCollision) {
                    Profiler.BeginSample("VerticalCollisionsMap");
                    VerticalCollisionsMap(ref velocity);
                    Profiler.EndSample();
                }
                else {
                    Profiler.BeginSample("VerticalCollisions");
                    VerticalCollisions(ref velocity);
                    Profiler.EndSample();
                }
                
            }

            if (velocity.x != 0 || velocity.y != 0)
                transform.Translate(velocity);
        }
        public bool UseMapCollision=true;
        void HorizontalCollisions(ref Vector3 velocity) {
            float directionX = Mathf.Sign(velocity.x);
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            for (int i = 0; i < horizontalRayCount; i++, rayOrigin.y += (horizontalRaySpacing)) {
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
                if (hit) {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                    if (i == 0 && slopeAngle <= maxClimbAngle) {
                        float distanceToSlopeStart = 0;
                        if (slopeAngle != collisions.slopeAngleOld) {
                            distanceToSlopeStart = hit.distance - skinWidth;
                            velocity.x -= distanceToSlopeStart * directionX;
                        }

                        ClimbSlope(ref velocity, slopeAngle);
                        velocity.x += distanceToSlopeStart * directionX;
                    }

                    if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
                        velocity.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;

                        if (collisions.climbingSlope) {
                            velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                        }

                        collisions.left = directionX == -1;
                        collisions.right = directionX == 1;
                    }
                }

            }
        }
        void HorizontalCollisionsMap(ref Vector3 velocity) {
            float directionX = Mathf.Sign(velocity.x);
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            var map=Map.Instance;
            for (int i = 0; i < horizontalRayCount; i++, rayOrigin.y += (horizontalRaySpacing)) {
                if (map.RayCastToTileHorizontal(rayOrigin, directionX*rayLength, out var distance, out var normal)) {
                    float slopeAngle = Vector2.Angle(normal, Vector2.up);
                    if (i == 0 && slopeAngle <= maxClimbAngle) {
                        float distanceToSlopeStart = 0;
                        if (slopeAngle != collisions.slopeAngleOld) {
                            distanceToSlopeStart = distance - skinWidth;
                            velocity.x -= distanceToSlopeStart * directionX;
                        }
                
                        ClimbSlope(ref velocity, slopeAngle);
                        velocity.x += distanceToSlopeStart * directionX;
                    }
                
                    if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
                        velocity.x = (distance - skinWidth) * directionX;
                        rayLength = distance;
                
                        if (collisions.climbingSlope) {
                            velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                        }
                
                        collisions.left = directionX == -1;
                        collisions.right = directionX == 1;
                    }
                }
            }
        }

        void VerticalCollisions(ref Vector3 velocity) {
            float directionY = Mathf.Sign(velocity.y);
            float rayLength = velocity.y * directionY + skinWidth;
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            for (int i = 0; i < verticalRayCount; i++, rayOrigin.x += (verticalRaySpacing + velocity.x)) {
                if (rayLength < 0f)
                    break;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
                if (hit) {
                    velocity.y = (hit.distance - skinWidth) * directionY;
                    rayLength = hit.distance - 0.01f;
                    if (velocity.x != 0 && collisions.climbingSlope) {
                        velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) *
                                     Mathf.Sign(velocity.x);
                    }

                    collisions.below = directionY == -1;
                    collisions.above = !collisions.below;
                }
            }
        }
        void VerticalCollisionsMap(ref Vector3 velocity) {
            float directionY = Mathf.Sign(velocity.y);
            float rayLength = velocity.y * directionY + skinWidth;
            var map=Map.Instance;
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            for (int i = 0; i < verticalRayCount; i++, rayOrigin.x += (verticalRaySpacing + velocity.x)) {
                if (rayLength < 0f)
                    break;
                if (map.RayCastToTileVertical(rayOrigin, directionY*rayLength, out var distance, out var normal)){
                    velocity.y = (distance - skinWidth) * directionY;
                    rayLength = distance - 0.01f;
                    if (velocity.x != 0 && collisions.climbingSlope) {
                        velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) *
                                     Mathf.Sign(velocity.x);
                    }
                    collisions.below = directionY == -1;
                    collisions.above = !collisions.below;
                }
            }
        }

        

        void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
            float moveDistance = Mathf.Abs(velocity.x);
            float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            if (velocity.y <= climbVelocityY) {
                velocity.y = climbVelocityY;
                velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                collisions.below = true;
                collisions.climbingSlope = true;
                collisions.slopeAngle = slopeAngle;
            }
        }

        void UpdateRaycastOrigins() {
            var pos = (Vector2) transform.position;
            var min = pos - Size / 2;
            var max = min + Size;
            min.x -= skinWidth;
            min.y -= skinWidth;
            max.x += skinWidth;
            max.y += skinWidth;

            raycastOrigins.center = new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2);
            raycastOrigins.halfSize = new Vector2(max.x, max.y) - raycastOrigins.center;
            raycastOrigins.bottomLeft = new Vector2(min.x, min.y);
            raycastOrigins.bottomRight = new Vector2(max.x, min.y);
            raycastOrigins.topLeft = new Vector2(min.x, max.y);
            raycastOrigins.topRight = new Vector2(max.x, max.y);
        }

        void CalculateRaySpacing() {
            var size = Size;

            horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
            verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

            horizontalRaySpacing = size.y / (horizontalRayCount - 1);
            verticalRaySpacing = size.x / (verticalRayCount - 1);
        }

        struct RaycastOrigins {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
            public Vector2 center;
            public Vector2 halfSize;
        }

        public struct CollisionInfo {
            public bool above, below;
            public bool left, right;

            public bool climbingSlope;
            public float slopeAngle, slopeAngleOld;

            public void Reset() {
                above = below = false;
                left = right = false;
                climbingSlope = false;

                slopeAngleOld = slopeAngle;
                slopeAngle = 0;
            }
        }
    }
}
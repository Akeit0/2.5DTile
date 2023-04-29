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

namespace TileGame {
    [RequireComponent (typeof (Controller2D))]
    public class Player : MonoBehaviour {

        public float jumpHeight = 4;
        public float timeToJumpApex = .4f;
        float accelerationTimeAirborne = .2f;
        public float AccelerationTimeGrounded = .1f;
        public float MoveSpeed = 6;
        public float DashSpeed = 12;
        public int MaxJumps = 2;
        int jumps;
        float gravity;
        float jumpVelocity;
        Vector2 velocity;
        float velocityXSmoothing;
        public float MaxFallSpeed = 20;
        Controller2D controller;

        void Start() {
            controller = GetComponent<Controller2D> ();
            jumps=MaxJumps;
            gravity = -(2 * jumpHeight) / Mathf.Pow (timeToJumpApex, 2);
            jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
            Camera??=Camera.main;
            if (Camera != null) Offset = Camera.transform.position - transform.position;
        }

        void Update() {

            if (controller.collisions.above || controller.collisions.below) {
                velocity.y = 0;
            }

            Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
            if (controller.collisions.below) {
                jumps=MaxJumps;
            }
            if (Input.GetKeyDown (KeyCode.Space) && (0<jumps)) {
                velocity.y = jumpVelocity;
                --jumps;
            }
            var moveSpeed = Input.GetKey(KeyCode.LeftShift)?DashSpeed:MoveSpeed;
            float targetVelocityX = input.x * moveSpeed;
            velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?AccelerationTimeGrounded:accelerationTimeAirborne);
            velocity.y += gravity * Time.deltaTime;
            velocity.y = Mathf.Clamp(velocity.y,-MaxFallSpeed,jumpVelocity*2);
            controller.Move (velocity * Time.deltaTime);
        }
        public Vector3 Offset;
        Vector2 currentSpeed;
        public Camera Camera;
        public float CameraSmoothRadius=20;
        public float CameraSmoothTime=1f;
        void LateUpdate() {
            var t = Camera.transform;
            var currentPos=t.position;
            var targetPos = transform.position+Offset;
            var deltaSqrMagnitude = (targetPos - currentPos).sqrMagnitude;
            if(deltaSqrMagnitude<0.1f) return;
           var deltaMagnitude = Mathf.Sqrt(deltaSqrMagnitude);
           var smoothTime=CameraSmoothTime* (deltaMagnitude < CameraSmoothRadius ? 1f : CameraSmoothRadius / deltaMagnitude);
            var newPos=  Vector2.SmoothDamp(currentPos, targetPos, ref currentSpeed, smoothTime);

            if (!float.IsFinite(newPos.x) || !float.IsFinite(newPos.x)) {
                currentSpeed = default;
            }
            else {
                t.position =  new Vector3(newPos.x,newPos.y,targetPos.z);
            }
          
        }
    }
}
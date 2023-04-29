using System;
using UnityEngine;

namespace TileGame {

    public class BallController :MonoBehaviour {
        public float Speed = 1f;
        public float JumpSpeed = 1f;
        public float Gravity = 100f;
        public Vector2 Velocity;
        public Camera Camera;
        public float Width;
        public float Height;
        void Start() {
            Camera??=Camera.main;
            if (Camera != null) Offset = Camera.transform.position - transform.position;
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                Velocity.y = JumpSpeed;
            }
            if (Input.GetKey(KeyCode.A)) {
                Velocity.x=-Speed;
            }else if (Input.GetKey(KeyCode.D)) {
                Velocity.x=Speed;
            }
            Velocity.y -= Gravity * Time.deltaTime;
            var delta = Velocity * Time.deltaTime;
            var pos=(Vector2)transform.position;
            
            if(delta.x<0) {
                var hit = Physics2D.BoxCast(pos-new Vector2(Width/2+0.01f,0),new Vector2(0.1f,Height), 0, Vector2.left,-delta.x);
                if (hit.collider != null) {
                    Velocity.x = 0;
                    delta.y-= delta.x * hit.normal.y;
                    if(delta.y!=0)
                     Velocity.y=delta.y/Time.deltaTime;
                    delta.x  =- hit.distance;
                }
            }else if(0<delta.x) {
                var hit = Physics2D.BoxCast(pos+new Vector2(Width/2+0.01f,0),new Vector2(0.1f,Height),  0, Vector2.right,delta.x);
                if (hit.collider != null) {
                    Velocity.x = 0;
                    delta.y+= delta.x * hit.normal.y;
                    if(delta.y!=0)
                     Velocity.y=delta.y/Time.deltaTime;
                    delta.x  =+ hit.distance;
                }
            }
            if(delta.y<0) {
                var hit = Physics2D.BoxCast(pos-new Vector2(0,Height/2+0.01f),new Vector2(Width,0.1f), 0, Vector2.down,-delta.y);
                if (hit.collider != null) {
                    Velocity.y = 0;
                    delta.y  =- hit.distance;
                }
            }else if(0<delta.y) {
                var hit = Physics2D.BoxCast(pos+new Vector2(0,Height/2+0.01f),new Vector2(Width,0.1f), 0, Vector2.up,delta.y);
                if (hit.collider != null) {
                    Velocity.y = 0;
                    delta.y  =+ hit.distance;
                }
            }
            pos+=delta;
            transform.position = pos;
        }
     
        public Vector3 Offset;
         Vector2 currentSpeed;
        void LateUpdate() {
            var t = Camera.transform;
            var currentPos=t.position;
            var targetPos = transform.position+Offset;
          var newPos=  Vector2.SmoothDamp(currentPos, targetPos, ref currentSpeed, 1);
            t.position =  new Vector3(newPos.x,newPos.y,targetPos.z);
        }
    }
}
using System;
using UnityEngine;

namespace TileGame {
    public class FPSController : MonoBehaviour {
        public int TargetFrameRate = 60;

        void Start() {
            Application.targetFrameRate = TargetFrameRate;
        }
    }
}
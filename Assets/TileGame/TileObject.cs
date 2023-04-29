using System;
using UnityEngine;

namespace TileGame {
    [CreateAssetMenu(fileName = "TileObject", menuName = "TileGame/TileObject", order = 0)]
    public class TileObject :ScriptableObject{
        public Sprite Sprite;
        public TileCollision Collision;

        // void OnValidate() {
        //     throw new NotImplementedException();
        // }
    }
}
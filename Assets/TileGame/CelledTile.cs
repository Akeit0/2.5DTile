using System;
using UnityEngine;
namespace TileGame {
    [CreateAssetMenu(fileName = "CelledTile", menuName = "TileGame/CelledTile", order = 0)]
    public class CelledTile:ScriptableObject {
        [Serializable]
        public struct TileData {
            public string Name;
            public TileCollision Collision;
        }
        public Texture2D Texture;
        public int PixelPerUnit;
        public TileData[] TileDataArray;
    }
}
using System.Runtime.InteropServices;

namespace TileGame {

 
   // [StructLayout(LayoutKind.Explicit)]
    public struct Tile {
        public ushort Wall;
        public ushort Block;
        public TileCollision Collision;
        public ushort FrontWall;
        public Tile (ushort type, ushort wall, ushort frontWall) {
            Block = type;
            Wall = wall;
            Collision = TileCollision.None;
            FrontWall = frontWall;
        }

        public  ushort this[int layer] {
            get{
                switch (layer) {
                    case 0:
                        return  Wall;
                    case 1:
                        return Block;
                    case 2:
                        return FrontWall;
                    default:
                        return 0;
                }
            }
            set {
                switch (layer) {
                    case 0:
                        Wall=value;
                        return;
                    case 1:
                        Block=value;
                        return;
                    case 2:
                        FrontWall=value;
                        return;
                    default:
                        return ;
                }
            }
        }

        public static Tile NewBlock(ushort type) => new Tile() {Block = type};
        public static Tile NewWall(ushort type) => new Tile() {Wall = type};
        public static Tile NewFrontWall(ushort type) => new Tile() {FrontWall = type};
       
        public bool IsEmpty => Block == 0 && Wall == 0 && FrontWall == 0;
    }
}
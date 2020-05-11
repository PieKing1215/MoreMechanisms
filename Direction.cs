using System.Collections.Generic;

namespace MoreMechanisms {
    public class Direction {

        public static Direction NONE  = new Direction( 0,  0, "None");
        public static Direction UP    = new Direction( 0, -1, "Up");
        public static Direction DOWN  = new Direction( 0,  1, "Down");
        public static Direction LEFT  = new Direction(-1,  0, "Left");
        public static Direction RIGHT = new Direction( 1,  0, "Right");
        public static List<Direction> DIRECTIONS = new List<Direction> { UP, DOWN, LEFT, RIGHT };

        public int dx;
        public int dy;
        public string label;

        public Direction Opposite {
            get {
                if(this == UP) {
                    return DOWN;
                }else if(this == DOWN) {
                    return UP;
                }else if(this == LEFT) {
                    return RIGHT;
                }else if(this == RIGHT) {
                    return LEFT;
                }
                return NONE;
            }
        }

        public Direction Clockwise {
            get {
                if (this == UP) {
                    return RIGHT;
                } else if (this == DOWN) {
                    return LEFT;
                } else if (this == LEFT) {
                    return UP;
                } else if (this == RIGHT) {
                    return DOWN;
                }
                return NONE;
            }
        }
        
        public Direction CounterClockwise {
            get {
                if (this == UP) {
                    return LEFT;
                } else if (this == DOWN) {
                    return RIGHT;
                } else if (this == LEFT) {
                    return DOWN;
                } else if (this == RIGHT) {
                    return UP;
                }
                return NONE;
            }
        }

        private Direction(int dx, int dy, string label) {
            this.dx = dx;
            this.dy = dy;
            this.label = label;
        }

        public override string ToString() {
            return label + "<" + dx + "," + dy + ">";
        }
    }
}

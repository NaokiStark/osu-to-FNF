using V2 = System.Numerics.Vector2;
namespace osuBMParser
{       
    public class Vector2
    {
        V2 vect = new V2();
        public float x {
            get
            {
                return vect.X;
            }
            set
            {
                vect.X = value;
            }
        }
        public float y
        {
            get
            {
                return vect.Y;
            }
            set
            {
                vect.Y = value;
            }
        }

        public Vector2() {
            vect = new V2();
        }

        public Vector2(float val)
        {
            vect = new V2(val);
        }

        public Vector2(float x, float y)
        {
            vect = new V2(x, y);
        }

    }
}

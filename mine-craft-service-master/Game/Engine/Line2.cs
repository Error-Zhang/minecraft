namespace Engine
{
    public struct Line2 : IEquatable<Line2>
    {
        public Vector2 Normal;

        public float D;

        public Line2(Vector2 normal, float d)
        {
            Normal = normal;
            D = d;
        }

        public Line2(Vector2 p1, Vector2 p2)
        {
            Normal = Vector2.Normalize(new Vector2(p1.Y - p2.Y, p2.X - p1.X));
            D = 0f - Vector2.Dot(Normal, p1);
        }

        public Line2(float x, float y, float d)
        : this(new Vector2(x, y), d)
        {
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Line2))
            {
                return false;
            }
            return Equals((Line2)obj);
        }

        public bool Equals(Line2 other)
        {
            if (Normal == other.Normal)
            {
                return D == other.D;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Normal.GetHashCode() + D.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Normal.X},{Normal.Y},{D}";
        }

        public float SignedDistance(Vector2 p)
        {
            return Vector2.Dot(Normal, p) + D;
        }

        public static Line2 Normalize(Line2 l)
        {
            float num = l.Normal.Length();
            if (num > 0f)
            {
                float num2 = 1f / num;
                return new Line2(l.Normal * num2, l.D * num2);
            }
            return new Line2(Vector2.UnitX, 0f);
        }

        public static Vector2? Intersection(Line2 l1, Line2 l2)
        {
            float num = 0f - Vector2.Cross(l1.Normal, l2.Normal);
            if (num == 0f)
            {
                return null;
            }
            float num2 = 1f / num;
            return new Vector2((l2.Normal.Y * l1.D - l1.Normal.Y * l2.D) * num2, (l1.Normal.X * l2.D - l2.Normal.X * l1.D) * num2);
        }

        public static bool operator ==(Line2 l1, Line2 l2)
        {
            return l1.Equals(l2);
        }

        public static bool operator !=(Line2 l1, Line2 l2)
        {
            return !l1.Equals(l2);
        }
    }
}
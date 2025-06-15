using System;

namespace Engine
{
	public struct BoundingCircle : IEquatable<BoundingCircle>
	{
		public Vector2 Center;

		public float Radius;

		public BoundingCircle(Vector2 center, float radius)
		{
			Center = center;
			Radius = radius;
		}

		public override bool Equals(object obj)
		{
            return obj is BoundingCircle circle && Equals(circle);
        }

        public override int GetHashCode()
		{
			return Center.GetHashCode() + Radius.GetHashCode();
		}

		public bool Equals(BoundingCircle other)
		{
            return Center == other.Center && Radius == other.Radius;
        }

        public override string ToString()
		{
			return $"{Center},{Radius}";
		}

        public bool Contains(Vector2 p)
        {
            return Vector2.DistanceSquared(Center, p) <= Radius * Radius;
        }

		public static bool operator ==(BoundingCircle c1, BoundingCircle c2)
		{
			return c1.Equals(c2);
		}

		public static bool operator !=(BoundingCircle c1, BoundingCircle c2)
		{
			return !c1.Equals(c2);
		}
	}
}
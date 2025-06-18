using System;

namespace Engine
{
	public struct BoundingSphere : IEquatable<BoundingSphere>
	{
		public Vector3 Center;

		public float Radius;

		public BoundingSphere(Vector3 center, float radius)
		{
			Center = center;
			Radius = radius;
		}

		public override bool Equals(object obj)
		{
            return obj is BoundingSphere sphere && Equals(sphere);
        }

        public override int GetHashCode()
		{
			return Center.GetHashCode() + Radius.GetHashCode();
		}

		public bool Equals(BoundingSphere other)
		{
            return Center == other.Center && Radius == other.Radius;
        }

        public override string ToString()
		{
			return $"{Center},{Radius}";
		}

		public static bool operator ==(BoundingSphere s1, BoundingSphere s2)
		{
			return s1.Equals(s2);
		}

		public static bool operator !=(BoundingSphere s1, BoundingSphere s2)
		{
			return !s1.Equals(s2);
		}
	}
}
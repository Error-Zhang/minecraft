using System;

namespace Engine
{
	public struct Point3 : IEquatable<Point3>
	{
		public int X;

		public int Y;

		public int Z;

		public static readonly Point3 Zero = default(Point3);

		public static readonly Point3 One = new(1, 1, 1);

		public static readonly Point3 UnitX = new(1, 0, 0);

		public static readonly Point3 UnitY = new(0, 1, 0);

		public static readonly Point3 UnitZ = new(0, 0, 1);

		public Point3(int v)
		{
			X = v;
			Y = v;
			Z = v;
		}

		public Point3(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Point3(Vector3 v)
		{
			X = (int)v.X;
			Y = (int)v.Y;
			Z = (int)v.Z;
		}

		public static implicit operator Point3((int X, int Y, int Z) v)
		{
			return new Point3(v.X, v.Y, v.Z);
		}

		public override int GetHashCode()
		{
			return X + Y + Z;
		}

		public override bool Equals(object obj)
		{
            return obj is Point3 && Equals((Point3)obj);
        }

        public bool Equals(Point3 other)
		{
            return other.X == X && other.Y == Y && other.Z == Z;
        }

        public override string ToString()
		{
			return $"{X},{Y},{Z}";
		}

        public static int Dot(Point3 p1, Point3 p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z;
        }

        public static Point3 Cross(Point3 p1, Point3 p2)
        {
            return new Point3(p1.Y * p2.Z - p1.Z * p2.Y, p1.Z * p2.X - p1.X * p2.Z, p1.X * p2.Y - p1.Y * p2.X);
        }

		public static Point3 Min(Point3 p, int v)
		{
			return new Point3(MathUtils.Min(p.X, v), MathUtils.Min(p.Y, v), MathUtils.Min(p.Z, v));
		}

		public static Point3 Min(Point3 p1, Point3 p2)
		{
			return new Point3(MathUtils.Min(p1.X, p2.X), MathUtils.Min(p1.Y, p2.Y), MathUtils.Min(p1.Z, p2.Z));
		}

		public static Point3 Max(Point3 p, int v)
		{
			return new Point3(MathUtils.Max(p.X, v), MathUtils.Max(p.Y, v), MathUtils.Max(p.Z, v));
		}

		public static Point3 Max(Point3 p1, Point3 p2)
		{
			return new Point3(MathUtils.Max(p1.X, p2.X), MathUtils.Max(p1.Y, p2.Y), MathUtils.Max(p1.Z, p2.Z));
		}

        public static int MinElement(Point3 p)
        {
            return MathUtils.Min(p.X, p.Y, p.Z);
        }

        public static int MaxElement(Point3 p)
        {
            return MathUtils.Max(p.X, p.Y, p.Z);
        }

		public static bool operator ==(Point3 p1, Point3 p2)
		{
			return p1.Equals(p2);
		}

		public static bool operator !=(Point3 p1, Point3 p2)
		{
			return !p1.Equals(p2);
		}

		public static Point3 operator +(Point3 p)
		{
			return p;
		}

		public static Point3 operator -(Point3 p)
		{
			return new Point3(-p.X, -p.Y, -p.Z);
		}

		public static Point3 operator +(Point3 p1, Point3 p2)
		{
			return new Point3(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
		}

		public static Point3 operator -(Point3 p1, Point3 p2)
		{
			return new Point3(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
		}

		public static Point3 operator *(int n, Point3 p)
		{
			return new Point3(p.X * n, p.Y * n, p.Z * n);
		}

		public static Point3 operator *(Point3 p, int n)
		{
			return new Point3(p.X * n, p.Y * n, p.Z * n);
		}

		public static Point3 operator *(Point3 p1, Point3 p2)
		{
			return new Point3(p1.X * p2.X, p1.Y * p2.Y, p1.Z * p2.Z);
		}

		public static Point3 operator /(Point3 p, int n)
		{
			return new Point3(p.X / n, p.Y / n, p.Z / n);
		}

		public static Point3 operator /(Point3 p1, Point3 p2)
		{
			return new Point3(p1.X / p2.X, p1.Y / p2.Y, p1.Z / p2.Z);
		}
	}
}
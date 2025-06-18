using System;

namespace Engine
{
	public struct Point2 : IEquatable<Point2>
	{
		public int X;

		public int Y;

		public static readonly Point2 Zero = default(Point2);

		public static readonly Point2 One = new(1, 1);

		public static readonly Point2 UnitX = new(1, 0);

		public static readonly Point2 UnitY = new(0, 1);

		public Point2(int v)
		{
			X = v;
			Y = v;
		}

		public Point2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static implicit operator Point2((int X, int Y) v)
		{
			return new Point2(v.X, v.Y);
		}

		public override int GetHashCode()
		{
			return X + Y;
		}

		public override bool Equals(object obj)
		{
            return obj is Point2 && Equals((Point2)obj);
        }

        public bool Equals(Point2 other)
		{
            return other.X == X && other.Y == Y;
        }

        public override string ToString()
		{
			return $"{X},{Y}";
		}

        public static int Dot(Point2 p1, Point2 p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }

        public static int Cross(Point2 p1, Point2 p2)
        {
            return p1.X * p2.Y - p1.Y * p2.X;
        }

        public static Point2 Perpendicular(Point2 p)
        {
            return new Point2(-p.Y, p.X);
        }

		public static Point2 Min(Point2 p, int v)
		{
			return new Point2(MathUtils.Min(p.X, v), MathUtils.Min(p.Y, v));
		}

		public static Point2 Min(Point2 p1, Point2 p2)
		{
			return new Point2(MathUtils.Min(p1.X, p2.X), MathUtils.Min(p1.Y, p2.Y));
		}

		public static Point2 Max(Point2 p, int v)
		{
			return new Point2(MathUtils.Max(p.X, v), MathUtils.Max(p.Y, v));
		}

		public static Point2 Max(Point2 p1, Point2 p2)
		{
			return new Point2(MathUtils.Max(p1.X, p2.X), MathUtils.Max(p1.Y, p2.Y));
		}

        public static int MinElement(Point2 p)
        {
            return MathUtils.Min(p.X, p.Y);
        }

        public static int MaxElement(Point2 p)
        {
            return MathUtils.Max(p.X, p.Y);
        }

		public static bool operator ==(Point2 p1, Point2 p2)
		{
			return p1.Equals(p2);
		}

		public static bool operator !=(Point2 p1, Point2 p2)
		{
			return !p1.Equals(p2);
		}

		public static Point2 operator +(Point2 p)
		{
			return p;
		}

		public static Point2 operator -(Point2 p)
		{
			return new Point2(-p.X, -p.Y);
		}

		public static Point2 operator +(Point2 p1, Point2 p2)
		{
			return new Point2(p1.X + p2.X, p1.Y + p2.Y);
		}

		public static Point2 operator -(Point2 p1, Point2 p2)
		{
			return new Point2(p1.X - p2.X, p1.Y - p2.Y);
		}

		public static Point2 operator *(int n, Point2 p)
		{
			return new Point2(p.X * n, p.Y * n);
		}

		public static Point2 operator *(Point2 p, int n)
		{
			return new Point2(p.X * n, p.Y * n);
		}

		public static Point2 operator *(Point2 p1, Point2 p2)
		{
			return new Point2(p1.X * p2.X, p1.Y * p2.Y);
		}

		public static Point2 operator /(Point2 p, int n)
		{
			return new Point2(p.X / n, p.Y / n);
		}

		public static Point2 operator /(Point2 p1, Point2 p2)
		{
			return new Point2(p1.X / p2.X, p1.Y / p2.Y);
		}
	}
}
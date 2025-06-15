using System;
using System.Collections.Generic;

namespace Engine
{
	public struct BoundingRectangle : IEquatable<BoundingRectangle>
	{
		public Vector2 Min;

		public Vector2 Max;

        public IEnumerable<Vector2> Corners
        {
            get
            {
                yield return new Vector2(Min.X, Min.Y);
                yield return new Vector2(Max.X, Min.Y);
                yield return new Vector2(Min.X, Max.Y);
                yield return new Vector2(Max.X, Max.Y);
            }
        }

		public BoundingRectangle(float x1, float y1, float x2, float y2)
		{
			Min = new Vector2(x1, y1);
			Max = new Vector2(x2, y2);
		}

		public BoundingRectangle(Vector2 min, Vector2 max)
		{
			Min = min;
			Max = max;
		}

		public BoundingRectangle(IEnumerable<Vector2> points)
		{
			ArgumentNullException.ThrowIfNull(points);
			Min = new Vector2(float.PositiveInfinity);
			Max = new Vector2(float.NegativeInfinity);
			foreach (Vector2 point in points)
			{
				Min.X = MathUtils.Min(Min.X, point.X);
				Min.Y = MathUtils.Min(Min.Y, point.Y);
				Max.X = MathUtils.Max(Max.X, point.X);
				Max.Y = MathUtils.Max(Max.Y, point.Y);
			}
			if (Min.X == float.PositiveInfinity)
			{
				throw new ArgumentException("points");
			}
		}

		public static implicit operator BoundingRectangle((float X1, float Y1, float X2, float Y2) v)
		{
			return new BoundingRectangle(v.X1, v.Y1, v.X2, v.Y2);
		}

		public override bool Equals(object obj)
		{
            return obj is BoundingRectangle rectangle && Equals(rectangle);
        }

        public override int GetHashCode()
		{
			return Min.GetHashCode() + Max.GetHashCode();
		}

		public override string ToString()
		{
			return $"{Min},{Max}";
		}

		public bool Equals(BoundingRectangle other)
		{
            return Min == other.Min && Max == other.Max;
        }

        public Vector2 Center()
		{
			return new Vector2(0.5f * (Min.X + Max.X), 0.5f * (Min.Y + Max.Y));
		}

		public Vector2 Size()
		{
			return Max - Min;
		}

		public float Area()
		{
			Vector2 vector = Size();
			return vector.X * vector.Y;
		}

		public bool Contains(Vector2 p)
		{
            return p.X >= Min.X && p.X <= Max.X && p.Y >= Min.Y && p.Y <= Max.Y;
        }

        public bool Intersection(BoundingRectangle r)
		{
            return r.Max.X >= Min.X && r.Min.X <= Max.X && r.Max.Y >= Min.Y && r.Min.Y <= Max.Y;
        }

        public bool Intersection(BoundingCircle circle)
		{
			float num = circle.Center.X - Math.Clamp(circle.Center.X, Min.X, Max.X);
			float num2 = circle.Center.Y - Math.Clamp(circle.Center.Y, Min.Y, Max.Y);
			return (num * num) + (num2 * num2) <= circle.Radius * circle.Radius;
		}

		public static BoundingRectangle Intersection(BoundingRectangle r1, BoundingRectangle r2)
		{
			Vector2 min = Vector2.Max(r1.Min, r2.Min);
			Vector2 max = Vector2.Min(r1.Max, r2.Max);
            return !(max.X > min.X) || !(max.Y > min.Y) ? default : new BoundingRectangle(min, max);
        }

        public static BoundingRectangle Union(BoundingRectangle r1, BoundingRectangle r2)
		{
			Vector2 min = Vector2.Min(r1.Min, r2.Min);
			Vector2 max = Vector2.Max(r1.Max, r2.Max);
			return new BoundingRectangle(min, max);
		}

		public static BoundingRectangle Union(BoundingRectangle r, Vector2 p)
		{
			Vector2 min = Vector2.Min(r.Min, p);
			Vector2 max = Vector2.Max(r.Max, p);
			return new BoundingRectangle(min, max);
		}

		public static float Distance(BoundingRectangle r, Vector2 p)
		{
			float num = MathUtils.Max(r.Min.X - p.X, 0f, p.X - r.Max.X);
			float num2 = MathUtils.Max(r.Min.Y - p.Y, 0f, p.Y - r.Max.Y);
			return MathF.Sqrt((num * num) + (num2 * num2));
		}

		public static bool operator ==(BoundingRectangle a, BoundingRectangle b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(BoundingRectangle a, BoundingRectangle b)
		{
			return !a.Equals(b);
		}
	}
}
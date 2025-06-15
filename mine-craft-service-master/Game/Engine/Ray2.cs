
using System;

namespace Engine
{
	public struct Ray2 : IEquatable<Ray2>
	{
		public Vector2 Position;

		public Vector2 Direction;

		public Ray2(Vector2 position, Vector2 direction)
		{
			Position = position;
			Direction = direction;
		}

		public override bool Equals(object obj)
		{
            return obj is Ray2 && Equals((Ray2)obj);
        }

        public override int GetHashCode()
		{
			return Position.GetHashCode() + Direction.GetHashCode();
		}

		public override string ToString()
		{
			return $"{Position.ToString()},{Direction.ToString()}";
		}

		public bool Equals(Ray2 other)
		{
            return Position == other.Position && Direction == other.Direction;
        }

        public float? Intersection(BoundingRectangle rectangle)
        {
            float num = 0f;
            if (Direction.X == 0f)
            {
                if (Position.X < rectangle.Min.X || Position.X > rectangle.Max.X)
                {
                    return null;
                }
            }
            else
            {
                float num2 = 1f / Direction.X;
                float num3 = (rectangle.Min.X - Position.X) * num2;
                float num4 = (rectangle.Max.X - Position.X) * num2;
                if (num3 > num4)
                {
                    float num5 = num3;
                    num3 = num4;
                    num4 = num5;
                }
                num = MathUtils.Max(num3, num);
                if (num > num4)
                {
                    return null;
                }
            }
            if (Direction.Y == 0f)
            {
                if (Position.Y < rectangle.Min.Y || Position.Y > rectangle.Max.Y)
                {
                    return null;
                }
            }
            else
            {
                float num6 = 1f / Direction.Y;
                float num7 = (rectangle.Min.Y - Position.Y) * num6;
                float num8 = (rectangle.Max.Y - Position.Y) * num6;
                if (num7 > num8)
                {
                    float num9 = num7;
                    num7 = num8;
                    num8 = num9;
                }
                num = MathUtils.Max(num7, num);
                if (num > num8)
                {
                    return null;
                }
            }
            return num;
        }

        public float? Intersection(BoundingCircle circle)
        {
            Vector2 v = circle.Center - Position;
            float num = v.LengthSquared();
            float num2 = circle.Radius * circle.Radius;
            if (num < num2)
            {
                return 0f;
            }
            float num3 = Vector2.Dot(Direction, v);
            if (num3 < 0f)
            {
                return null;
            }
            float num4 = num2 + num3 * num3 - num;
            if (!(num4 < 0f))
            {
                return num3 - MathF.Sqrt(num4);
            }
            return null;
        }

        public Vector2 Sample(float distance)
        {
            return Position + Direction * distance;
        }

        public static Ray2 Transform(Ray2 r, Matrix m)
        {
            Transform(ref r, ref m, out var result);
            return result;
        }

        public static void Transform(ref Ray2 r, ref Matrix m, out Ray2 result)
        {
            Vector2.Transform(ref r.Position, ref m, out result.Position);
            Vector2.TransformNormal(ref r.Direction, ref m, out result.Direction);
        }

        public static bool operator ==(Ray2 a, Ray2 b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Ray2 a, Ray2 b)
		{
			return !a.Equals(b);
		}
	}
}
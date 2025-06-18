using System;
using System.Collections.Generic;

namespace Engine
{
	public struct BoundingBox : IEquatable<BoundingBox>
	{
		public Vector3 Min;

		public Vector3 Max;

        public IEnumerable<Vector3> Corners
        {
            get
            {
                yield return new Vector3(Min.X, Min.Y, Min.Z);
                yield return new Vector3(Max.X, Min.Y, Min.Z);
                yield return new Vector3(Min.X, Max.Y, Min.Z);
                yield return new Vector3(Max.X, Max.Y, Min.Z);
                yield return new Vector3(Min.X, Min.Y, Max.Z);
                yield return new Vector3(Max.X, Min.Y, Max.Z);
                yield return new Vector3(Min.X, Max.Y, Max.Z);
                yield return new Vector3(Max.X, Max.Y, Max.Z);
            }
        }

		public BoundingBox(float x1, float y1, float z1, float x2, float y2, float z2)
		{
			Min = new Vector3(x1, y1, z1);
			Max = new Vector3(x2, y2, z2);
		}

		public BoundingBox(Vector3 min, Vector3 max)
		{
			Min = min;
			Max = max;
		}

		public BoundingBox(IEnumerable<Vector3> points)
		{
			ArgumentNullException.ThrowIfNull(points);
			Min = new Vector3(float.PositiveInfinity);
			Max = new Vector3(float.NegativeInfinity);
			foreach (Vector3 point in points)
			{
				Min.X = MathF.Min(Min.X, point.X);
				Min.Y = MathF.Min(Min.Y, point.Y);
				Min.Z = MathF.Min(Min.Z, point.Z);
				Max.X = MathF.Max(Max.X, point.X);
				Max.Y = MathF.Max(Max.Y, point.Y);
				Max.Z = MathF.Max(Max.Z, point.Z);
			}
			if (Min.X == float.PositiveInfinity)
			{
				throw new ArgumentException("points");
			}
		}

		public static implicit operator BoundingBox((float X1, float Y1, float Z1, float X2, float Y2, float Z2) v)
		{
			return new BoundingBox(v.X1, v.Y1, v.Z1, v.X2, v.Y2, v.Z2);
		}

		public override bool Equals(object obj)
		{
            return obj is BoundingBox box && Equals(box);
        }

        public override int GetHashCode()
		{
			return Min.GetHashCode() + Max.GetHashCode();
		}

		public override string ToString()
		{
			return $"{Min},{Max}";
		}

		public bool Equals(BoundingBox other)
		{
            return Min == other.Min && Max == other.Max;
        }

        public Vector3 Center()
		{
			return new Vector3(0.5f * (Min.X + Max.X), 0.5f * (Min.Y + Max.Y), 0.5f * (Min.Z + Max.Z));
		}

		public Vector3 Size()
		{
			return Max - Min;
		}

		public float Volume()
		{
			Vector3 vector = Size();
			return vector.X * vector.Y * vector.Z;
		}

		public bool Contains(Vector3 p)
		{
            return p.X >= Min.X && p.X <= Max.X && p.Y >= Min.Y && p.Y <= Max.Y && p.Z >= Min.Z && p.Z <= Max.Z;
        }

        public bool Intersection(BoundingBox box)
		{
            return box.Max.X >= Min.X && box.Min.X <= Max.X && box.Max.Y >= Min.Y && box.Min.Y <= Max.Y && box.Max.Z >= Min.Z&& box.Min.Z <= Max.Z;
        }

        public bool Intersection(BoundingSphere sphere)
		{
			if (sphere.Center.X - Min.X > sphere.Radius && sphere.Center.Y - Min.Y > sphere.Radius && sphere.Center.Z - Min.Z > sphere.Radius && Max.X - sphere.Center.X > sphere.Radius && Max.Y - sphere.Center.Y > sphere.Radius && Max.Z - sphere.Center.Z > sphere.Radius)
			{
				return true;
			}
			float num = 0f;
			if (sphere.Center.X - Min.X <= sphere.Radius)
			{
				num += (sphere.Center.X - Min.X) * (sphere.Center.X - Min.X);
			}
			else if (Max.X - sphere.Center.X <= sphere.Radius)
			{
				num += (sphere.Center.X - Max.X) * (sphere.Center.X - Max.X);
			}
			if (sphere.Center.Y - Min.Y <= sphere.Radius)
			{
				num += (sphere.Center.Y - Min.Y) * (sphere.Center.Y - Min.Y);
			}
			else if (Max.Y - sphere.Center.Y <= sphere.Radius)
			{
				num += (sphere.Center.Y - Max.Y) * (sphere.Center.Y - Max.Y);
			}
			if (sphere.Center.Z - Min.Z <= sphere.Radius)
			{
				num += (sphere.Center.Z - Min.Z) * (sphere.Center.Z - Min.Z);
			}
			else if (Max.Z - sphere.Center.Z <= sphere.Radius)
			{
				num += (sphere.Center.Z - Max.Z) * (sphere.Center.Z - Max.Z);
			}
            return num <= sphere.Radius * sphere.Radius;
        }

        public static BoundingBox Intersection(BoundingBox b1, BoundingBox b2)
		{
			Vector3 min = Vector3.Max(b1.Min, b2.Min);
			Vector3 max = Vector3.Min(b1.Max, b2.Max);
            return !(max.X > min.X) || !(max.Y > min.Y) || !(max.Z > min.Z) ? default(BoundingBox) : new BoundingBox(min, max);
        }

        public static BoundingBox Union(BoundingBox b1, BoundingBox b2)
		{
			Vector3 min = Vector3.Min(b1.Min, b2.Min);
			Vector3 max = Vector3.Max(b1.Max, b2.Max);
			return new BoundingBox(min, max);
		}

		public static BoundingBox Union(BoundingBox b, Vector3 p)
		{
			Vector3 min = Vector3.Min(b.Min, p);
			Vector3 max = Vector3.Max(b.Max, p);
			return new BoundingBox(min, max);
		}

		public static float Distance(BoundingBox b, Vector3 p)
		{
			float num = MathUtils.Max(b.Min.X - p.X, 0f, p.X - b.Max.X);
			float num2 = MathUtils.Max(b.Min.Y - p.Y, 0f, p.Y - b.Max.Y);
			float num3 = MathUtils.Max(b.Min.Z - p.Z, 0f, p.Z - b.Max.Z);
			return MathF.Sqrt((num * num) + (num2 * num2) + (num3 * num3));
		}

		public static BoundingBox Transform(BoundingBox b, Matrix m)
		{
			Transform(ref b, ref m, out BoundingBox result);
			return result;
		}

		public static void Transform(ref BoundingBox b, ref Matrix m, out BoundingBox result)
		{
			Vector3[] sourceArray =
            [
                new(b.Min.X, b.Min.Y, b.Min.Z),
				new(b.Max.X, b.Min.Y, b.Min.Z),
				new(b.Min.X, b.Max.Y, b.Min.Z),
				new(b.Max.X, b.Max.Y, b.Min.Z),
				new(b.Min.X, b.Min.Y, b.Max.Z),
				new(b.Max.X, b.Min.Y, b.Max.Z),
				new(b.Min.X, b.Max.Y, b.Max.Z),
				new(b.Max.X, b.Max.Y, b.Max.Z)
			];
			Vector3[] array = new Vector3[8];
			Vector3.Transform(sourceArray, 0, ref m, array, 0, 8);
			result = new BoundingBox(array);
		}

		public static bool operator ==(BoundingBox a, BoundingBox b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(BoundingBox a, BoundingBox b)
		{
			return !a.Equals(b);
		}
	}
}

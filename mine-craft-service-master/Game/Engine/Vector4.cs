using System;

namespace Engine
{
	public struct Vector4 : IEquatable<Vector4>
	{
		public float X;

		public float Y;

		public float Z;

		public float W;

		public static readonly Vector4 Zero = new(0f);

		public static readonly Vector4 One = new(1f);

		public static readonly Vector4 UnitX = new(1f, 0f, 0f, 0f);

		public static readonly Vector4 UnitY = new(0f, 1f, 0f, 0f);

		public static readonly Vector4 UnitZ = new(0f, 0f, 1f, 0f);

		public static readonly Vector4 UnitW = new(0f, 0f, 0f, 1f);

		public Vector4(float v)
		{
			X = v;
			Y = v;
			Z = v;
			W = v;
		}

		public Vector4(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public Vector4(Vector3 xyz, float w)
		{
			X = xyz.X;
			Y = xyz.Y;
			Z = xyz.Z;
			W = w;
		}

		public Vector4(Color c)
		{
			X = c.R / 255f;
			Y = c.G / 255f;
			Z = c.B / 255f;
			W = c.A / 255f;
		}

		public static implicit operator Vector4((float X, float Y, float Z, float W) v)
		{
			return new Vector4(v.X, v.Y, v.Z, v.W);
		}

		public override bool Equals(object obj)
		{
            return obj is Vector4 && Equals((Vector4)obj);
        }

        public override int GetHashCode()
		{
			return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
		}

		public override string ToString()
		{
			return $"{X},{Y},{Z},{W}";
		}

		public bool Equals(Vector4 other)
		{
            return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        }

        public static float Distance(Vector4 v1, Vector4 v2)
		{
			return MathF.Sqrt(DistanceSquared(v1, v2));
		}

		public static float DistanceSquared(Vector4 v1, Vector4 v2)
		{
			return MathUtils.Sqr(v1.X - v2.X) + MathUtils.Sqr(v1.Y - v2.Y) + MathUtils.Sqr(v1.Z - v2.Z) + MathUtils.Sqr(v1.W - v2.W);
		}

		public static float Dot(Vector4 v1, Vector4 v2)
		{
			return (v1.X * v2.X) + (v1.Y * v2.Y) + (v1.Z * v2.Z) + (v1.W * v2.W);
		}

		public float Length()
		{
			return MathF.Sqrt(LengthSquared());
		}

		public float LengthSquared()
		{
			return (X * X) + (Y * Y) + (Z * Z);
		}

		public static Vector4 Floor(Vector4 v)
		{
			return new Vector4(MathF.Floor(v.X), MathF.Floor(v.Y), MathF.Floor(v.Z), MathF.Floor(v.W));
		}

		public static Vector4 Ceiling(Vector4 v)
		{
			return new Vector4(MathF.Ceiling(v.X), MathF.Ceiling(v.Y), MathF.Ceiling(v.Z), MathF.Ceiling(v.W));
		}

		public static Vector4 Round(Vector4 v)
		{
			return new Vector4(MathF.Round(v.X), MathF.Round(v.Y), MathF.Round(v.Z), MathF.Round(v.W));
		}

		public static Vector4 Min(Vector4 v, float f)
		{
			return new Vector4(MathF.Min(v.X, f), MathF.Min(v.Y, f), MathF.Min(v.Z, f), MathF.Min(v.W, f));
		}

		public static Vector4 Min(Vector4 v1, Vector4 v2)
		{
			return new Vector4(MathF.Min(v1.X, v2.X), MathF.Min(v1.Y, v2.Y), MathF.Min(v1.Z, v2.Z), MathF.Min(v1.W, v2.W));
		}

		public static Vector4 Max(Vector4 v, float f)
		{
			return new Vector4(MathF.Max(v.X, f), MathF.Max(v.Y, f), MathF.Max(v.Z, f), MathF.Max(v.W, f));
		}

		public static Vector4 Max(Vector4 v1, Vector4 v2)
		{
			return new Vector4(MathF.Max(v1.X, v2.X), MathF.Max(v1.Y, v2.Y), MathF.Max(v1.Z, v2.Z), MathF.Max(v1.W, v2.W));
		}

		public static Vector4 Clamp(Vector4 v, float min, float max)
		{
			return new Vector4(Math.Clamp(v.X, min, max), Math.Clamp(v.Y, min, max), Math.Clamp(v.Z, min, max), Math.Clamp(v.W, min, max));
		}

		public static Vector4 Saturate(Vector4 v)
		{
			return new Vector4(MathUtils.Saturate(v.X), MathUtils.Saturate(v.Y), MathUtils.Saturate(v.Z), MathUtils.Saturate(v.W));
		}

		public static Vector4 Lerp(Vector4 v1, Vector4 v2, float f)
		{
			return new Vector4(MathUtils.Lerp(v1.X, v2.X, f), MathUtils.Lerp(v1.Y, v2.Y, f), MathUtils.Lerp(v1.Z, v2.Z, f), MathUtils.Lerp(v1.W, v2.W, f));
		}

		public static Vector4 CatmullRom(Vector4 v1, Vector4 v2, Vector4 v3, Vector4 v4, float f)
		{
			return new Vector4(MathUtils.CatmullRom(v1.X, v2.X, v3.X, v4.X, f), MathUtils.CatmullRom(v1.Y, v2.Y, v3.Y, v4.Y, f), MathUtils.CatmullRom(v1.Z, v2.Z, v3.Z, v4.Z, f), MathUtils.CatmullRom(v1.W, v2.W, v3.W, v4.W, f));
		}

		public static Vector4 Normalize(Vector4 v)
		{
			float num = v.Length();
            return !(num > 0f) ? UnitX : v / num;
        }

        public static Vector4 LimitLength(Vector4 v, float maxLength)
		{
			float num = v.LengthSquared();
            return num > maxLength * maxLength ? v * (maxLength / MathF.Sqrt(num)) : v;
        }

        public static Vector4 Transform(Vector4 v, Matrix m)
		{
			return new Vector4((v.X * m.M11) + (v.Y * m.M21) + (v.Z * m.M31) + m.M41, (v.X * m.M12) + (v.Y * m.M22) + (v.Z * m.M32) + m.M42, (v.X * m.M13) + (v.Y * m.M23) + (v.Z * m.M33) + m.M43, (v.X * m.M14) + (v.Y * m.M24) + (v.Z * m.M34) + m.M44);
		}

		public static void Transform(ref Vector4 v, ref Matrix m, out Vector4 result)
		{
			result = new Vector4((v.X * m.M11) + (v.Y * m.M21) + (v.Z * m.M31) + m.M41, (v.X * m.M12) + (v.Y * m.M22) + (v.Z * m.M32) + m.M42, (v.X * m.M13) + (v.Y * m.M23) + (v.Z * m.M33) + m.M43, (v.X * m.M14) + (v.Y * m.M24) + (v.Z * m.M34) + m.M44);
		}

		public static void Transform(Vector4[] sourceArray, int sourceIndex, ref Matrix m, Vector4[] destinationArray, int destinationIndex, int count)
		{
			for (int i = 0; i < count; i++)
			{
				Vector4 vector = sourceArray[sourceIndex + i];
				destinationArray[destinationIndex + i] = new Vector4((vector.X * m.M11) + (vector.Y * m.M21) + (vector.Z * m.M31) + m.M41, (vector.X * m.M12) + (vector.Y * m.M22) + (vector.Z * m.M32) + m.M42, (vector.X * m.M13) + (vector.Y * m.M23) + (vector.Z * m.M33) + m.M43, (vector.X * m.M14) + (vector.Y * m.M24) + (vector.Z * m.M34) + m.M44);
			}
		}

		public static bool operator ==(Vector4 v1, Vector4 v2)
		{
			return v1.Equals(v2);
		}

		public static bool operator !=(Vector4 v1, Vector4 v2)
		{
			return !v1.Equals(v2);
		}

		public static Vector4 operator +(Vector4 v)
		{
			return v;
		}

		public static Vector4 operator -(Vector4 v)
		{
			return new Vector4(0f - v.X, 0f - v.Y, 0f - v.Z, 0f - v.W);
		}

		public static Vector4 operator +(Vector4 v1, Vector4 v2)
		{
			return new Vector4(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z, v1.W + v2.W);
		}

		public static Vector4 operator -(Vector4 v1, Vector4 v2)
		{
			return new Vector4(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z, v1.W - v2.W);
		}

		public static Vector4 operator *(Vector4 v1, Vector4 v2)
		{
			return new Vector4(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z, v1.W * v2.W);
		}

		public static Vector4 operator *(Vector4 v, float s)
		{
			return new Vector4(v.X * s, v.Y * s, v.Z * s, v.W * s);
		}

		public static Vector4 operator *(float s, Vector4 v)
		{
			return new Vector4(v.X * s, v.Y * s, v.Z * s, v.W * s);
		}

		public static Vector4 operator /(Vector4 v1, Vector4 v2)
		{
			return new Vector4(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z, v1.W / v2.W);
		}

		public static Vector4 operator /(Vector4 v, float d)
		{
			float num = 1f / d;
			return new Vector4(v.X * num, v.Y * num, v.Z * num, v.W * num);
		}
	}
}
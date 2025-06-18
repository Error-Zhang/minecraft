using System;

namespace Engine
{
	public struct Quaternion : IEquatable<Quaternion>
	{
		public float X;

		public float Y;

		public float Z;

		public float W;

		public static readonly Quaternion Identity = new(0f, 0f, 0f, 1f);

		public Quaternion(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public Quaternion(Vector3 v, float s)
		{
			X = v.X;
			Y = v.Y;
			Z = v.Z;
			W = s;
		}

		public static implicit operator Quaternion((float X, float Y, float Z, float W) v)
		{
			return new Quaternion(v.X, v.Y, v.Z, v.W);
		}

		public override bool Equals(object obj)
		{
            return obj is Quaternion && Equals((Quaternion)obj);
        }

        public override int GetHashCode()
		{
			return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
		}

		public override string ToString()
		{
			return $"{X.ToString()},{Y.ToString()},{Z.ToString()},{W.ToString()}";
		}

		public bool Equals(Quaternion other)
		{
            return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        }

        public static Quaternion Conjugate(Quaternion q)
		{
			return new Quaternion(0f - q.X, 0f - q.Y, 0f - q.Z, q.W);
		}

		public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
		{
			float x = angle * 0.5f;
			float num = MathF.Sin(x);
			float w = MathF.Cos(x);
			Quaternion result = default(Quaternion);
			result.X = axis.X * num;
			result.Y = axis.Y * num;
			result.Z = axis.Z * num;
			result.W = w;
			return result;
		}

		public static Quaternion CreateFromRotationMatrix(Matrix m)
		{
			float num = m.M11 + m.M22 + m.M33;
			Quaternion result = default(Quaternion);
			if (num > 0f)
			{
				float num2 = MathF.Sqrt(num + 1f);
				result.W = num2 * 0.5f;
				num2 = 0.5f / num2;
				result.X = (m.M23 - m.M32) * num2;
				result.Y = (m.M31 - m.M13) * num2;
				result.Z = (m.M12 - m.M21) * num2;
				return result;
			}
			if (m.M11 >= m.M22 && m.M11 >= m.M33)
			{
				float num3 = MathF.Sqrt(1f + m.M11 - m.M22 - m.M33);
				float num4 = 0.5f / num3;
				result.X = 0.5f * num3;
				result.Y = (m.M12 + m.M21) * num4;
				result.Z = (m.M13 + m.M31) * num4;
				result.W = (m.M23 - m.M32) * num4;
				return result;
			}
			if (m.M22 > m.M33)
			{
				float num5 = MathF.Sqrt(1f + m.M22 - m.M11 - m.M33);
				float num6 = 0.5f / num5;
				result.X = (m.M21 + m.M12) * num6;
				result.Y = 0.5f * num5;
				result.Z = (m.M32 + m.M23) * num6;
				result.W = (m.M31 - m.M13) * num6;
				return result;
			}
			float num7 = MathF.Sqrt(1f + m.M33 - m.M11 - m.M22);
			float num8 = 0.5f / num7;
			result.X = (m.M31 + m.M13) * num8;
			result.Y = (m.M32 + m.M23) * num8;
			result.Z = 0.5f * num7;
			result.W = (m.M12 - m.M21) * num8;
			return result;
		}

		public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
		{
			float x = roll * 0.5f;
			float x2 = pitch * 0.5f;
			float x3 = yaw * 0.5f;
			float num = MathF.Sin(x);
			float num2 = MathF.Cos(x);
			float num3 = MathF.Sin(x2);
			float num4 = MathF.Cos(x2);
			float num5 = MathF.Sin(x3);
			float num6 = MathF.Cos(x3);
			return new Quaternion((num6 * num3 * num2) + (num5 * num4 * num), (num5 * num4 * num2) - (num6 * num3 * num), (num6 * num4 * num) - (num5 * num3 * num2), (num6 * num4 * num2) + (num5 * num3 * num));
		}

		public static float Dot(Quaternion q1, Quaternion q2)
		{
			return (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W);
		}

		public static Quaternion Inverse(Quaternion q)
		{
			float num = (q.X * q.X) + (q.Y * q.Y) + (q.Z * q.Z) + (q.W * q.W);
			float num2 = 1f / num;
			Quaternion result = default(Quaternion);
			result.X = (0f - q.X) * num2;
			result.Y = (0f - q.Y) * num2;
			result.Z = (0f - q.Z) * num2;
			result.W = q.W * num2;
			return result;
		}

		public float Length()
		{
			return MathF.Sqrt(LengthSquared());
		}

		public float LengthSquared()
		{
			return (X * X) + (Y * Y) + (Z * Z) + (W * W);
		}

		public static Quaternion Lerp(Quaternion q1, Quaternion q2, float f)
		{
			float num = 1f - f;
			Quaternion result = default(Quaternion);
			if ((q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W) >= 0f)
			{
				result.X = (num * q1.X) + (f * q2.X);
				result.Y = (num * q1.Y) + (f * q2.Y);
				result.Z = (num * q1.Z) + (f * q2.Z);
				result.W = (num * q1.W) + (f * q2.W);
			}
			else
			{
				result.X = (num * q1.X) - (f * q2.X);
				result.Y = (num * q1.Y) - (f * q2.Y);
				result.Z = (num * q1.Z) - (f * q2.Z);
				result.W = (num * q1.W) - (f * q2.W);
			}
			float num2 = 1f / result.Length();
			result.X *= num2;
			result.Y *= num2;
			result.Z *= num2;
			result.W *= num2;
			return result;
		}

		public static Quaternion Slerp(Quaternion q1, Quaternion q2, float f)
		{
			float num = (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W);
			bool flag = false;
			if (num < 0f)
			{
				flag = true;
				num = 0f - num;
			}
			float num2;
			float num3;
			if (num > 0.999999f)
			{
				num2 = 1f - f;
				num3 = flag ? (0f - f) : f;
			}
			else
			{
				float num4 = MathF.Acos(num);
				float num5 = 1f / MathF.Sin(num4);
				num2 = MathF.Sin((1f - f) * num4) * num5;
				num3 = flag ? ((0f - MathF.Sin(f * num4)) * num5) : (MathF.Sin(f * num4) * num5);
			}
			Quaternion result = default(Quaternion);
			result.X = (num2 * q1.X) + (num3 * q2.X);
			result.Y = (num2 * q1.Y) + (num3 * q2.Y);
			result.Z = (num2 * q1.Z) + (num3 * q2.Z);
			result.W = (num2 * q1.W) + (num3 * q2.W);
			return result;
		}

		public static Quaternion Normalize(Quaternion q)
		{
			float num = q.Length();
            return num == 0f ? Identity : q / num;
        }

        public Matrix ToMatrix()
		{
			float num = X * X;
			float num2 = Y * Y;
			float num3 = Z * Z;
			float num4 = X * Y;
			float num5 = Z * W;
			float num6 = X * Z;
			float num7 = Y * W;
			float num8 = Y * Z;
			float num9 = X * W;
			Matrix result = default(Matrix);
			result.M11 = 1f - (2f * (num2 + num3));
			result.M12 = 2f * (num4 + num5);
			result.M13 = 2f * (num6 - num7);
			result.M14 = 0f;
			result.M21 = 2f * (num4 - num5);
			result.M22 = 1f - (2f * (num3 + num));
			result.M23 = 2f * (num8 + num9);
			result.M24 = 0f;
			result.M31 = 2f * (num6 + num7);
			result.M32 = 2f * (num8 - num9);
			result.M33 = 1f - (2f * (num2 + num));
			result.M34 = 0f;
			result.M41 = 0f;
			result.M42 = 0f;
			result.M43 = 0f;
			result.M44 = 1f;
			return result;
		}

		public Vector3 GetRightVector()
		{
			return new Vector3(1f - (2f * ((Y * Y) + (Z * Z))), 2f * ((X * Y) + (Z * W)), 2f * ((X * Z) - (W * Y)));
		}

		public Vector3 GetUpVector()
		{
			return new Vector3(2f * ((X * Y) - (Z * W)), 1f - (2f * ((X * X) + (Z * Z))), 2f * ((Y * Z) + (X * W)));
		}

		public Vector3 GetForwardVector()
		{
			return new Vector3(-2f * ((Y * W) + (X * Z)), 2f * ((X * W) - (Y * Z)), (2f * ((X * X) + (Y * Y))) - 1f);
		}

		public Vector3 ToYawPitchRoll()
		{
			float num = -2f * ((Y * W) + (X * Z));
			float x = 2f * ((X * W) - (Y * Z));
			float num2 = (2f * ((X * X) + (Y * Y))) - 1f;
			float y = 2f * ((X * Y) + (Z * W));
			float x2 = 1f - (2f * ((X * X) + (Z * Z)));
			float x3 = MathF.Atan2(0f - num, 0f - num2);
			float y2 = MathF.Asin(x);
			float z = MathF.Atan2(y, x2);
			return new Vector3(x3, y2, z);
		}

		public static bool operator ==(Quaternion q1, Quaternion q2)
		{
			return q1.Equals(q2);
		}

		public static bool operator !=(Quaternion q1, Quaternion q2)
		{
			return !q1.Equals(q2);
		}

		public static Quaternion operator +(Quaternion q)
		{
			return q;
		}

		public static Quaternion operator -(Quaternion q)
		{
			return new Quaternion(0f - q.X, 0f - q.Y, 0f - q.Z, 0f - q.W);
		}

		public static Quaternion operator +(Quaternion q1, Quaternion q2)
		{
			return new Quaternion(q1.X + q2.X, q1.Y + q2.Y, q1.Z + q2.Z, q1.W + q2.W);
		}

		public static Quaternion operator -(Quaternion q1, Quaternion q2)
		{
			return new Quaternion(q1.X - q2.X, q1.Y - q2.Y, q1.Z - q2.Z, q1.W - q2.W);
		}

		public static Quaternion operator *(Quaternion q1, Quaternion q2)
		{
			float x = q1.X;
			float y = q1.Y;
			float z = q1.Z;
			float w = q1.W;
			float x2 = q2.X;
			float y2 = q2.Y;
			float z2 = q2.Z;
			float w2 = q2.W;
			float num = (y * z2) - (z * y2);
			float num2 = (z * x2) - (x * z2);
			float num3 = (x * y2) - (y * x2);
			float num4 = (x * x2) + (y * y2) + (z * z2);
			Quaternion result = default(Quaternion);
			result.X = (x * w2) + (x2 * w) + num;
			result.Y = (y * w2) + (y2 * w) + num2;
			result.Z = (z * w2) + (z2 * w) + num3;
			result.W = (w * w2) - num4;
			return result;
		}

		public static Quaternion operator *(Quaternion q, float s)
		{
			return new Quaternion(q.X * s, q.Y * s, q.Z * s, q.W * s);
		}

		public static Quaternion operator /(Quaternion q1, Quaternion q2)
		{
			float x = q1.X;
			float y = q1.Y;
			float z = q1.Z;
			float w = q1.W;
			float num = (q2.X * q2.X) + (q2.Y * q2.Y) + (q2.Z * q2.Z) + (q2.W * q2.W);
			float num2 = 1f / num;
			float num3 = (0f - q2.X) * num2;
			float num4 = (0f - q2.Y) * num2;
			float num5 = (0f - q2.Z) * num2;
			float num6 = q2.W * num2;
			float num7 = (y * num5) - (z * num4);
			float num8 = (z * num3) - (x * num5);
			float num9 = (x * num4) - (y * num3);
			float num10 = (x * num3) + (y * num4) + (z * num5);
			Quaternion result = default(Quaternion);
			result.X = (x * num6) + (num3 * w) + num7;
			result.Y = (y * num6) + (num4 * w) + num8;
			result.Z = (z * num6) + (num5 * w) + num9;
			result.W = (w * num6) - num10;
			return result;
		}

		public static Quaternion operator /(Quaternion q, float d)
		{
			float num = 1f / d;
			return new Quaternion(q.X * num, q.Y * num, q.Z * num, q.W * num);
		}
	}
}
using System;

namespace Engine
{
	public struct Matrix(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44) : IEquatable<Matrix>
	{
		public float M11 = m11;
		public float M21 = m21;
		public float M31 = m31;
		public float M41 = m41;
		public float M12 = m12;
		public float M22 = m22;
		public float M32 = m32;
		public float M42 = m42;
        public float M13 = m13;
        public float M23 = m23;
		public float M33 = m33;
		public float M43 = m43;
		public float M14 = m14;
		public float M24 = m24;
		public float M34 = m34;
		public float M44 = m44;

		public static readonly Matrix Zero = default;

		public static readonly Matrix Identity = new(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);

		public Vector3 Right
		{
			get
			{
				return new Vector3(M11, M12, M13);
			}
			set
			{
				M11 = value.X;
				M12 = value.Y;
				M13 = value.Z;
			}
		}

		public Vector3 Up
		{
			get
			{
				return new Vector3(M21, M22, M23);
			}
			set
			{
				M21 = value.X;
				M22 = value.Y;
				M23 = value.Z;
			}
		}

		public Vector3 Forward
		{
			get
			{
				return new Vector3(0f - M31, 0f - M32, 0f - M33);
			}
			set
			{
				M31 = 0f - value.X;
				M32 = 0f - value.Y;
				M33 = 0f - value.Z;
			}
		}

		public Vector3 Translation
		{
			get
			{
				return new Vector3(M41, M42, M43);
			}
			set
			{
				M41 = value.X;
				M42 = value.Y;
				M43 = value.Z;
			}
		}

		public Matrix TranslationMatrix
		{
			get
			{
				return new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, M41, M42, M43, 1f);
			}
			set
			{
				M41 = value.M41;
				M42 = value.M42;
				M43 = value.M43;
			}
		}

		public Matrix OrientationMatrix
		{
			get
			{
				return new Matrix(M11, M12, M13, 0f, M21, M22, M23, 0f, M31, M32, M33, 0f, 0f, 0f, 0f, 1f);
			}
			set
			{
				M11 = value.M11;
				M12 = value.M12;
				M13 = value.M13;
				M21 = value.M21;
				M22 = value.M22;
				M23 = value.M23;
				M31 = value.M31;
				M32 = value.M32;
				M33 = value.M33;
			}
		}

        public override bool Equals(object obj)
		{
            return obj is Matrix && Equals((Matrix)obj);
        }

        public override int GetHashCode()
		{
			return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() + M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() + M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode() + M34.GetHashCode() + M41.GetHashCode() + M42.GetHashCode() + M43.GetHashCode() + M44.GetHashCode();
		}

		public override string ToString()
		{
			return $"{M11},{M12},{M13},{M14}, {M21},{M22},{M23},{M24}, {M31},{M32},{M33},{M34}, {M41},{M42},{M43},{M44}";
		}

		public bool Equals(Matrix other)
		{
            return M11 == other.M11 && M22 == other.M22 && M33 == other.M33 && M44 == other.M44 && M12 == other.M12 && M13 == other.M13 && M14 == other.M14 && M21 == other.M21 && M23 == other.M23 && M24 == other.M24 && M31 == other.M31 && M32 == other.M32 && M34 == other.M34 && M41 == other.M41 && M42 == other.M42
&& M43 == other.M43;
        }

        public float Determinant()
		{
			float num = (M33 * M44) - (M34 * M43);
			float num2 = (M32 * M44) - (M34 * M42);
			float num3 = (M32 * M43) - (M33 * M42);
			float num4 = (M31 * M44) - (M34 * M41);
			float num5 = (M31 * M43) - (M33 * M41);
			float num6 = (M31 * M42) - (M32 * M41);
			return (M11 * ((M22 * num) - (M23 * num2) + (M24 * num3))) - (M12 * ((M21 * num) - (M23 * num4) + (M24 * num5))) + (M13 * ((M21 * num2) - (M22 * num4) + (M24 * num6))) - (M14 * ((M21 * num3) - (M22 * num5) + (M23 * num6)));
		}

		public Vector3 ToYawPitchRoll()
		{
			Decompose(out Vector3 _, out Quaternion rotation, out Vector3 _);
			return rotation.ToYawPitchRoll();
		}

		public bool Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
		{
			translation.X = M41;
			translation.Y = M42;
			translation.Z = M43;
			float num = (!(M11 * M12 * M13 * M14 < 0f)) ? 1 : (-1);
			float num2 = (!(M21 * M22 * M23 * M24 < 0f)) ? 1 : (-1);
			float num3 = (!(M31 * M32 * M33 * M34 < 0f)) ? 1 : (-1);
			scale.X = num * MathF.Sqrt((M11 * M11) + (M12 * M12) + (M13 * M13));
			scale.Y = num2 * MathF.Sqrt((M21 * M21) + (M22 * M22) + (M23 * M23));
			scale.Z = num3 * MathF.Sqrt((M31 * M31) + (M32 * M32) + (M33 * M33));
			if (scale.X == 0f || scale.Y == 0f || scale.Z == 0f)
			{
				rotation = Quaternion.Identity;
				return false;
			}
			rotation = Quaternion.CreateFromRotationMatrix(new Matrix(M11 / scale.X, M12 / scale.X, M13 / scale.X, 0f, M21 / scale.Y, M22 / scale.Y, M23 / scale.Y, 0f, M31 / scale.Z, M32 / scale.Z, M33 / scale.Z, 0f, 0f, 0f, 0f, 1f));
			return true;
		}

		public static Matrix CreateFromAxisAngle(Vector3 axis, float angle)
		{
			float x = axis.X;
			float y = axis.Y;
			float z = axis.Z;
			float num = MathF.Sin(angle);
			float num2 = MathF.Cos(angle);
			float num3 = x * x;
			float num4 = y * y;
			float num5 = z * z;
			float num6 = x * y;
			float num7 = x * z;
			float num8 = y * z;
			Matrix result = default(Matrix);
			result.M11 = num3 + (num2 * (1f - num3));
			result.M12 = num6 - (num2 * num6) + (num * z);
			result.M13 = num7 - (num2 * num7) - (num * y);
			result.M14 = 0f;
			result.M21 = num6 - (num2 * num6) - (num * z);
			result.M22 = num4 + (num2 * (1f - num4));
			result.M23 = num8 - (num2 * num8) + (num * x);
			result.M24 = 0f;
			result.M31 = num7 - (num2 * num7) + (num * y);
			result.M32 = num8 - (num2 * num8) - (num * x);
			result.M33 = num5 + (num2 * (1f - num5));
			result.M34 = 0f;
			result.M41 = 0f;
			result.M42 = 0f;
			result.M43 = 0f;
			result.M44 = 1f;
			return result;
		}

		public static Matrix CreateFromQuaternion(Quaternion quaternion)
		{
			return quaternion.ToMatrix();
		}

		public static Matrix CreateFromYawPitchRoll(float yaw, float pitch, float roll)
		{
			return Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll).ToMatrix();
		}

		public static Matrix CreateLookAt(Vector3 position, Vector3 target, Vector3 up)
		{
			Vector3 vector = Vector3.Normalize(position - target);
			Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector));
			up = Vector3.Normalize(Vector3.Cross(vector, vector2));
			Matrix result = default(Matrix);
			result.M11 = vector2.X;
			result.M12 = up.X;
			result.M13 = vector.X;
			result.M14 = 0f;
			result.M21 = vector2.Y;
			result.M22 = up.Y;
			result.M23 = vector.Y;
			result.M24 = 0f;
			result.M31 = vector2.Z;
			result.M32 = up.Z;
			result.M33 = vector.Z;
			result.M34 = 0f;
			result.M41 = 0f - Vector3.Dot(vector2, position);
			result.M42 = 0f - Vector3.Dot(up, position);
			result.M43 = 0f - Vector3.Dot(vector, position);
			result.M44 = 1f;
			return result;
		}

		public static Matrix CreateOrthographic(float width, float height, float nearPlane, float farPlane)
		{
			Matrix result = default(Matrix);
			result.M11 = 2f / width;
			result.M12 = result.M13 = result.M14 = 0f;
			result.M22 = 2f / height;
			result.M21 = result.M23 = result.M24 = 0f;
			result.M33 = 1f / (nearPlane - farPlane);
			result.M31 = result.M32 = result.M34 = 0f;
			result.M41 = result.M42 = 0f;
			result.M43 = nearPlane / (nearPlane - farPlane);
			result.M44 = 1f;
			return result;
		}

		public static Matrix CreateOrthographicOffCenter(float left, float right, float bottom, float top, float nearPlane, float farPlane)
		{
			Matrix result = default(Matrix);
			result.M11 = 2f / (right - left);
			result.M12 = 0f;
			result.M13 = 0f;
			result.M14 = 0f;
			result.M21 = 0f;
			result.M22 = 2f / (top - bottom);
			result.M23 = 0f;
			result.M24 = 0f;
			result.M31 = 0f;
			result.M32 = 0f;
			result.M33 = 1f / (nearPlane - farPlane);
			result.M34 = 0f;
			result.M41 = (left + right) / (left - right);
			result.M42 = (top + bottom) / (bottom - top);
			result.M43 = nearPlane / (nearPlane - farPlane);
			result.M44 = 1f;
			return result;
		}

		public static Matrix CreatePerspective(float width, float height, float nearPlane, float farPlane)
		{
			Matrix result = default(Matrix);
			result.M11 = 2f * nearPlane / width;
			result.M12 = result.M13 = result.M14 = 0f;
			result.M22 = 2f * nearPlane / height;
			result.M21 = result.M23 = result.M24 = 0f;
			result.M33 = farPlane / (nearPlane - farPlane);
			result.M31 = result.M32 = 0f;
			result.M34 = -1f;
			result.M41 = result.M42 = result.M44 = 0f;
			result.M43 = nearPlane * farPlane / (nearPlane - farPlane);
			return result;
		}

		public static Matrix CreatePerspectiveFieldOfView(float fieldOfViewY, float aspectRatio, float nearPlane, float farPlane)
		{
			float num = 1f / MathF.Tan(fieldOfViewY * 0.5f);
			Matrix result = default;
			result.M11 = num / aspectRatio;
			result.M12 = result.M13 = result.M14 = 0f;
			result.M22 = num;
			result.M21 = result.M23 = result.M24 = 0f;
			result.M31 = result.M32 = 0f;
			result.M33 = farPlane / (nearPlane - farPlane);
			result.M34 = -1f;
			result.M41 = result.M42 = result.M44 = 0f;
			result.M43 = nearPlane * farPlane / (nearPlane - farPlane);
			return result;
		}

		public static Matrix CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlane, float farPlane)
		{
			Matrix result = default(Matrix);
			result.M11 = 2f * nearPlane / (right - left);
			result.M12 = result.M13 = result.M14 = 0f;
			result.M22 = 2f * nearPlane / (top - bottom);
			result.M21 = result.M23 = result.M24 = 0f;
			result.M31 = (left + right) / (right - left);
			result.M32 = (top + bottom) / (top - bottom);
			result.M33 = farPlane / (nearPlane - farPlane);
			result.M34 = -1f;
			result.M43 = nearPlane * farPlane / (nearPlane - farPlane);
			result.M41 = result.M42 = result.M44 = 0f;
			return result;
		}

        public static Matrix CreateRotationX(float radians)
        {
            float num = MathF.Cos(radians);
            float num2 = MathF.Sin(radians);
            return new Matrix(1f, 0f, 0f, 0f, 0f, num, num2, 0f, 0f, 0f - num2, num, 0f, 0f, 0f, 0f, 1f);
        }

        public static Matrix CreateRotationX(float radians, Vector3 center)
        {
            float num = MathF.Cos(radians);
            float num2 = MathF.Sin(radians);
            return new Matrix(1f, 0f, 0f, 0f, 0f, num, num2, 0f, 0f, 0f - num2, num, 0f, 0f, num2 * center.Z - num * center.Y + center.Y, (0f - num) * center.Z - num2 * center.Y + center.Z, 1f);
        }

        public static Matrix CreateRotationY(float radians)
        {
            float num = MathF.Cos(radians);
            float num2 = MathF.Sin(radians);
            return new Matrix(num, 0f, 0f - num2, 0f, 0f, 1f, 0f, 0f, num2, 0f, num, 0f, 0f, 0f, 0f, 1f);
        }

        public static Matrix CreateRotationY(float radians, Vector3 center)
        {
            float num = MathF.Cos(radians);
            float num2 = MathF.Sin(radians);
            return new Matrix(num, 0f, 0f - num2, 0f, 0f, 1f, 0f, 0f, num2, 0f, num, 0f, (0f - num2) * center.Z - num * center.X + center.X, 0f, (0f - num) * center.Z + num2 * center.X + center.Z, 1f);
        }

        public static Matrix CreateRotationZ(float radians)
        {
            float num = MathF.Cos(radians);
            float num2 = MathF.Sin(radians);
            return new Matrix(num, num2, 0f, 0f, 0f - num2, num, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
        }

        public static Matrix CreateRotationZ(float radians, Vector3 center)
        {
            float num = MathF.Cos(radians);
            float num2 = MathF.Sin(radians);
            return new Matrix(num, num2, 0f, 0f, 0f - num2, num, 0f, 0f, 0f, 0f, 1f, 0f, num2 * center.Y - num * center.X + center.X, (0f - num) * center.Y - num2 * center.X + center.Y, 0f, 1f);
        }

		public static Matrix CreateScale(float scale)
		{
			return new Matrix(scale, 0f, 0f, 0f, 0f, scale, 0f, 0f, 0f, 0f, scale, 0f, 0f, 0f, 0f, 1f);
		}

		public static Matrix CreateScale(float x, float y, float z)
		{
			return new Matrix(x, 0f, 0f, 0f, 0f, y, 0f, 0f, 0f, 0f, z, 0f, 0f, 0f, 0f, 1f);
		}

		public static Matrix CreateScale(Vector3 scale)
		{
			return new Matrix(scale.X, 0f, 0f, 0f, 0f, scale.Y, 0f, 0f, 0f, 0f, scale.Z, 0f, 0f, 0f, 0f, 1f);
		}

		public static Matrix CreateTranslation(float x, float y, float z)
		{
			return new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, x, y, z, 1f);
		}

		public static Matrix CreateTranslation(Vector3 position)
		{
			return new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, position.X, position.Y, position.Z, 1f);
		}

		public static Matrix CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
		{
			forward = Vector3.Normalize(forward);
			Vector3 vector = Vector3.Normalize(Vector3.Cross(forward, Vector3.Normalize(up)));
			up = Vector3.Normalize(Vector3.Cross(vector, forward));
			Matrix result = default(Matrix);
			result.Right = vector;
			result.Up = up;
			result.Forward = forward;
			result.Translation = position;
			result.M44 = 1f;
			return result;
		}

		public static Matrix CreateShadow(Vector4 lightDirection, Plane plane)
		{
			float num = (plane.Normal.X * lightDirection.X) + (plane.Normal.Y * lightDirection.Y) + (plane.Normal.Z * lightDirection.Z);
			float num2 = 0f - plane.Normal.X;
			float num3 = 0f - plane.Normal.Y;
			float num4 = 0f - plane.Normal.Z;
			float num5 = 0f - plane.D;
			Matrix result = default(Matrix);
			result.M11 = (num2 * lightDirection.X) + num;
			result.M21 = num3 * lightDirection.X;
			result.M31 = num4 * lightDirection.X;
			result.M41 = num5 * lightDirection.X;
			result.M12 = num2 * lightDirection.Y;
			result.M22 = (num3 * lightDirection.Y) + num;
			result.M32 = num4 * lightDirection.Y;
			result.M42 = num5 * lightDirection.Y;
			result.M13 = num2 * lightDirection.Z;
			result.M23 = num3 * lightDirection.Z;
			result.M33 = (num4 * lightDirection.Z) + num;
			result.M43 = num5 * lightDirection.Z;
			result.M14 = num2 * lightDirection.W;
			result.M24 = num3 * lightDirection.W;
			result.M34 = num4 * lightDirection.W;
			result.M44 = (num5 * lightDirection.W) + num;
			return result;
		}

		public static Matrix Transpose(Matrix m)
		{
			return new Matrix(m.M11, m.M21, m.M31, m.M41, m.M12, m.M22, m.M32, m.M42, m.M13, m.M23, m.M33, m.M43, m.M14, m.M24, m.M34, m.M44);
		}

		public static Matrix Invert(Matrix m)
		{
			float m2 = m.M11;
			float m3 = m.M12;
			float m4 = m.M13;
			float m5 = m.M14;
			float m6 = m.M21;
			float m7 = m.M22;
			float m8 = m.M23;
			float m9 = m.M24;
			float m10 = m.M31;
			float m11 = m.M32;
			float m12 = m.M33;
			float m13 = m.M34;
			float m14 = m.M41;
			float m15 = m.M42;
			float m16 = m.M43;
			float m17 = m.M44;
			float num = (m12 * m17) - (m13 * m16);
			float num2 = (m11 * m17) - (m13 * m15);
			float num3 = (m11 * m16) - (m12 * m15);
			float num4 = (m10 * m17) - (m13 * m14);
			float num5 = (m10 * m16) - (m12 * m14);
			float num6 = (m10 * m15) - (m11 * m14);
			float num7 = (m7 * num) - (m8 * num2) + (m9 * num3);
			float num8 = 0f - ((m6 * num) - (m8 * num4) + (m9 * num5));
			float num9 = (m6 * num2) - (m7 * num4) + (m9 * num6);
			float num10 = 0f - ((m6 * num3) - (m7 * num5) + (m8 * num6));
			float num11 = 1f / ((m2 * num7) + (m3 * num8) + (m4 * num9) + (m5 * num10));
			Matrix result = default(Matrix);
			result.M11 = num7 * num11;
			result.M21 = num8 * num11;
			result.M31 = num9 * num11;
			result.M41 = num10 * num11;
			result.M12 = (0f - ((m3 * num) - (m4 * num2) + (m5 * num3))) * num11;
			result.M22 = ((m2 * num) - (m4 * num4) + (m5 * num5)) * num11;
			result.M32 = (0f - ((m2 * num2) - (m3 * num4) + (m5 * num6))) * num11;
			result.M42 = ((m2 * num3) - (m3 * num5) + (m4 * num6)) * num11;
			float num12 = (m8 * m17) - (m9 * m16);
			float num13 = (m7 * m17) - (m9 * m15);
			float num14 = (m7 * m16) - (m8 * m15);
			float num15 = (m6 * m17) - (m9 * m14);
			float num16 = (m6 * m16) - (m8 * m14);
			float num17 = (m6 * m15) - (m7 * m14);
			result.M13 = ((m3 * num12) - (m4 * num13) + (m5 * num14)) * num11;
			result.M23 = (0f - ((m2 * num12) - (m4 * num15) + (m5 * num16))) * num11;
			result.M33 = ((m2 * num13) - (m3 * num15) + (m5 * num17)) * num11;
			result.M43 = (0f - ((m2 * num14) - (m3 * num16) + (m4 * num17))) * num11;
			float num18 = (m8 * m13) - (m9 * m12);
			float num19 = (m7 * m13) - (m9 * m11);
			float num20 = (m7 * m12) - (m8 * m11);
			float num21 = (m6 * m13) - (m9 * m10);
			float num22 = (m6 * m12) - (m8 * m10);
			float num23 = (m6 * m11) - (m7 * m10);
			result.M14 = (0f - ((m3 * num18) - (m4 * num19) + (m5 * num20))) * num11;
			result.M24 = ((m2 * num18) - (m4 * num21) + (m5 * num22)) * num11;
			result.M34 = (0f - ((m2 * num19) - (m3 * num21) + (m5 * num23))) * num11;
			result.M44 = ((m2 * num20) - (m3 * num22) + (m4 * num23)) * num11;
			return result;
		}

		public static Matrix Lerp(Matrix m1, Matrix m2, float f)
		{
			m1.M11 += (m2.M11 - m1.M11) * f;
			m1.M12 += (m2.M12 - m1.M12) * f;
			m1.M13 += (m2.M13 - m1.M13) * f;
			m1.M14 += (m2.M14 - m1.M14) * f;
			m1.M21 += (m2.M21 - m1.M21) * f;
			m1.M22 += (m2.M22 - m1.M22) * f;
			m1.M23 += (m2.M23 - m1.M23) * f;
			m1.M24 += (m2.M24 - m1.M24) * f;
			m1.M31 += (m2.M31 - m1.M31) * f;
			m1.M32 += (m2.M32 - m1.M32) * f;
			m1.M33 += (m2.M33 - m1.M33) * f;
			m1.M34 += (m2.M34 - m1.M34) * f;
			m1.M41 += (m2.M41 - m1.M41) * f;
			m1.M42 += (m2.M42 - m1.M42) * f;
			m1.M43 += (m2.M43 - m1.M43) * f;
			m1.M44 += (m2.M44 - m1.M44) * f;
			return m1;
		}

		public static void MultiplyRestricted(ref Matrix m1, ref Matrix m2, out Matrix result)
		{
			result.M11 = (m1.M11 * m2.M11) + (m1.M12 * m2.M21) + (m1.M13 * m2.M31) + (m1.M14 * m2.M41);
			result.M12 = (m1.M11 * m2.M12) + (m1.M12 * m2.M22) + (m1.M13 * m2.M32) + (m1.M14 * m2.M42);
			result.M13 = (m1.M11 * m2.M13) + (m1.M12 * m2.M23) + (m1.M13 * m2.M33) + (m1.M14 * m2.M43);
			result.M14 = (m1.M11 * m2.M14) + (m1.M12 * m2.M24) + (m1.M13 * m2.M34) + (m1.M14 * m2.M44);
			result.M21 = (m1.M21 * m2.M11) + (m1.M22 * m2.M21) + (m1.M23 * m2.M31) + (m1.M24 * m2.M41);
			result.M22 = (m1.M21 * m2.M12) + (m1.M22 * m2.M22) + (m1.M23 * m2.M32) + (m1.M24 * m2.M42);
			result.M23 = (m1.M21 * m2.M13) + (m1.M22 * m2.M23) + (m1.M23 * m2.M33) + (m1.M24 * m2.M43);
			result.M24 = (m1.M21 * m2.M14) + (m1.M22 * m2.M24) + (m1.M23 * m2.M34) + (m1.M24 * m2.M44);
			result.M31 = (m1.M31 * m2.M11) + (m1.M32 * m2.M21) + (m1.M33 * m2.M31) + (m1.M34 * m2.M41);
			result.M32 = (m1.M31 * m2.M12) + (m1.M32 * m2.M22) + (m1.M33 * m2.M32) + (m1.M34 * m2.M42);
			result.M33 = (m1.M31 * m2.M13) + (m1.M32 * m2.M23) + (m1.M33 * m2.M33) + (m1.M34 * m2.M43);
			result.M34 = (m1.M31 * m2.M14) + (m1.M32 * m2.M24) + (m1.M33 * m2.M34) + (m1.M34 * m2.M44);
			result.M41 = (m1.M41 * m2.M11) + (m1.M42 * m2.M21) + (m1.M43 * m2.M31) + (m1.M44 * m2.M41);
			result.M42 = (m1.M41 * m2.M12) + (m1.M42 * m2.M22) + (m1.M43 * m2.M32) + (m1.M44 * m2.M42);
			result.M43 = (m1.M41 * m2.M13) + (m1.M42 * m2.M23) + (m1.M43 * m2.M33) + (m1.M44 * m2.M43);
			result.M44 = (m1.M41 * m2.M14) + (m1.M42 * m2.M24) + (m1.M43 * m2.M34) + (m1.M44 * m2.M44);
		}

		public static bool operator ==(Matrix m1, Matrix m2)
		{
			return m1.Equals(m2);
		}

		public static bool operator !=(Matrix m1, Matrix m2)
		{
			return !m1.Equals(m2);
		}

		public static Matrix operator +(Matrix m)
		{
			return m;
		}

		public static Matrix operator -(Matrix m)
		{
			return new Matrix(0f - m.M11, 0f - m.M12, 0f - m.M13, 0f - m.M14, 0f - m.M21, 0f - m.M22, 0f - m.M23, 0f - m.M24, 0f - m.M31, 0f - m.M32, 0f - m.M33, 0f - m.M34, 0f - m.M41, 0f - m.M42, 0f - m.M43, 0f - m.M44);
		}

		public static Matrix operator +(Matrix m1, Matrix m2)
		{
			return new Matrix(m1.M11 + m2.M11, m1.M12 + m2.M12, m1.M13 + m2.M13, m1.M14 + m2.M14, m1.M21 + m2.M21, m1.M22 + m2.M22, m1.M23 + m2.M23, m1.M24 + m2.M24, m1.M31 + m2.M31, m1.M32 + m2.M32, m1.M33 + m2.M33, m1.M34 + m2.M34, m1.M41 + m2.M41, m1.M42 + m2.M42, m1.M43 + m2.M43, m1.M44 + m2.M44);
		}

		public static Matrix operator -(Matrix m1, Matrix m2)
		{
			return new Matrix(m1.M11 - m2.M11, m1.M12 - m2.M12, m1.M13 - m2.M13, m1.M14 - m2.M14, m1.M21 - m2.M21, m1.M22 - m2.M22, m1.M23 - m2.M23, m1.M24 - m2.M24, m1.M31 - m2.M31, m1.M32 - m2.M32, m1.M33 - m2.M33, m1.M34 - m2.M34, m1.M41 - m2.M41, m1.M42 - m2.M42, m1.M43 - m2.M43, m1.M44 - m2.M44);
		}

		public static Matrix operator *(Matrix m1, Matrix m2)
		{
			MultiplyRestricted(ref m1, ref m2, out Matrix result);
			return result;
		}

		public static Matrix operator *(Matrix m, float s)
		{
			return new Matrix(m.M11 * s, m.M12 * s, m.M13 * s, m.M14 * s, m.M21 * s, m.M22 * s, m.M23 * s, m.M24 * s, m.M31 * s, m.M32 * s, m.M33 * s, m.M34 * s, m.M41 * s, m.M42 * s, m.M43 * s, m.M44 * s);
		}

		public static Matrix operator *(float s, Matrix m)
		{
			return new Matrix(m.M11 * s, m.M12 * s, m.M13 * s, m.M14 * s, m.M21 * s, m.M22 * s, m.M23 * s, m.M24 * s, m.M31 * s, m.M32 * s, m.M33 * s, m.M34 * s, m.M41 * s, m.M42 * s, m.M43 * s, m.M44 * s);
		}

		public static Matrix operator /(Matrix m1, Matrix m2)
		{
			return new Matrix(m1.M11 / m2.M11, m1.M12 / m2.M12, m1.M13 / m2.M13, m1.M14 / m2.M14, m1.M21 / m2.M21, m1.M22 / m2.M22, m1.M23 / m2.M23, m1.M24 / m2.M24, m1.M31 / m2.M31, m1.M32 / m2.M32, m1.M33 / m2.M33, m1.M34 / m2.M34, m1.M41 / m2.M41, m1.M42 / m2.M42, m1.M43 / m2.M43, m1.M44 / m2.M44);
		}

		public static Matrix operator /(Matrix m, float d)
		{
			float num = 1f / d;
			return new Matrix(m.M11 * num, m.M12 * num, m.M13 * num, m.M14 * num, m.M21 * num, m.M22 * num, m.M23 * num, m.M24 * num, m.M31 * num, m.M32 * num, m.M33 * num, m.M34 * num, m.M41 * num, m.M42 * num, m.M43 * num, m.M44 * num);
		}
	}
}
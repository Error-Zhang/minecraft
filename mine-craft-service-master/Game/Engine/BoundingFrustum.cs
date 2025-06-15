using System;

namespace Engine
{
	public class BoundingFrustum : IEquatable<BoundingFrustum>
	{
		private Matrix m_viewProjection;

		private Plane[] m_planes = new Plane[6];

        private Vector3[] m_corners = new Vector3[8];

        private bool m_cornersValid;

		public Plane Near => m_planes[0];

		public Plane Far => m_planes[1];

		public Plane Left => m_planes[2];

		public Plane Right => m_planes[3];

		public Plane Top => m_planes[4];

		public Plane Bottom => m_planes[5];

		public Matrix Matrix
		{
			get
			{
				return m_viewProjection;
			}
			set
			{
				m_viewProjection = value;
				m_planes[0].Normal.X = 0f - value.M13;
				m_planes[0].Normal.Y = 0f - value.M23;
				m_planes[0].Normal.Z = 0f - value.M33;
				m_planes[0].D = 0f - value.M43;
				m_planes[1].Normal.X = 0f - value.M14 + value.M13;
				m_planes[1].Normal.Y = 0f - value.M24 + value.M23;
				m_planes[1].Normal.Z = 0f - value.M34 + value.M33;
				m_planes[1].D = 0f - value.M44 + value.M43;
				m_planes[2].Normal.X = 0f - value.M14 - value.M11;
				m_planes[2].Normal.Y = 0f - value.M24 - value.M21;
				m_planes[2].Normal.Z = 0f - value.M34 - value.M31;
				m_planes[2].D = 0f - value.M44 - value.M41;
				m_planes[3].Normal.X = 0f - value.M14 + value.M11;
				m_planes[3].Normal.Y = 0f - value.M24 + value.M21;
				m_planes[3].Normal.Z = 0f - value.M34 + value.M31;
				m_planes[3].D = 0f - value.M44 + value.M41;
				m_planes[4].Normal.X = 0f - value.M14 + value.M12;
				m_planes[4].Normal.Y = 0f - value.M24 + value.M22;
				m_planes[4].Normal.Z = 0f - value.M34 + value.M32;
				m_planes[4].D = 0f - value.M44 + value.M42;
				m_planes[5].Normal.X = 0f - value.M14 - value.M12;
				m_planes[5].Normal.Y = 0f - value.M24 - value.M22;
				m_planes[5].Normal.Z = 0f - value.M34 - value.M32;
				m_planes[5].D = 0f - value.M44 - value.M42;
				for (int i = 0; i < 6; i++)
				{
					float num = m_planes[i].Normal.Length();
					m_planes[i].Normal /= num;
					m_planes[i].D /= num;
				}
                m_cornersValid = false;
			}
		}

        public ReadOnlyList<Vector3> Corners
        {
            get
            {
                if (!m_cornersValid)
                {
                    m_cornersValid = true;
                    Ray3 ray = ComputeIntersectionLine(m_planes[0], m_planes[2]);
                    m_corners[0] = ComputeIntersection(m_planes[4], ray);
                    m_corners[3] = ComputeIntersection(m_planes[5], ray);
                    ray = ComputeIntersectionLine(m_planes[3], m_planes[0]);
                    m_corners[1] = ComputeIntersection(m_planes[4], ray);
                    m_corners[2] = ComputeIntersection(m_planes[5], ray);
                    ray = ComputeIntersectionLine(m_planes[2], m_planes[1]);
                    m_corners[4] = ComputeIntersection(m_planes[4], ray);
                    m_corners[7] = ComputeIntersection(m_planes[5], ray);
                    ray = ComputeIntersectionLine(m_planes[1], m_planes[3]);
                    m_corners[5] = ComputeIntersection(m_planes[4], ray);
                    m_corners[6] = ComputeIntersection(m_planes[5], ray);
                }
                return new ReadOnlyList<Vector3>(m_corners);
            }
        }

		public BoundingFrustum(Matrix viewProjection)
		{
			Matrix = viewProjection;
		}

		public override bool Equals(object obj)
		{
			BoundingFrustum boundingFrustum = obj as BoundingFrustum;
            return boundingFrustum != null && m_viewProjection == boundingFrustum.m_viewProjection;
        }

        public override int GetHashCode()
		{
			return m_viewProjection.GetHashCode();
		}

		public bool Equals(BoundingFrustum other)
		{
            return other != null && m_viewProjection == other.m_viewProjection;
        }

        public override string ToString()
		{
			return m_viewProjection.ToString();
		}

		public bool Intersection(Vector3 point)
		{
			for (int i = 0; i < m_planes.Length; i++)
			{
				float x = m_planes[i].Normal.X;
				float y = m_planes[i].Normal.Y;
				float z = m_planes[i].Normal.Z;
				float d = m_planes[i].D;
				if ((x * point.X) + (y * point.Y) + (z * point.Z) + d > 0f)
				{
					return false;
				}
			}
			return true;
		}

		public bool Intersection(BoundingSphere sphere)
		{
			for (int i = 0; i < m_planes.Length; i++)
			{
				float x = m_planes[i].Normal.X;
				float y = m_planes[i].Normal.Y;
				float z = m_planes[i].Normal.Z;
				float d = m_planes[i].D;
				if ((x * sphere.Center.X) + (y * sphere.Center.Y) + (z * sphere.Center.Z) + d > sphere.Radius)
				{
					return false;
				}
			}
			return true;
		}

		public bool Intersection(BoundingBox box)
		{
			for (int i = 0; i < m_planes.Length; i++)
			{
				float x = m_planes[i].Normal.X;
				float y = m_planes[i].Normal.Y;
				float z = m_planes[i].Normal.Z;
				float d = m_planes[i].D;
				float num = (x > 0f) ? box.Min.X : box.Max.X;
				float num2 = (y > 0f) ? box.Min.Y : box.Max.Y;
				float num3 = (z > 0f) ? box.Min.Z : box.Max.Z;
				if ((x * num) + (y * num2) + (z * num3) + d > 0f)
				{
					return false;
				}
			}
			return true;
		}

		public static bool operator ==(BoundingFrustum f1, BoundingFrustum f2)
		{
			return object.Equals(f1, f2);
		}

		public static bool operator !=(BoundingFrustum f1, BoundingFrustum f2)
		{
			return !object.Equals(f1, f2);
		}

		public static Vector3 ComputeIntersection(Plane plane, Ray3 ray)
		{
			float s = (0f - plane.D - Vector3.Dot(plane.Normal, ray.Position)) / Vector3.Dot(plane.Normal, ray.Direction);
			return ray.Position + (ray.Direction * s);
		}

        public static Ray3 ComputeIntersectionLine(Plane p1, Plane p2)
		{
			Ray3 result = default(Ray3);
			result.Direction = Vector3.Cross(p1.Normal, p2.Normal);
			float d = result.Direction.LengthSquared();
			result.Position = Vector3.Cross(((0f - p1.D) * p2.Normal) + (p2.D * p1.Normal), result.Direction) / d;
			return result;
		}
	}
}
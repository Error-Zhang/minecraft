using System;

namespace Engine
{
	public struct Rectangle : IEquatable<Rectangle>
	{
		public int Left;

		public int Top;

		public int Width;

		public int Height;

		public static Rectangle Empty;

		public Point2 Location
		{
			get
			{
				return new Point2(Left, Top);
			}
			set
			{
				Left = value.X;
				Top = value.Y;
			}
		}

		public Point2 Size
		{
			get
			{
				return new Point2(Width, Height);
			}
			set
			{
				Width = value.X;
				Height = value.Y;
			}
		}

		public int Right => Left + Width;

		public int Bottom => Top + Height;

		public Rectangle(int left, int top, int width, int height)
		{
			Left = left;
			Top = top;
			Width = width;
			Height = height;
		}

		public static implicit operator Rectangle((int Left, int Top, int Width, int Height) v)
		{
			return new Rectangle(v.Left, v.Top, v.Width, v.Height);
		}

		public bool Equals(Rectangle other)
		{
            return Left == other.Left && Top == other.Top && Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
		{
            return obj is Rectangle && Equals((Rectangle)obj);
        }

        public override int GetHashCode()
		{
			return Left + Top + Width + Height;
		}

		public override string ToString()
		{
			return $"{Left},{Top},{Width},{Height}";
		}

		public bool Contains(Point2 p)
		{
            return p.X >= Left && p.X < Left + Width && p.Y >= Top && p.Y < Top + Height;
        }

        public bool Intersection(Rectangle r)
		{
			int num = MathUtils.Max(Left, r.Left);
			int num2 = MathUtils.Max(Top, r.Top);
			int num3 = MathUtils.Min(Left + Width, r.Left + r.Width);
			int num4 = MathUtils.Min(Top + Height, r.Top + r.Height);
            return num3 > num && num4 > num2;
        }

        public static Rectangle Intersection(Rectangle r1, Rectangle r2)
		{
			int num = MathUtils.Max(r1.Left, r2.Left);
			int num2 = MathUtils.Max(r1.Top, r2.Top);
			int num3 = MathUtils.Min(r1.Left + r1.Width, r2.Left + r2.Width);
			int num4 = MathUtils.Min(r1.Top + r1.Height, r2.Top + r2.Height);
            return num3 <= num || num4 <= num2 ? Empty : new Rectangle(num, num2, num3 - num, num4 - num2);
        }

        public static Rectangle Union(Rectangle r1, Rectangle r2)
		{
			int num = MathUtils.Min(r1.Left, r2.Left);
			int num2 = MathUtils.Min(r1.Top, r2.Top);
			int num3 = MathUtils.Max(r1.Left + r1.Width, r2.Left + r2.Width);
			int num4 = MathUtils.Max(r1.Top + r1.Height, r2.Top + r2.Height);
			return new Rectangle(num, num2, num3 - num, num4 - num2);
		}

		public static bool operator ==(Rectangle r1, Rectangle r2)
		{
			return r1.Equals(r2);
		}

		public static bool operator !=(Rectangle r1, Rectangle r2)
		{
			return !r1.Equals(r2);
		}
	}
}
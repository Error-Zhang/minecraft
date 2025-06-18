using System;

namespace Engine
{
	public struct Box : IEquatable<Box>
	{
		public int Left;

		public int Top;

		public int Near;

		public int Width;

		public int Height;

		public int Depth;

		public static Box Empty;

		public Point3 Location
		{
			get
			{
				return new Point3(Left, Top, Near);
			}
			set
			{
				Left = value.X;
				Top = value.Y;
				Near = value.Z;
			}
		}

		public Point3 Size
		{
			get
			{
				return new Point3(Width, Height, Depth);
			}
			set
			{
				Width = value.X;
				Height = value.Y;
				Depth = value.Z;
			}
		}

		public int Right => Left + Width;

		public int Bottom => Top + Height;

		public int Far => Near + Depth;

		public Box(int left, int top, int near, int width, int height, int depth)
		{
			Left = left;
			Top = top;
			Near = near;
			Width = width;
			Height = height;
			Depth = depth;
		}

		public static implicit operator Box((int Left, int Top, int Near, int Width, int Height, int Depth) v)
		{
			return new Box(v.Left, v.Top, v.Near, v.Width, v.Height, v.Depth);
		}

		public bool Equals(Box other)
		{
            return Left == other.Left && Top == other.Top && Near == other.Near && Width == other.Width && Height == other.Height
&& Depth == other.Depth;
        }

        public override bool Equals(object obj)
		{
            return obj is Box box && Equals(box);
        }

        public override int GetHashCode()
		{
			return Left + Top + Near + Width + Height + Depth;
		}

		public override string ToString()
		{
			return $"{Left},{Top},{Near},{Width},{Height},{Depth}";
		}

		public bool Contains(Point3 p)
		{
            return p.X >= Left && p.X < Left + Width && p.Y >= Top && p.Y < Top + Height && p.Z >= Near && p.Z < Near + Depth;
        }

        public static Box Intersection(Box b1, Box b2)
		{
			int num = Math.Max(b1.Left, b2.Left);
			int num2 = Math.Max(b1.Top, b2.Top);
			int num3 = Math.Min(b1.Near, b2.Near);
			int num4 = Math.Min(b1.Left + b1.Width, b2.Left + b2.Width);
			int num5 = Math.Min(b1.Top + b1.Height, b2.Top + b2.Height);
			int num6 = Math.Min(b1.Near + b1.Depth, b2.Near + b2.Depth);
            return num4 <= num || num5 <= num2 || num6 <= num3 ? Empty : new Box(num, num2, num3, num4 - num, num5 - num2, num6 - num3);
        }

        public static Box Union(Box b1, Box b2)
		{
			int num = Math.Min(b1.Left, b2.Left);
			int num2 = Math.Min(b1.Top, b2.Top);
			int num3 = Math.Min(b1.Near, b2.Near);
			int num4 = Math.Max(b1.Left + b1.Width, b2.Left + b2.Width);
			int num5 = Math.Max(b1.Top + b1.Height, b2.Top + b2.Height);
			int num6 = Math.Max(b1.Near + b1.Depth, b2.Near + b2.Depth);
			return new Box(num, num2, num3, num4 - num, num5 - num2, num6 - num3);
		}

		public static bool operator ==(Box b1, Box b2)
		{
			return b1.Equals(b2);
		}

		public static bool operator !=(Box b1, Box b2)
		{
			return !b1.Equals(b2);
		}
	}
}
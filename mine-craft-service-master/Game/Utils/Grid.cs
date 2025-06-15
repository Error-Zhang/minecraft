namespace MineCraftService.Game;

public class Grid2D
{
	private readonly int _sizeX;

	private readonly int _sizeY;

	private readonly float[] _data;

	public int SizeX => _sizeX;

	public int SizeY => _sizeY;

	public Grid2D(int sizeX, int sizeY)
	{
		_sizeX = sizeX;
		_sizeY = sizeY;
		_data = new float[_sizeX * _sizeY];
	}

	public float Get(int x, int y)
	{
		return _data[x + y * _sizeX];
	}

	public void Set(int x, int y, float value)
	{
		_data[x + y * _sizeX] = value;
	}
}
public class Grid3D
	{
		private readonly int _sizeX;

		private readonly int _sizeY;

		private readonly int _sizeZ;

		private readonly int _sizeXY;

		private readonly float[] _data;

		public int SizeX => _sizeX;

		public int SizeY => _sizeY;

		public int SizeZ => _sizeZ;

		public Grid3D(int sizeX, int sizeY, int sizeZ)
		{
			_sizeX = sizeX;
			_sizeY = sizeY;
			_sizeZ = sizeZ;
			_sizeXY = _sizeX * _sizeY;
			_data = new float[_sizeX * _sizeY * _sizeZ];
		}

		public void Get8(int x, int y, int z, out float v111, out float v211, out float v121, out float v221, out float v112, out float v212, out float v122, out float v222)
		{
			int num = x + y * _sizeX + z * _sizeXY;
			v111 = _data[num];
			v211 = _data[num + 1];
			v121 = _data[num + _sizeX];
			v221 = _data[num + 1 + _sizeX];
			v112 = _data[num + _sizeXY];
			v212 = _data[num + 1 + _sizeXY];
			v122 = _data[num + _sizeX + _sizeXY];
			v222 = _data[num + 1 + _sizeX + _sizeXY];
		}

		public float Get(int x, int y, int z)
		{
			return _data[x + y * _sizeX + z * _sizeXY];
		}

		public void Set(int x, int y, int z, float value)
		{
			_data[x + y * _sizeX + z * _sizeXY] = value;
		}
	}
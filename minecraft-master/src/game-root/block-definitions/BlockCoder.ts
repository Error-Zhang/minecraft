export class BlockCoder {
	static readonly ID_MASK = 0xfff; // 12 bits for ID (0–4095)
	/** 湿度位掩码 (bit 12–15) */
	static readonly HUMIDITY_MASK = 0xf000;
	/** 温度位掩码 (bit 8–11) */
	static readonly TEMPERATURE_MASK = 0xf00;

	static decodePlantIsWild(value: number) {
		let data = this.extractData(value);
		return (data & 8) != 0;
	}

	/** 提取 ID（低 12 位） */
	static extractId(blockValue: number): number {
		if (blockValue <= this.ID_MASK) return blockValue;
		return blockValue & this.ID_MASK;
	}

	/** 提取元数据（高 4 位，位于 bit 12–15） */
	static extractData(blockValue: number): number {
		return (blockValue >> 12) & 0xf;
	}

	/** 提取湿度（4 bit） */
	static extractHumidity(shaftValue: number): number {
		return (shaftValue & this.HUMIDITY_MASK) >>> 12;
	}

	/** 提取温度（4 bit） */
	static extractTemperature(shaftValue: number): number {
		return (shaftValue & this.TEMPERATURE_MASK) >>> 8;
	}

	static decodePlantSize(value: number): number {
		let data = this.extractData(value);
		return data & 7;
	}

	static decodeWoodDirection(value: number) {
		let data = this.extractData(value);
		data &= 3;
		switch (data) {
			case 1:
				return 1;
			case 2:
				return 2;
			default:
				return 0;
		}
	}
}

type Direction = "up" | "down";

export class ParabolaMotion {
	private elapsed = 0;
	private duration: number;
	private peakHeight: number;
	private direction: Direction;
	private running = true;

	private acceleration = 0;
	private initialSpeed = 0;

	constructor(peakHeight: number, direction: Direction, duration = 1) {
		this.peakHeight = peakHeight;
		this.duration = duration;
		this.direction = direction;

		// 半程时间（上升或下降）
		const halfDuration = duration / 2;

		// 计算出加速度和初始速度（用于模拟速度变化）
		this.acceleration = peakHeight / (duration * duration);
		this.initialSpeed = this.acceleration * halfDuration;
	}

	/**
	 * 获取每帧速度值：以先快后慢上升，再慢后快下降的方式模拟跳跃
	 */
	public update(deltaTime: number): number {
		if (!this.running) return 0;

		this.elapsed += deltaTime / 1000;
		const t = this.elapsed;

		if (t >= this.duration) {
			this.running = false;
			return 0;
		}

		const halfDuration = this.duration / 2;

		let v: number;

		if (t < halfDuration) {
			// 上升：v = v0 - a * t
			v = this.initialSpeed - this.acceleration * t;
		} else {
			// 下降：v = - a * (t - halfDuration)
			const dt = t - halfDuration;
			v = -this.acceleration * dt;
		}

		// 方向决定返回值正负
		return this.direction === "up" ? v : -v;
	}

	public reset() {
		this.elapsed = 0;
		this.running = true;
	}
}

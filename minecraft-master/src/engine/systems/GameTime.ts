import { SingleClass } from "@engine/core/Singleton.ts";

export class GameTime extends SingleClass {
	/** 游戏中一天的时长（秒） */
	public readonly GAME_SECONDS_PER_DAY = 86400;
	public readonly GAME_SECONDS_PER_HOUR = 3600;
	public readonly GAME_SECONDS_PER_MINUTE = 60;

	private _totalGameSeconds = this.GAME_SECONDS_PER_DAY / 2; // 从中午开始

	constructor(private readonly realSecondsPerDay: number = 1800) {
		super();
	}

	/** 当前是第几天 */
	public get day(): number {
		return Math.floor(this._totalGameSeconds / this.GAME_SECONDS_PER_DAY);
	}

	/** 当前天内的时间（秒） */
	public get timeOfDaySeconds(): number {
		return this._totalGameSeconds % this.GAME_SECONDS_PER_DAY;
	}

	/** 当前一天中的进度（0 ~ 1） */
	public get dayProgress(): number {
		return this.timeOfDaySeconds / this.GAME_SECONDS_PER_DAY;
	}

	public set dayProgress(value: number) {
		this._totalGameSeconds = this.GAME_SECONDS_PER_DAY * value;
	}

	/** 太阳角度(-1 ~ 1) */
	public get sunInclination(): number {
		return this.dayProgress * 2 - 1;
	}

	/** 当前小时（0 ~ 23） */
	public get hour(): number {
		return Math.floor(this.timeOfDaySeconds / this.GAME_SECONDS_PER_HOUR);
	}

	/** 当前分钟（0 ~ 59） */
	public get minute(): number {
		const remainingSeconds = this.timeOfDaySeconds % this.GAME_SECONDS_PER_HOUR;
		return Math.floor(remainingSeconds / this.GAME_SECONDS_PER_MINUTE);
	}

	/** 是否是白天（默认 6:00 ~ 18:00） */
	public get isDaytime(): boolean {
		const h = this.hour;
		return h >= 6 && h < 18;
	}

	/** 获取当前时间字符串 */
	public get timeString(): string {
		const h = String(this.hour).padStart(2, "0");
		const m = String(this.minute).padStart(2, "0");
		return `Day ${this.day}, ${h}:${m}`;
	}

	/** 更新游戏时间 */
	public update(deltaTime: number) {
		const gameSecondsPerRealSecond = this.GAME_SECONDS_PER_DAY / this.realSecondsPerDay;
		this._totalGameSeconds += deltaTime * gameSecondsPerRealSecond;
	}

	/** 设置当前游戏总时间 */
	public setTime(totalGameSeconds: number) {
		this._totalGameSeconds = totalGameSeconds;
	}

	/** 重置为中午（游戏时间 12:00） */
	public dispose() {
		this._totalGameSeconds = this.GAME_SECONDS_PER_DAY / 2;
	}
}

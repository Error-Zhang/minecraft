import GameWindow from "@/game-root/core/GameWindow.ts";

// 定义所有可能的动作类型
type ActionType =
	| "moveForward"
	| "moveBackward"
	| "moveLeft"
	| "moveRight"
	| "jump"
	| "fly"
	| "sneak"
	| "sprint"
	| "attack"
	| "break"
	| "interact";

type ActionMap = {
	[K in ActionType]: {
		keys: string[]; // 绑定的物理按键
		comboKeys?: string[]; // 需要同时按下的组合键
		pressType: "hold" | "tap"; // 按压类型（持续/点按）
		holdTime?: number; // 长按判定时间（毫秒）
		cooldown?: number; // 触发间隔（毫秒）
	};
};

type EventCallback = (...args: any[]) => void;

// 默认按键配置
const DEFAULT_ACTION_MAP: ActionMap = {
	// 基础移动
	moveForward: {
		keys: ["KeyW"],
		pressType: "hold",
	},
	moveBackward: {
		keys: ["KeyS"],
		pressType: "hold",
	},
	moveLeft: {
		keys: ["KeyA"],
		pressType: "hold",
	},
	moveRight: {
		keys: ["KeyD"],
		pressType: "hold",
	},
	// 跳跃和下蹲
	jump: {
		keys: ["Space"],
		pressType: "tap",
	},
	fly: {
		keys: ["Space"],
		pressType: "hold",
	},
	sneak: {
		keys: ["ShiftLeft"],
		pressType: "hold",
	},
	// 冲刺
	sprint: {
		keys: ["ShiftLeft"],
		comboKeys: ["KeyW"], // 需要同时按W
		pressType: "hold",
	},
	// 交互
	attack: {
		keys: ["Mouse0"], // 鼠标左键
		pressType: "tap", // 点击攻击
	},
	break: {
		keys: ["Mouse0"], // 鼠标左键
		pressType: "hold", // 长按破坏
		holdTime: 200, // 200ms判定为长按
		cooldown: 200,
	},
	interact: {
		keys: ["Mouse2"], // 鼠标右键
		pressType: "tap", // 点击交互/放置
	},
};

export class PlayerInputSystem {
	private static instance: PlayerInputSystem;
	private readonly actionMap: ActionMap;
	private activeActions = new Map<ActionType, boolean>();
	private keyStates = new Map<string, boolean>();
	private eventListeners = new Map<string, EventCallback[]>();
	private keyPressStartTime = new Map<string, number>();
	private lastTriggerTime = new Map<ActionType, number>(); // 记录每个动作的最后触发时间

	// 事件类型
	private events = {
		actionStart: "action_start",
		actionEnd: "action_end",
		actionUpdate: "action_update",
	};

	private constructor() {
		this.actionMap = { ...DEFAULT_ACTION_MAP };
		this.initEventListeners();
	}

	static get Instance(): PlayerInputSystem {
		if (!PlayerInputSystem.instance) {
			PlayerInputSystem.instance = new PlayerInputSystem();
		}
		return PlayerInputSystem.instance;
	}

	onActionStart(action: ActionType, callback: () => void) {
		this.on(this.events.actionStart, (triggeredAction: ActionType) => {
			if (triggeredAction === action) callback();
		});
	}

	onActionEnd(action: ActionType, callback: () => void) {
		this.on(this.events.actionEnd, (triggeredAction: ActionType) => {
			if (triggeredAction === action) callback();
		});
	}

	onActionUpdate(action: ActionType, callback: () => void) {
		this.on(this.events.actionUpdate, (triggeredAction: ActionType) => {
			if (triggeredAction === action) callback();
		});
	}

	// 配置管理
	rebindAction(action: ActionType, newKey: string) {
		const config = this.actionMap[action];
		if (config) {
			config.keys = [newKey];
		}
	}

	// 状态查询
	isActionActive(action: ActionType): boolean {
		return this.activeActions.get(action) || false;
	}

	public update() {
		this.checkActionStates();
	}

	// 事件系统
	private on(event: string, callback: EventCallback) {
		if (!this.eventListeners.has(event)) {
			this.eventListeners.set(event, []);
		}
		this.eventListeners.get(event)!.push(callback);
	}

	private emit(event: string, ...args: any[]) {
		const listeners = this.eventListeners.get(event);
		if (listeners) {
			listeners.forEach(callback => callback(...args));
		}
	}

	private initEventListeners() {
		const window = GameWindow.Instance;

		// 键盘事件
		window.addEventListener("keydown", e => this.handleKey(e.code, true));
		window.addEventListener("keyup", e => this.handleKey(e.code, false), false);

		// 鼠标事件
		window.addEventListener("mousedown", e => this.handleKey(`Mouse${e.button}`, true));
		window.addEventListener("mouseup", e => this.handleKey(`Mouse${e.button}`, false), false);
	}

	private handleKey(code: string, isDown: boolean) {
		if (isDown) {
			this.keyPressStartTime.set(code, Date.now());
		} else {
			this.keyPressStartTime.delete(code);
		}
		this.keyStates.set(code, isDown);
	}

	private checkActionStates() {
		// 遍历所有动作配置
		(Object.entries(this.actionMap) as [ActionType, ActionMap[ActionType]][]).forEach(
			([action, config]) => {
				const isActive = this.checkActionConfig(config);
				const wasActive = this.activeActions.get(action);
				const now = Date.now();
				const lastTrigger = this.lastTriggerTime.get(action) || 0;

				// 检查触发间隔
				if (config.cooldown && now - lastTrigger < config.cooldown) {
					// 如果未达到触发间隔，且之前是激活状态，则取消激活
					if (wasActive) {
						this.activeActions.set(action, false);
						this.emitActionEvent(action, false);
					}
					return;
				}

				// 检查长按时间
				if (isActive && config.holdTime) {
					const pressTime = this.keyPressStartTime.get(config.keys[0]);
					if (pressTime && Date.now() - pressTime >= config.holdTime) {
						// 长按条件满足
						if (!wasActive) {
							this.activeActions.set(action, true);
							this.emitActionEvent(action, true);
						}
						if (config.pressType === "hold") {
							this.emit(this.events.actionUpdate, action);
						}
					} else if (wasActive) {
						// 长按条件不满足，但之前是激活状态
						this.activeActions.set(action, false);
						this.emitActionEvent(action, false);
					}
				} else {
					// 状态变化检测（非长按动作）
					if (isActive !== wasActive) {
						this.activeActions.set(action, isActive);
						this.emitActionEvent(action, isActive);
					}

					// 持续按压事件
					if (isActive && config.pressType === "hold") {
						this.emit(this.events.actionUpdate, action);
					}
				}
			}
		);
	}

	private checkActionConfig(config: ActionMap[ActionType]): boolean {
		// 检测主按键
		const mainKeyActive = config.keys.some(k => this.keyStates.get(k));

		// 检测组合键
		const comboActive = config.comboKeys
			? config.comboKeys.every(k => this.keyStates.get(k))
			: true;

		return mainKeyActive && comboActive;
	}

	private emitActionEvent(action: ActionType, isActive: boolean) {
		const eventType = isActive ? this.events.actionStart : this.events.actionEnd;
		this.emit(eventType, action);

		if (isActive) {
			this.lastTriggerTime.set(action, Date.now());
		}
	}
}

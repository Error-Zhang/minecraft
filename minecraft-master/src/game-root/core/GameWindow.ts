type GameEventHandler = (...args: any) => void;

type ActiveChangeHandler = (active: boolean, reason?: string) => void;

class GameWindow {
	private static instance: GameWindow;

	private isActive = false;
	private canvas: HTMLCanvasElement;
	private listeners: Map<string, Map<GameEventHandler, EventListener>> = new Map();

	private clickCanvasCallbacks: (() => void)[] = [];

	private activeChangeHandlers: Set<ActiveChangeHandler> = new Set();

	private constructor(canvas: HTMLCanvasElement) {
		this.canvas = canvas;

		canvas.onclick = () => {
			if (!this.isActive) {
				this.clickCanvasCallbacks.forEach(callback => callback());
				this.togglePointerLock();
			}
		};
		// 某些浏览器（例如 Chrome）不允许在解锁后立即锁定指针。https://github.com/pmndrs/drei/issues/1988
		document.addEventListener("pointerlockchange", this.boundOnPointerLockChange);
	}

	public static get Instance() {
		return this.instance;
	}

	public get isInGame(): boolean {
		return this.isActive;
	}

	public static create(canvas: HTMLCanvasElement): GameWindow {
		if (!this.instance) {
			this.instance = new this(canvas);
		}
		return this.instance;
	}

	public onClickCanvas(callback: () => void) {
		this.clickCanvasCallbacks.push(callback);
	}

	public onActiveChange(handler: ActiveChangeHandler) {
		this.activeChangeHandlers.add(handler);
	}

	public async togglePointerLock() {
		if (document.pointerLockElement === this.canvas) {
			document.exitPointerLock();
		} else {
			await this.canvas.requestPointerLock();
		}
	}

	public addEventListener(type: string, handler: GameEventHandler, isBlock = true) {
		const wrapped: EventListener = (event: Event) => {
			if (this.isActive || !isBlock) {
				handler(event);
			}
		};

		if (!this.listeners.has(type)) {
			this.listeners.set(type, new Map());
		}

		this.listeners.get(type)!.set(handler, wrapped);
		window.addEventListener(type, wrapped);
	}

	public removeEventListener(type: string, handler: GameEventHandler) {
		const typeMap = this.listeners.get(type);
		if (!typeMap) return;

		const wrapped = typeMap.get(handler);
		if (!wrapped) return;

		window.removeEventListener(type, wrapped);
		typeMap.delete(handler);

		if (typeMap.size === 0) {
			this.listeners.delete(type);
		}
	}

	public dispose() {
		for (const [type, handlerMap] of this.listeners.entries()) {
			for (const wrapped of handlerMap.values()) {
				window.removeEventListener(type, wrapped);
			}
		}
		this.listeners.clear();
		this.clickCanvasCallbacks.length = 0;
		document.removeEventListener("pointerlockchange", this.boundOnPointerLockChange);
		this.activeChangeHandlers.clear();
	}

	// 触发激活状态变化回调
	private notifyActiveChange(isActive: boolean) {
		this.isActive = isActive;
		for (const handler of this.activeChangeHandlers) {
			try {
				handler(isActive);
			} catch (e) {
				console.error("ActiveChangeHandler error:", e);
			}
		}
	}

	private onPointerLockChange = () => {
		let isActive = document.pointerLockElement === this.canvas;
		this.notifyActiveChange(isActive);
	};
	private readonly boundOnPointerLockChange = this.onPointerLockChange.bind(this);
}

export default GameWindow;

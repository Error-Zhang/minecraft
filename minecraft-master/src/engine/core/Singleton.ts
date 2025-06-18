export class Singleton {
	private static instances = new Map<Function, SingleClass>();

	/**
	 * 创建单例实例
	 */
	public static create<T extends SingleClass, C extends new (...args: any[]) => T>(
		clazz: C,
		...args: ConstructorParameters<C>
	): T {
		if (this.instances.has(clazz)) {
			return <T>this.instances.get(clazz);
		}
		const instance = Reflect.construct(clazz, args);
		this.instances.set(clazz, instance);
		return instance;
	}

	/**
	 * 释放所有单例
	 */
	public static disposeAll() {
		this.instances.forEach(instance => instance.dispose());
		this.instances.clear();
	}

	/**
	 * 释放指定单例
	 */
	public static dispose<T extends SingleClass>(clazz: new (...args: any[]) => T): void {
		this.instances.get(clazz)?.dispose();
		this.instances.delete(clazz);
	}

	/**
	 * 获取单例（必须已通过 create 创建）
	 */
	protected static getInstance<T extends SingleClass>(clazz: new (...args: any[]) => T) {
		if (!this.instances.has(clazz)) {
			throw new Error("Singleton not initialized. Call create() first.");
		}
		return <T>this.instances.get(clazz);
	}
}

export class SingleClass extends Singleton {
	// 静态访问
	public static get Instance() {
		return this.getInstance();
	}

	public static dispose() {
		super.dispose(this);
	}

	public static getInstance<T extends SingleClass>() {
		return <T>super.getInstance(this);
	}

	/**
	 * 子类必须实现：用于释放资源等
	 */
	public dispose() {
		throw new Error("dispose must be override.");
	}
}

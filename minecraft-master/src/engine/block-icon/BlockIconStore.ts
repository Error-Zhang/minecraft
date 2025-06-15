import { IDBPDatabase, openDB } from "idb";

interface BlockDB {
	blocks: {
		key: string;
		value: Blob; // 直接存储 Blob
	};
}

export class BlockIconStore {
	private static dbPromise: Promise<IDBPDatabase<BlockDB>>;

	/** 初始化数据库（懒加载） */
	private static get db() {
		if (!this.dbPromise) {
			this.dbPromise = openDB<BlockDB>("BlockDB", 1, {
				upgrade(db) {
					if (!db.objectStoreNames.contains("blocks")) {
						db.createObjectStore("blocks");
					}
				},
			});
		}
		return this.dbPromise;
	}

	/** 获取图标总数 */
	static async count(): Promise<number> {
		const db = await this.db;
		return await db.count("blocks");
	}

	/** 存储图像 Blob */
	static async set(key: string, blob: Blob): Promise<void> {
		const db = await this.db;
		await db.put("blocks", blob, key);
	}

	/** 获取图像 Blob */
	static async get(key: string): Promise<Blob | undefined> {
		const db = await this.db;
		return await db.get("blocks", key);
	}

	/** 删除图像 */
	static async delete(key: string): Promise<void> {
		const db = await this.db;
		await db.delete("blocks", key);
	}

	/** 清空所有图像缓存 */
	static async clear(): Promise<void> {
		const db = await this.db;
		await db.clear("blocks");
	}

	/** 判断图像是否存在 */
	static async has(key: string): Promise<boolean> {
		return (await this.get(key)) !== undefined;
	}

	/**
	 * 将 Blob 转换为可在 <img> 或 <a> 中使用的 URL
	 * @returns 可用的 Object URL
	 * @param key
	 */
	static async getIconUrl(key: string): Promise<string> {
		const blob = await this.get(key);
		return blob ? URL.createObjectURL(blob) : "";
	}

	/** 根据传入的 keys 获取所有对应的 icon URL */
	static async getIconUrls(keys: string[]) {
		const db = await this.db;
		const tx = db.transaction("blocks", "readonly");
		const store = tx.objectStore("blocks");

		const result: Record<string, string> = {};

		for (const key of keys) {
			const blob = await store.get(key);
			if (blob) {
				result[key] = URL.createObjectURL(blob);
			}
		}

		return result;
	}
}

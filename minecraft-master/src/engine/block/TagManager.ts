export type TagID = `#${string}:${string}`;

export class TagManager {
	// 存储所有标签 [TagID → TagDefinition]
	private tags: Map<TagID, Set<number>>;

	public constructor() {
		this.tags = new Map();
	}

	/**
	 * 创建/更新标签
	 * @param id 标签ID (格式 "#namespace:path")
	 * @param resources 包含的资源ID数组
	 */
	createTag(id: TagID, resources: number[]): void {
		// 验证ID格式
		if (!id.startsWith("#") || id.split(":").length !== 2) {
			throw new Error(`Invalid tag ID format: ${id}. Must be "#namespace:path"`);
		}

		const members = new Set(resources);

		// 存储标签定义
		this.tags.set(id, members);
	}

	/**
	 * 检查资源是否属于某个标签
	 * @param resource 资源ID
	 * @param tag 标签ID
	 */
	hasTag(resource: number, tag: TagID): boolean {
		return this.tags.get(tag)?.has(resource) ?? false;
	}

	/**
	 * 获取标签的所有成员
	 * @param tag 标签ID
	 * @returns 资源ID数组
	 */
	getMembers(tag: TagID): number[] {
		return Array.from(this.tags.get(tag) ?? []);
	}

	/**
	 * 添加资源到现有标签
	 * @param tag 标签ID
	 * @param resources 要添加的资源ID数组
	 */
	addToTag(tag: TagID, resources: number[]): void {
		const tagDef = this.tags.get(tag);
		if (!tagDef) {
			this.createTag(tag, resources);
		} else {
			resources.forEach(resource => {
				tagDef.add(resource);
			});
		}
	}

	/**
	 * 从标签移除资源
	 * @param tag 标签ID
	 * @param resources 要移除的资源ID数组
	 */
	removeFromTag(tag: TagID, resources: number[]): void {
		const tagDef = this.tags.get(tag);
		if (!tagDef) return;

		resources.forEach(resource => {
			tagDef.delete(resource);
		});
	}

	/**
	 * 合并多个标签的成员（逻辑OR）
	 * @param tags 标签ID数组
	 * @returns 合并后的资源ID集合
	 */
	unionTags(tags: TagID[]): Set<number> {
		const result = new Set<number>();
		tags.forEach(tag => {
			this.getMembers(tag).forEach(resource => result.add(resource));
		});
		return result;
	}

	/**
	 * 查找多个标签的交集（逻辑AND）
	 * @param tags 标签ID数组
	 * @returns 交集的资源ID集合
	 */
	intersectTags(tags: TagID[]): Set<number> {
		if (tags.length === 0) return new Set();

		const sets: Set<number>[] = tags.map(tag => this.tags.get(tag) ?? new Set());
		const result = new Set(sets[0]);

		for (let i = 1; i < sets.length; i++) {
			sets[i].forEach(resource => {
				if (!result.has(resource)) {
					result.delete(resource);
				}
			});
		}

		return result;
	}
}

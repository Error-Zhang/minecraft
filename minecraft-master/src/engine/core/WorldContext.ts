import { Scene } from "@babylonjs/core";
import { Singleton } from "@engine/core/Singleton.ts";

export interface DisposableModule {
	constructor: any;

	dispose(): void;
}

export class WorldContext {
	private modules: DisposableModule[] = [];

	constructor(public scene: Scene) {}

	add(module: DisposableModule) {
		this.modules.push(module);
	}

	disposeAll() {
		for (const m of this.modules.reverse()) {
			try {
				m.dispose?.();
				Singleton.dispose(m.constructor);
			} catch (err) {
				console.error("Dispose error in module", m, err);
			}
		}
		this.modules = [];
		this.disposeSceneSafe(this.scene);
	}

	public disposeSceneSafe(scene: Scene) {
		// 安全清理所有资源
		scene.meshes.forEach(m => m.dispose());
		scene.materials.forEach(m => m.dispose());
		scene.textures.forEach(t => t.dispose());
		scene.lights.forEach(l => l.dispose());
		scene.cameras.forEach(c => c.dispose());
		scene.particleSystems.forEach(p => p.dispose());
		scene.postProcesses.forEach(p => p.dispose());
		scene.skeletons.forEach(s => s.dispose());
		scene.animationGroups.forEach(a => a.dispose());

		// 清理 GUI（如果使用了 Babylon GUI）
		if ((scene as any).layerMasks) (scene as any).layerMasks.length = 0;
		if ((scene as any).ui) (scene as any).ui.dispose?.();

		// 最后 dispose 场景本身
		scene.dispose();
	}
}

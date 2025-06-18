import {
	AdvancedDynamicTexture,
	Control,
	Grid,
	Rectangle,
	StackPanel,
	TextBlock,
} from "@babylonjs/gui";
import { Scene } from "@babylonjs/core";

export class LoadingScreen {
	private guiTexture: AdvancedDynamicTexture;
	private progressBlocks: Rectangle[] = [];
	private worldInfoText: TextBlock;
	private seasonText: TextBlock;
	private timeText: TextBlock;

	private readonly seasonColors = {
		春季: "#66BB6A", // Spring green
		夏季: "#FFCA28", // Summer yellow
		秋季: "#FF7043", // Autumn orange
		冬季: "#42A5F5", // Winter blue
	};
	private iconProgressText: TextBlock;

	constructor(scene: Scene) {
		this.guiTexture = AdvancedDynamicTexture.CreateFullscreenUI("LoadingScreen", true, scene);
		this.guiTexture.background = "black";

		// 主容器
		const mainContainer = new Rectangle();
		mainContainer.width = "100%";
		mainContainer.height = "100%";
		mainContainer.thickness = 0;
		mainContainer.verticalAlignment = Control.VERTICAL_ALIGNMENT_CENTER;
		mainContainer.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
		this.guiTexture.addControl(mainContainer);

		// 垂直布局
		const stackPanel = new StackPanel();
		stackPanel.width = "100%";
		stackPanel.isVertical = true;
		stackPanel.paddingTop = "20px";
		stackPanel.spacing = 14;
		mainContainer.addControl(stackPanel);

		// 世界名
		this.worldInfoText = new TextBlock();
		this.worldInfoText.color = "#FFFFFF";
		this.worldInfoText.fontSize = 20;
		this.worldInfoText.height = "30px";
		this.worldInfoText.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
		stackPanel.addControl(this.worldInfoText);

		// 中间一行：季节和时间（并排）
		const infoGrid = new Grid();
		infoGrid.width = "200px";
		infoGrid.height = "30px";
		infoGrid.addColumnDefinition(0.5);
		infoGrid.addColumnDefinition(0.5);
		infoGrid.addRowDefinition(1);
		stackPanel.addControl(infoGrid);

		this.seasonText = new TextBlock();
		this.seasonText.fontSize = 16;
		this.seasonText.height = "100%";
		this.seasonText.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
		this.seasonText.paddingLeft = "0px";
		infoGrid.addControl(this.seasonText, 0, 0);

		this.timeText = new TextBlock();
		this.timeText.fontSize = 16;
		this.timeText.height = "100%";
		this.timeText.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
		this.timeText.paddingRight = "0px";
		infoGrid.addControl(this.timeText, 0, 1);

		// 进度方块
		this.createProgressBlocks(stackPanel);

		// 当前图标生成进度文字
		this.iconProgressText = new TextBlock();
		this.iconProgressText.color = "#CCCCCC";
		this.iconProgressText.fontSize = 16;
		this.iconProgressText.height = "30px";
		this.iconProgressText.textHorizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
		stackPanel.addControl(this.iconProgressText);
	}

	public dispose() {
		this.guiTexture.dispose();
	}

	public update(props: { worldName: string; seasonId: number; time: string; progress: number }) {
		const { worldName, seasonId, time, progress } = props;
		let season: "春季" | "夏季" | "秋季" | "冬季";
		switch (seasonId) {
			case 0:
				season = "春季";
				break;
			case 1:
				season = "夏季";
				break;
			case 2:
				season = "秋季";
				break;
			case 3:
				season = "冬季";
				break;
			default:
				season = "春季";
		}
		this.worldInfoText.text = `正在加载 ${worldName} 的世界`;
		this.seasonText.text = `季节: ${season}`;
		this.timeText.text = `时间: ${time}`;

		const color = this.seasonColors[season] ?? "#CCCCCC";
		this.seasonText.color = color;
		this.timeText.color = "#FFFFFF";

		const blockCount = this.progressBlocks.length;
		const totalProgress = progress * blockCount;

		this.progressBlocks.forEach((block, index) => {
			const p = index / (blockCount - 1);
			if (index < totalProgress) {
				const brightness = 0.5 + 0.5 * p; // 从 0.5 到 1.0
				block.background = this.adjustColorBrightness(color, brightness);
			} else {
				block.background = "#333333";
			}
		});
	}

	public updateIconText(text: string) {
		this.iconProgressText.text = text;
	}

	private adjustColorBrightness(hex: string, factor: number): string {
		// 去掉 #
		hex = hex.replace("#", "");

		const r = Math.min(255, Math.round(parseInt(hex.substring(0, 2), 16) * factor));
		const g = Math.min(255, Math.round(parseInt(hex.substring(2, 4), 16) * factor));
		const b = Math.min(255, Math.round(parseInt(hex.substring(4, 6), 16) * factor));

		return `rgb(${r}, ${g}, ${b})`;
	}

	private createProgressBlocks(parent: StackPanel) {
		const blockSize = 20;
		const spacing = 8;
		const count = 5;

		const container = new Rectangle();
		container.width = `${(blockSize + spacing) * count - spacing}px`;
		container.height = "30px";
		container.thickness = 0;
		container.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
		container.verticalAlignment = Control.VERTICAL_ALIGNMENT_CENTER;
		parent.addControl(container);

		for (let i = 0; i < count; i++) {
			const block = new Rectangle();
			block.width = `${blockSize}px`;
			block.height = `${blockSize}px`;
			block.thickness = 0;
			block.background = "#333333";
			block.left = `${i * (blockSize + spacing) - ((blockSize + spacing) * (count - 1)) / 2}px`;
			block.top = "0px";
			block.cornerRadius = 0;
			block.horizontalAlignment = Control.HORIZONTAL_ALIGNMENT_CENTER;
			block.verticalAlignment = Control.VERTICAL_ALIGNMENT_CENTER;

			container.addControl(block);
			this.progressBlocks.push(block);
		}
	}
}

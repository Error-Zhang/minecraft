import React, { useEffect, useState } from "react";
import "./index.less";
import GameButton from "@/ui-root/components/game-button";
import exitIcon from "@/ui-root/assets/icons/exit.svg";
import GameInput from "@/ui-root/components/game-input";
import GameSelect from "@/ui-root/components/game-select";
import { openGameDialog } from "@/ui-root/components/game-dialog/dialogService.tsx";
import {
	gameModeOptions,
	generateRandomSeed,
	generateRandomWorldName,
	publicOptions,
	seasonOptions,
	worldModeOptions,
} from "./constant.ts";
import { IWorld } from "@/ui-root/api/interface.ts";
import { worldApi } from "../../../api";

interface WorldFormPanelProps {
	world?: IWorld;
	onSubmit: () => void;
	onCancel: () => void;
	isEditing?: boolean;
}

const WorldFormPanel: React.FC<WorldFormPanelProps> = ({
	isEditing,
	onSubmit,
	onCancel,
	world,
}) => {
	const [formData, setFormData] = useState<IWorld>({
		worldName: generateRandomWorldName(),
		seed: generateRandomSeed(),
		gameMode: 0,
		worldMode: 0,
		season: 0,
		isPublic: 0,
	});

	// 初始化表单数据
	useEffect(() => {
		if (isEditing && world) {
			setFormData({ ...world });
		}
	}, [isEditing, world]);

	const handleChange = (field: keyof typeof formData, value: any) => {
		setFormData(prev => ({ ...prev, [field]: value }));
	};

	const handleSubmit = async () => {
		const data = {
			...formData,
			worldName: formData.worldName || generateRandomWorldName(),
			seed: formData.seed || generateRandomSeed(),
		};
		const api = isEditing ? worldApi.updateWorld : worldApi.createWorld;

		try {
			await api(data);
			onSubmit?.();
		} catch (e: any) {
			openGameDialog({
				title: e.message,
				message: e.data ? JSON.stringify(e.data) : "",
			});
		}
	};

	return (
		<div className="world-form-panel">
			<h2 className="title">{isEditing ? "编辑世界" : "创建世界"}</h2>
			<GameButton className="leave" onClick={onCancel}>
				<img src={exitIcon} />
			</GameButton>
			<div className="form-fields">
				<GameInput
					label="世界名字"
					maxLength={16}
					placeholder="默认随机生成"
					value={formData.worldName}
					onChange={e => handleChange("worldName", e.target.value)}
				/>
				<GameInput
					label="世界种子"
					maxLength={16}
					placeholder="默认随机生成"
					value={formData.seed}
					onChange={e => handleChange("seed", e.target.value)}
					disabled={isEditing}
				/>
				<GameSelect
					label="游戏模式"
					options={gameModeOptions}
					value={formData.gameMode}
					onChange={v => handleChange("gameMode", v)}
				/>
				<GameSelect
					label="世界类型"
					options={worldModeOptions}
					value={formData.worldMode}
					onChange={v => handleChange("worldMode", v)}
					disabled={isEditing}
				/>
				<GameSelect
					label="季节"
					options={seasonOptions}
					value={formData.season}
					onChange={v => handleChange("season", v)}
					disabled={isEditing}
				/>
				<GameSelect
					label="世界显示"
					options={publicOptions}
					value={formData.isPublic}
					onChange={v => handleChange("isPublic", v)}
				/>
			</div>
			<div className="form-actions">
				<GameButton className="submit" onClick={handleSubmit}>
					{isEditing ? "保存更改" : "创建世界"}
				</GameButton>
			</div>
		</div>
	);
};

export default WorldFormPanel;

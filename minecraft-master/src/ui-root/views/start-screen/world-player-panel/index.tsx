import React, { useState } from "react";
import "./index.less";
import GameButton from "@/ui-root/components/game-button";
import GameInput from "@/ui-root/components/game-input";
import GameSelect from "@/ui-root/components/game-select";
import exitIcon from "@/ui-root/assets/icons/exit.svg";
import { useUserStore } from "@/store";
import { IPlayer } from "@/ui-root/api/interface.ts";
import { playerApi } from "../../../api";

interface WorldPlayersPanelProps {
	worldId: number;
	onBack: () => void;
	onEnter: () => void;
}

const sexOptions = [
	{ label: "男", value: 0 },
	{ label: "女", value: 1 },
];
const WorldPlayersPanel: React.FC<WorldPlayersPanelProps> = ({ worldId, onBack, onEnter }) => {
	const { userId, username } = useUserStore();
	const [player, setPlayer] = useState<IPlayer>({
		playerName: username,
		sex: 0,
	} as any);

	const addPlayer = async (player: IPlayer) => {
		player.userId = userId!;
		player.worldId = worldId;
		await playerApi.addPlayerToWorld(player);
	};

	return (
		<div className="backdrop">
			<div className="world-players-panel absolute-center">
				<h2 className="title">创建玩家</h2>
				<GameButton className="leave" onClick={onBack}>
					<img src={exitIcon} />
				</GameButton>
				<div className="player-list">
					<GameInput
						label="玩家名称"
						value={player.playerName}
						onChange={e => setPlayer({ ...player, playerName: e.target.value })}
					/>
					<GameSelect
						label="性别"
						options={sexOptions}
						value={player.sex}
						onChange={v => setPlayer({ ...player, sex: v })}
					/>
				</div>
				<div className="actions">
					<GameButton
						className="enter-btn"
						disabled={!player.playerName || player.sex === undefined}
						onClick={() => {
							addPlayer(player).then(() => {
								onEnter();
							});
						}}
					>
						进入世界
					</GameButton>
				</div>
			</div>
		</div>
	);
};

export default WorldPlayersPanel;

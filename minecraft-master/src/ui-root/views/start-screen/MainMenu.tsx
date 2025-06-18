import React, { useState } from "react";
import "./index.less";
import titleIcon from "@/ui-root/assets/gui/Title.webp";
import webIcon from "@/ui-root/assets/gui/Web.webp";
import GameButton from "@/ui-root/components/game-button";
import AuthModal from "./auth-modal";
import { ScreenPage } from "@/ui-root/views/start-screen/index.tsx";
import { useUserStore } from "@/store";

const MainMenu: React.FC<{ setPage: (page: ScreenPage) => void }> = ({ setPage }) => {
	const [showModal, setShowModal] = useState(false);
	const { username } = useUserStore();
	return (
		<div className="main-menu">
			<span className="user">
				{username ? `玩家:${username.charAt(0).toUpperCase() + username.slice(1)}` : "未登录"}
			</span>

			<img src={titleIcon} alt="" className="logo" />
			<div className="menu-buttons">
				<GameButton onClick={() => setPage("worldManager")}> 开始 </GameButton>
				<GameButton> 内容 </GameButton>
				<GameButton> 设置 </GameButton>
				<GameButton> 帮助 </GameButton>
			</div>

			<GameButton className="login-btn" onClick={() => setShowModal(true)}>
				<img src={webIcon} alt="" />
			</GameButton>
			{showModal && <AuthModal onClose={() => setShowModal(false)} />}
		</div>
	);
};
export default MainMenu;

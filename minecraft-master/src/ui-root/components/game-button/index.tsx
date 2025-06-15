import React, { ReactNode } from "react";
import "./index.less";
import { audios } from "@/ui-root/assets/sounds";

interface GameButtonProps {
	children?: ReactNode;
	onClick?: () => void;
	className?: string;
	disabled?: boolean;
}

const GameButton: React.FC<GameButtonProps> = ({ children, onClick, className, disabled }) => {
	return (
		<button
			disabled={disabled}
			className={`game-button ${className}`}
			onClick={() => {
				audios.ButtonClick.play();
				onClick?.();
			}}
		>
			{children}
		</button>
	);
};

export default GameButton;

import React from "react";
import "./index.less";

interface GameInputProps extends React.InputHTMLAttributes<HTMLInputElement> {
	label?: string;
}

const GameInput: React.FC<GameInputProps> = ({ label, ...props }) => {
	return (
		<div className="game-input">
			{label && <label className="game-input-label">{label}</label>}
			<input className="game-input-field" {...props} />
		</div>
	);
};

export default GameInput;

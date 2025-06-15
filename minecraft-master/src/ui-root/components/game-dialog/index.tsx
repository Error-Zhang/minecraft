import React from "react";
import "./index.less";
import GameButton from "@/ui-root/components/game-button";

interface ConfirmDialogProps {
	show: boolean;
	title: string;
	message?: string;
	onConfirm?: () => void;
	onCancel?: () => void;
}

const GameDialog: React.FC<ConfirmDialogProps> = ({
	show = false,
	title,
	message,
	onConfirm,
	onCancel,
}) => {
	const handleBackdropClick = (e: React.MouseEvent) => {
		e.stopPropagation();
		onCancel?.();
	};

	return (
		show && (
			<div className="confirm-backdrop" onClick={handleBackdropClick}>
				<div className="confirm-dialog">
					<h2>{title}</h2>
					<p>{message}</p>
					<div className="confirm-actions">
						<GameButton className="btn cancel" onClick={onCancel}>
							取消
						</GameButton>
						<GameButton className="btn confirm" onClick={onConfirm}>
							确认
						</GameButton>
					</div>
				</div>
			</div>
		)
	);
};

export default GameDialog;

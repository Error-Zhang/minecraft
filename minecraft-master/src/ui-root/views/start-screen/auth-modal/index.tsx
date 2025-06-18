import React, { useState } from "react";
import "./index.less";
import { userApi } from "../../../api";
import { useUserStore } from "@/store";

interface AuthModalProps {
	onClose: () => void;
}

const AuthModal: React.FC<AuthModalProps> = ({ onClose }) => {
	const [mode, setMode] = useState<"login" | "register">("login");
	const [username, setUsername] = useState("");
	const [password, setPassword] = useState("");
	const [confirmPassword, setConfirmPassword] = useState("");
	const [error, setError] = useState<string | null>(null);
	const [loading, setLoading] = useState(false);

	const submit = async (e: React.FormEvent) => {
		e.preventDefault();
		setError(null);

		if (!username || !password) {
			setError("请输入用户名和密码");
			return;
		}

		if (mode === "register" && password !== confirmPassword) {
			setError("两次密码不一致");
			return;
		}
		setLoading(true);
		let userId: number;
		try {
			if (mode === "register") {
				const { id } = await userApi.register({ userName: username, passWord: password });
				userId = id!;
			} else {
				const { id } = await userApi.login({ userName: username, passWord: password });
				userId = id!;
			}
			useUserStore.setState({ userId, username });
			setLoading(false);
		} finally {
			onClose();
		}
	};

	return (
		<div className="auth-modal-backdrop">
			<div className="auth-modal" onClick={e => e.stopPropagation()}>
				<span className="title">连接到服务器</span>
				<form onSubmit={submit}>
					<input
						type="text"
						placeholder="用户名"
						value={username}
						onInput={() => setError("")}
						onChange={e => setUsername(e.target.value)}
					/>
					<input
						type="password"
						placeholder="密码"
						value={password}
						onInput={() => setError("")}
						onChange={e => setPassword(e.target.value)}
					/>
					{mode === "register" && (
						<input
							type="password"
							placeholder="确认密码"
							value={confirmPassword}
							onInput={() => setError("")}
							onChange={e => setConfirmPassword(e.target.value)}
						/>
					)}
					{error && <div className="error">{error}</div>}
					<button type="submit" disabled={loading}>
						{loading ? "处理中..." : mode === "login" ? "登录" : "注册"}
					</button>
				</form>
				<div className="auth-switch-container">
					<>
						{mode === "login" ? (
							<span className="auth-switch" onClick={() => setMode("register")}>
								没有账号？注册
							</span>
						) : (
							<span className="auth-switch" onClick={() => setMode("login")}>
								已有账号？登录
							</span>
						)}
					</>
					<span
						className="auth-switch"
						onClick={() => {
							useUserStore.getState().reset();
							onClose();
						}}
					>
						退出登录
					</span>
				</div>
				<button className="auth-close" onClick={onClose}>
					×
				</button>
			</div>
		</div>
	);
};

export default AuthModal;

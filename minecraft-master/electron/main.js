import { app, BrowserWindow } from "electron";

function createWindow() {
	const win = new BrowserWindow({
		width: 1280,
		height: 720,
		webPreferences: {
			contextIsolation: true,
		},
	});

	// 加载本地 HTML（Vite 构建后的 index.html）
	win.loadFile("../dist/index.html");

	// win.webContents.openDevTools(); // 开发时开启控制台
}

app.whenReady().then(createWindow);

app.on("window-all-closed", () => {
	if (process.platform !== "darwin") app.quit();
});

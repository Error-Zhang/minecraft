import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
	plugins: [react()],
	resolve: {
		alias: {
			"@": "/src", // 设置@指向src文件夹
			"@engine": "/src/engine",
		},
	},
	assetsInclude: ["**/*.gltf", "**/*.glb"],
	base: "./", // 让electron路径从相对路径生成（避免 file:// 错误）
	build: {
		target: "es2020",
		sourcemap: false, // 构建时关闭 source map
		minify: "esbuild", // 使用 esbuild 进行压缩，速度快
		terserOptions: {
			format: {
				comments: false, // 删除注释
			},
			compress: {
				drop_console: true, // 删除 console.*
				drop_debugger: true, // 删除 debugger
				pure_funcs: ["console.log"], // 确保 console.log 被当作无副作用函数清除
			},
		},
		rollupOptions: {
			output: {
				manualChunks(id) {
					if (id.includes("node_modules")) {
						if (id.includes("@babylonjs")) return "babylon";
						if (id.includes("react")) return "react";
						if (id.includes("havok")) return "havok";
						return "vendor";
					}
				},
			},
		},
		assetsInlineLimit: 0, // 所有资源都单独打包为文件，不 inline
		chunkSizeWarningLimit: 3000, // 默认是 500kb，游戏项目可适当放宽
		cssCodeSplit: true, // 拆分 CSS
		emptyOutDir: true, // 每次构建清空 dist
	},
	worker: {
		format: "es",
	},
	server: {
		port: 4110,
		/*proxy: {
			// 代理 API 请求
			"/api": {
				target: "http://localhost:5110/api", // 后端服务地址
				changeOrigin: true,
				rewrite: path => path.replace(/^\/api/, ""),
			},
		},*/
	},
});

# Minecraft Web

## 项目简介
本项目是一个 Minecraft 风格的游戏项目，包含基于 Babylon.js + React + Electron 的前端（含桌面端）和基于 .NET 10 的后端服务。前端负责游戏渲染、UI 和桌面端打包，后端负责游戏数据、实时通信和 API。

## 视频介绍
🎬 [bilibili视频介绍](https://www.bilibili.com/video/BV1g6MqzFExL/)

## 目录结构
```
├── minecraft-master/          # 前端项目
│   ├── src/                  # 源代码目录
│   │   ├── game-root/       # 游戏核心逻辑
│   │   ├── ui-root/         # 用户界面
│   │   ├── engine/          # 游戏引擎
│   │   ├── store/           # 状态管理
│   │   ├── main.tsx         # 应用入口
│   │   └── App.tsx          # 主应用组件
│   ├── electron/            # Electron 相关配置
│   ├── public/              # 静态资源
│   └── package.json         # 项目配置和依赖
│
├── mine-craft-service-master/ # 后端项目
│   ├── Game/                # 游戏核心逻辑
│   ├── GameDataBase/        # 数据库相关
│   ├── GameHub/             # 实时通信中心
│   ├── Web/                 # Web API 和路由
│   ├── Program.cs           # 服务端入口
│   └── MineCraftService.csproj # 项目配置和依赖
│
├── .gitignore               # Git 忽略文件
└── README.md                # 项目说明文档
```

## 技术栈
### 前端
- **Babylon.js**: 3D 游戏引擎
- **React**: 用户界面开发
- **TypeScript**: 类型安全的 JavaScript 超集
- **Electron**: 跨平台桌面应用开发
- **Vite**: 现代前端构建工具

### 后端
- **.NET 10**: 后端框架
- **Entity Framework Core**: 数据库 ORM
- **SignalR**: 实时通信
- **SQLite**: 数据库

## 安装与运行
### 前端
1. 进入前端目录：
   ```bash
   cd minecraft-master
   ```
2. 安装依赖：
   ```bash
   yarn install
   # 或
   npm install
   ```
3. 开发模式启动：
   ```bash
   yarn dev
   # 或
   npm run dev
   ```
4. 构建桌面端：
   ```bash
   yarn electron:build
   # 或
   npm run electron:build
   ```

### 后端
1. 进入后端目录：
   ```bash
   cd mine-craft-service-master
   ```
2. 安装依赖：
   ```bash
   dotnet restore
   ```
3. 数据库生成与迁移：
   - 确保已安装以下工具和包：
     ```bash
     dotnet tool install --global dotnet-ef
     dotnet add package Microsoft.EntityFrameworkCore.Sqlite
     dotnet add package Microsoft.EntityFrameworkCore.Design
     ```
   - 按顺序执行以下命令：
     ```bash
     # 添加初始迁移
     dotnet ef migrations add InitialCreate
     # 应用迁移，创建表结构
     dotnet ef database update
     ```
4. 启动服务：
   ```bash
   dotnet run
   ```


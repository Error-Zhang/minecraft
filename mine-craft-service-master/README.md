# Minecraft 服务端项目文档

## 项目概述
这是一个基于 ASP.NET Core 开发的 Minecraft 服务端项目，提供了游戏服务器所需的各种功能和服务。

## 技术栈
- ASP.NET Core
- Entity Framework Core
- SQLite 数据库
- SignalR 实时通信
- OpenAPI (Swagger)

## 项目结构
```
MineCraftService/
├── GameDataBase/        # 数据库相关
├── GameHub/            # SignalR 实时通信
└── Web/                # Web API 接口
```

## 配置说明

### 服务器配置
- 服务端口: 5110
- 允许的跨域源: http://localhost:4110

### 数据库配置
- 使用 SQLite 数据库
- 自动执行数据库迁移

### API 文档
- 访问地址: http://localhost:5110/openapi/v1.json
- 仅在开发环境启用

## 主要服务
- UserService: 用户服务
- PlayerService: 玩家服务
- WorldService: 世界服务
- ChunkService: 区块服务

## 静态资源
- 静态文件目录: /Assets
- 访问路径: /assets

## 开发环境要求
- .NET 10.0
- SQLite 数据库

## 启动说明
1. 确保已安装所需依赖
2. 运行项目
3. 数据库将自动创建和迁移
4. 访问 http://localhost:5110 开始使用

## 打包说明

### 独立部署
```bash
# 发布到指定目录
dotnet publish -c Release -o ./publish

# 发布为单个文件（包含所有依赖）
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o ./publish
```
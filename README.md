# FGUFW_UPM
unity程序框架fgufw的unity package

## 项目目录
```
FGUFW_UPM
│
├── Packages
│
│   ├── com.fgufw.core
│   │   ├── package.json
│   │   ├── Runtime
│   │   ├── Editor
│   │   ├── Tests
│   │   ├── Samples~
│   │   ├── Documentation~
│   │   ├── CHANGELOG.md
│   │   └── README.md
│   │
│   └── com.fgufw.xxx
│
├── LICENSE
└── README.md
```

### package.json
- dependencies无法关联自定义upm 需要手动在manifest.json配置引用 ~~~也许可以使用自定义包试试~~~
```
{
  "name": "com.fgufw.core",
  "version": "1.0.0",
  "displayName": "FGUFW Core",
  "description": "FGUFW Unity Framework Core Package",
  "unity": "2022.3",
  "license": "GPL-3.0",
  "author": 
  {
    "name": "feigebabata"
  },
  "dependencies": 
  {
    "com.unity.addressables": "1.21.21",
    "com.fgufw.core": "file:../FGUFW_UPM/Packages/com.fgufw.core"
  }
}
```

### CHANGELOG.md
```
# Changelog

All notable changes to this package will be documented in this file.


## [1.0.0] - 2026-08-18 //版本号 日期

### Added  //新增

- Initial release of FGUFW Core
- Added core framework architecture
- Added module initialization system
- Added runtime lifecycle management


### Changed //修改

- None


### Fixed //修复

- None


### Removed //移除

- None

### Breaking Changes //破坏性修改

- Removed FGUFW.Manager
- Replace with FGUFW.Service
```

### 版本管理
- 通过打tag确定内容到commit
- tag需要带包名 core@1.0.0
- 例:https://github.com/feigebabata/FGUFW_UPM.git?path=/Packages/com.fgufw.core#core@1.0.0
- 版本号说明: 不兼容旧版本.新增功能.改bug

### 快捷提交
执行release.bat脚本 参数:包缩写 版本号
```
release.bat core 1.0.0
```

### 同步修改
- 把仓库git clone到本地 在manifest.json中引用本地路径就能方便调试

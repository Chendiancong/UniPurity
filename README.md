# UniPurity
focus-creative-games的HybridCLR项目提供了一种全新的、基于il2cpp的脚本热更新方案。它不采用虚拟机的方式，通过il2cpp以解释器的方式执行IL代码，相比于传统方案，可以做到aot和hotupdate代码几乎无感的融合，并且能够直接使用到C#和Unity的绝大部分特性和api，在开发效率上应该会有不错的提升。   
这个项目是在HybridCLR的基础上对于一些打包和游戏启动工作流的完善，也是对其的一次学习和尝试。   
需要注意的是，使用UniPurity需要先安装HybridCLR的工具套件   
可以通过gitee的url来安装：https://gitee.com/focus-creative-games/hybridclr_unity.git,   
也可以先clone下来，进行本地安装

# 安装
# UniPurity
- focus-creative-games的HybridCLR项目提供了一种全新的、基于il2cpp的脚本热更新方案。它扩展了il2cpp虚拟机，以解释器的方式执行IL，相比于传统方案，可以做到aot和hotupdate代码几乎无感的融合，并且能够直接使用到C#和Unity的绝大部分特性和api，在开发效率上应该会有不错的提升。   
这个项目是在HybridCLR的基础上对于一些构建和游戏启动工作流的完善，也是对其的一次学习和尝试。   
HybridCLR的说明文档：https://focus-creative-games.github.io/hybridclr/about/#%E6%96%87%E6%A1%A3

## 功能
### Editor功能
提供了整合HybridCLR命令的功能，这些操作都集合在UniPurity/SettingWindow窗口里面，包括：
- 添加补充元数据的aot程序集   
- 自定义程序集的最终输出目录   
- 根据平台，基于hclr工具链对脚本进行构建   
- 基于构建得到的程序集，生成md5清单文件，并将程序集和清单文件输出到指定目录   

### Runtime功能
提供了一个可定制的dll加载器UniPurityPrepare，该加载器主要执行这些工作：   
- 通过对比清单文件，下载并更新本地热更新程序集   
- 加载指定的aot程序集，补充元数据   
- 加载热更新程序集
- 可以通过这个方法来完成加载器的任务：
```CSharp
using UniPurity;

private IEnumerator Init()
{
    //UniPurityPrepare的构造函数中，可以注入IPrepareConfig和IPrepareProxy的对象进行定制
    //IPrepareConfig用来指定dll清单以及dll文件的本地目录和下载地址
    //IPrepareProxy用来指定加载dll的方式
    using (var prepare = new UniPurityPrepare())
    {
        prepare.OnMsg += msg => Debug.Log($"message:{msg}");
        prepare.OnError += e => Debug.Log($"error:{e.ToString()}");
        prepare.OnProgress += (ref UniPurityPrepare.ProgressInfo pi) =>
            Debug.Log($"{pi.fileName}, {pi.cur}/{pi.groupTotal}");
        yield return StartCoroutine(prepare.PrepareDlls());
    }
    /// todo
}
```
加载器的实现位于同名的UniPurityPrepare.cs里面   
对于程序集的划分，并没有什么特别的要求，做好设置就可以了，文档里面也有相关描述   
#### 可能会出现的报错
- 在使用2021-1.0版本的HybridCLR的时候，调用HybridCLR-Generate-Il2CppDef命令的时候发生异常：region:PLACE_HOLDER start not find。   
这是因为一个模板文件的替换字符不对应所致，可以到 项目目录/HybridCLRData/LocalIl2Cpp-WindowsEditor/il2cpp/libil2cpp/hybridclr/Il2CppCompatibleDec.cpp中，将"!!!{{DHE"和"!!!}}DHE"中的DHE替换成PLACE_HOLDER，问题得以解决

## 安装
使用UniPurity需要先安装HybridCLR的工具套件   
可以通过gitee的url来安装：https://gitee.com/focus-creative-games/hybridclr_unity.git,   
也可以先clone下来，进行本地安装
再安装UniPurity工具，url为：https://gitee.com/diancongchen/UniPurity.git
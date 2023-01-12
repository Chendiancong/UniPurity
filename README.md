# UniPure
a unity engine's hot update framework using hybridclr
## HybridCLR文档
https://focus-creative-games.github.io/hybridclr/about/#%E6%96%87%E6%A1%A3   
https://github.com/focus-creative-games/inspect_hybridclr   
## 可能遇到的问题
1.HybridCLR/Generate/Il2CppDef抛出异常："Exception:region:PLACE_HOLDER start not find"，是由于安装后工程主目录/HybridCLRData/LocalIl2CppData-{Platform}/il2cpp/libil2cpp/hybridclr/Il2CppCompatibleDef.cpp中占位字符不匹配导致，原为：   
```cpp
const char* g_differentialHybridAssemblies[]
{
    //!!!{{DHE
    //!!!}}DHE
    nullptr
}
```
将其中DHE改成PLACE_HOLDER即可：   
```cpp
const char* g_differentialHybridAssemblies[]
{
    ///!!!{{PLACE_HOLDER
    ///!!!}}PLACE_HOLDER
}
```
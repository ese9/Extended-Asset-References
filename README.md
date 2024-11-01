# Extended Asset References 🎮

Extended Asset References is an open-source package that enhances Unity's Addressable Asset System functionality. This package introduces custom attributes designed to improve and streamline your workflow when dealing with AssetReferences.

**Compatible with:**
- ✅ [Tri Inspector](https://github.com/codewriter-packages/Tri-Inspector)
- ⏳ Odin Inspector (compatibility being verified)

## ✨ Features

- Custom attributes for enhanced AssetReference handling
- Improved workflow for Addressable asset management
- Easy integration with existing Unity projects
- Visual sprite preview and selection tools
- Automated atlas validation and problem-solving

## 📦 Installation

Install via Unity Package Manager using one of these Git URLs:

- Main branch:
```
https://github.com/ese9/Extended-Asset-References.git
```

- Specific version:
```
https://github.com/ese9/Extended-Asset-References.git#v1.0.0
```

## 🚀 Usage

Once installed, enhance your Unity scripts with our custom attributes for better asset reference management.

### Asset Reference Sprite

#### FromAtlas
```csharp
using Nine.AssetReferences;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Resource : ScriptableObject
{
    [FromAtlas]
    public AssetReferenceSprite iconRef;
}
```

#### FromSprite
```csharp
using Nine.AssetReferences;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Resource : ScriptableObject
{
    [FromSprite]
    public AssetReferenceSprite iconRef;
}
```

Example of selecting a sprite when using any of the attributes

![demo_1](https://github.com/user-attachments/assets/be50a362-9196-4184-81ed-0350ee5086c7)

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/ese9/Extended-Asset-References/blob/main/LICENSE.md) file for details.

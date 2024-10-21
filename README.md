# Extended Asset References

Extended Asset References is an open source package that provides additional functionality for working with Unity's Addressable Asset System. This package introduces custom attributes designed to improve and streamline your workflow when dealing with AssetReferences.

**Compatible with:** [Tri Inspector](https://github.com/codewriter-packages/Tri-Inspector) ~& Odin Inspector~ (checking)

## Features

- Custom attributes for enhanced AssetReference handling
- Improved workflow for Addressable asset management
- Easy integration with existing Unity projects

## Installation

**Package Manager from git URL...**
* main branch `https://github.com/ese9/Extended-Asset-References.git`
* or tag with version `https://github.com/ese9/Extended-Asset-References.git#v1.0.0`

## Usage
Once installed, you can use the new custom attributes in your Unity scripts to enhance the way you reference and manage assets. Here's a simple example:

### From Atlas

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

**The attribute will help solve problems related to:**
* Visualization of the selected sprite

|  Before | After  |
|:---:|:---:|
| ![image](https://github.com/user-attachments/assets/c1fc0eea-5792-4dcb-b966-e69f04f09a87)  | ![image](https://github.com/user-attachments/assets/c1ad8771-d63c-4b5e-addd-786f4025031e) |

* Visualization sprite picking process

|  Before | After  |
|:---:|:---:|
| ![image](https://github.com/user-attachments/assets/0fd0c20c-7624-4b70-bb5a-30bf10420c92)  | ![image](https://github.com/user-attachments/assets/5e2edb62-f085-44b1-9fe3-67ff60d0534e)  |

* Check validity of AssetSpriteReference and possibility to fix the problem by adding atlas to default asset group

|  Before | After  |
|:---:|:---:|
| ![image](https://github.com/user-attachments/assets/75fcdfa7-cb78-41a3-9e43-d56cf1d8c7b1)  | ![image](https://github.com/user-attachments/assets/f4ab71f4-71ad-44cc-a950-6866a19081b3)  |

* Check if the sprite is in the atlas and search for a new atlas if the sprite has been moved

|  Before | After  |
|:---:|:---:|
| ![image](https://github.com/user-attachments/assets/ee245edb-473c-4dbf-a64f-a18c9d290eb7)  | ![image](https://github.com/user-attachments/assets/903972bc-84e1-48bd-882a-6e7163155dcf)  |

## License
This project is licensed under the MIT License. See the [LICENSE](https://github.com/ese9/Extended-Asset-References/blob/main/LICENSE.md) file for more details.


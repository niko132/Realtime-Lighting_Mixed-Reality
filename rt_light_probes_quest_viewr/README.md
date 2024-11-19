## Requirements
- Git credential manager
- Unity 2021.3.8f1 URP
- Android for the specific Unity version

## Packages included:
- ViewR-Core:  https://dev.ixlab.inf.tu-dresden.de/lab/multiuservrfoundations/viewr_core.git?path=/Assets/ViewR
- FoyerAPB: https://dev.ixlab.inf.tu-dresden.de/lab/ixmodels/foyerapb_content.git?path=/Assets/FoyerAPB_Core
- Kulturpalast: https://dev.ixlab.inf.tu-dresden.de/lab/ixmodels/kulturpalast_content.git?path=/Assets/Kulturpalast_Core

## How to:

### Add scene

- Add scene to scene experience chooser insite start scene
- Make sure that scene is added to build
- Add "DisplayController" and "AddPassthroughMaterialSwapperToChildren" script to scene geometry (keep content outside of this)

### Normcore

- Create Normcore account
- Create AppID
- Copy it into "NormcoreAppSettings" as "App Key" inside ViewR

## Troubleshooting

Wrong lightmaps in android build
- reimport skybox texture

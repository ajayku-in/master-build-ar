# Master Build'AR
A small application demonstrating the use of AR for displaying interactive Lego instructions.

Reccomended Unity version **2019.3.10+**

To try this project in AR check out the repository and switch to the Android  or iOS (untested) platform and hit build and run with your device connected.

## Adding another model to the projcet

* Download the mdl file for the model from your repository of choice and place it under **LDrawFiles\blueprints\models**. Then when you open Unity you can use the **LDrawImporter** menu to bring in your model following their documentation linked below
* Update the **Step** GameObjects in the created model to specify animations or step numbers other than the default ones
* Use the PowerShell script **InstructionFiles\GetInstructions.ps1** to download instructions by specifying **-SetId** and **-NumPages** (So long as they are present on https://lego.brickinstructions.com)
* Add a new **Reference Image library** with the instruction pages include in it and named according to their page number
* Update the **ARTrackedImageManager** on the **ARSessionOrigin** GameObject with your new Reference Image Library
* Update the **TrackedInstructionManager** on the **ARSessionOrigin** with the **Model Prefab/Offset/Rotation/Scale** for the model you have imported in the Inspector
* Update **TrackedInstructionManager.cs** with the Page to Step number mappings for your chosen instructions / model  

## References

https://github.com/Unity-Technologies/arfoundation-samples

https://github.com/arjundube/LDraw_Importer_Unity

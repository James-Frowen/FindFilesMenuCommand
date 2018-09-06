# FindFilesMenuCommand
Extension for Visual Studio that searches folder and adds matching files to projects

### Install 

1. Build the project
2. Run `FindFilesMenuCommand.vsix` located in the `bin\Debug` folder

### Update

1. Increase version in `source.extension.vsixmanifest`
2. Rebuild 
3. Run `FindFilesMenuCommand.vsix`

[More Info](https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-update-a-visual-studio-extension?view=vs-2017)

# How to use

A `Find matching files for projects` button will be in the tool menu.

The extension creates a `AutoFindFiles.xml` file if one does not exist or failed to load. The file will contain XML for the current projects. The default XML is below. 

Update the folders and matches for the files you want to add, then set enabled to true.

Run the extension again to add files to projects.

```xml
<Project>
  <enabled>false</enabled>
  <name>{ProjectName}</name>
  <folders>
    <folder>./</folder>
  </folders>
  <matches>
    <match>*.cs</match>
  </matches>
</Project>
```
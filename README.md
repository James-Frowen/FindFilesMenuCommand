# FindFilesMenuCommand
visual studio addon that searches folder and adds matching files to projects

### Install 

Build the project then run `FindFilesMenuCommand.vsix` that should be located in the `bin\Debug` folder

### Update

Increase version in `source.extension.vsixmanifest` then build and run `FindFilesMenuCommand.vsix`

[More Info](https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-update-a-visual-studio-extension?view=vs-2017)

# How to use

By pressing the `Find matching files for projects` button in the tool menu a `AutoFindFiles.xml` file should be created with the current projects. The default xml is below. 

Update the folders and matches for the files you want to add, then set enabled to true.

After you have updated `AutoFindFiles.xml` then press the `Find matching files for projects` button again and the extension will run and add files

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
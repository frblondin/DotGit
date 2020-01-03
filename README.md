[![NuGet Badge](https://buildstats.info/nuget/DotGit)](https://www.nuget.org/packages/DotGit/)
[![Build Status](https://dev.azure.com/thomas0449/GitHub/_apis/build/status/frblondin.DotGit?branchName=master)](https://dev.azure.com/thomas0449/GitHub/_build/latest?definitionId=4&branchName=master)

DotGit is a fully managed .Net Standard library that reads Git data using. This library allows you to read commits, tree entries, and files.

```csharp
var reader = new RepositoryReader(pathToRepo);
var commit = reader.Read<Commit>("c462e3f3024c8cabc252ed5d309922ed06a492b9");
```

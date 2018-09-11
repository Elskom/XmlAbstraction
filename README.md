# XmlAbstraction

[![NuGet Badge](https://buildstats.info/nuget/XmlAbstraction?includePreReleases=true)](https://www.nuget.org/packages/XmlAbstraction/)

A library that contains a System.Xml and System.Xml.Linq abstraction class.

# TODOs:

- Add functions to remove XML Entries and Attributes too.
- Finish Read(string elementname, string attributename) and Write(string elementname, string attributename, object attributevalue) shortcut methods.
- Add ways of adding, editing, and deleting elements within elements.
- Add element to pending ElementsEdits dictionary. (AddAttribute)
- Add Subelements to pending changes list. (Write(string, string, string[]))
- Add edited Subelements to pending changes list. Then on save overwrite the whole collection with the pending data from here. (Write(string, string, string[]))

Contributions:

Any contributions that helps on the TODO's optimizes the code, or just flat out new feature requests are welcome.
Also be sure to frequently check the [contributing](.github/CONTRIBUTING.md) file.

License:

MIT

Code of Conduct:

See our Code of Conduct [here](CODE_OF_CONDUCT.md).

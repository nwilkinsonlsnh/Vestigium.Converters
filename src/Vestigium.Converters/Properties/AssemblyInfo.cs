using System.Windows;
using System.Windows.Markup;
using Vestigium.Converters;

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

[assembly: XmlnsDefinition(Xmlns.Uri, "Vestigium.Converters.Xaml")]
[assembly: XmlnsDefinition(Xmlns.Uri, "Vestigium.Converters.Converters.Boolean")]
[assembly: XmlnsDefinition(Xmlns.Uri, "Vestigium.Converters.Converters.Formatting")]
[assembly: XmlnsDefinition(Xmlns.Uri, "Vestigium.Converters.Converters.Numeric")]
[assembly: XmlnsDefinition(Xmlns.Uri, "Vestigium.Converters.Converters.Collections")]
[assembly: XmlnsDefinition(Xmlns.Uri, "Vestigium.Converters.Converters.Domain")]
[assembly: XmlnsPrefix(Xmlns.Uri, "v")]

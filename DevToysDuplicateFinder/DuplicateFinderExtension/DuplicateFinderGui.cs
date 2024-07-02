using DevToys.Api;
using static DevToys.Api.GUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderExtension
{
    [Export(typeof(IGuiTool))]
    [Name("DuplicateFinder")]
    [ToolDisplayInformation(
        IconFontName = "FluentSystemIcon", 
        IconGlyph = '\uE670', 
        GroupName = PredefinedCommonToolGroupNames.Text,
        ResourceManagerAssemblyIdentifier = nameof(ResourceAssemblyIdentifier),
        ResourceManagerBaseName = "DuplicateFinderExtension.DuplicateFinderExtension",
        ShortDisplayTitleResourceName = nameof(DuplicateFinderExtension.ShortDisplayTitle),
        LongDisplayTitleResourceName = nameof(DuplicateFinderExtension.LongDisplayTitle),
        DescriptionResourceName = nameof(DuplicateFinderExtension.Description),
        AccessibleNameResourceName = nameof(DuplicateFinderExtension.AccessibleName)
    )]
    internal sealed class DuplicateFinderGui : IGuiTool
    {
        public UIToolView View
            //=> new(Label().Style(UILabelStyle.BodyStrong).Text(DuplicateFinderExtension.HelloWorldLabel));
            => new(
                Stack()
                .Vertical()
                .WithChildren(
                    MultiLineTextInput().Title("Input").Extendable().OnTextChanged(OnInputTextChanged),
                    MultiLineTextInput().Title("Found duplicates").ReadOnly().HideCommandBar().NeverShowLineNumber())
                );

        public void OnDataReceived(string dataTypeName, object? parsedData)
        {
            throw new NotImplementedException();
        }

        private void OnInputTextChanged(string text)
        {

        }
    }
}

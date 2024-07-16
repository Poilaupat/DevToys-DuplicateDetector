using DevToys.Api;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace DuplicateDetectorExtension
{
    [Export(typeof(IResourceAssemblyIdentifier))]
    [Name(nameof(ResourceAssemblyIdentifier))]
    internal sealed class ResourceAssemblyIdentifier : IResourceAssemblyIdentifier
    {
        [DebuggerHidden]
        public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
        {
            throw new NotImplementedException();
        }
    }
}

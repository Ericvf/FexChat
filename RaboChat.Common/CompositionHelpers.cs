using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace RaboChat.Common
{
    public class CompositionHelpers
    {
        public static void InitializeComposition(IEnumerable<Type> assemblyTypes, object instance)
        {
            var catalog = new AggregateCatalog();

            foreach (var assembly in assemblyTypes)
            {
                catalog.Catalogs.Add(new AssemblyCatalog(assembly.Assembly));
            }

            var container = new CompositionContainer(catalog);
            container.ComposeParts(instance);
        }
    }
}

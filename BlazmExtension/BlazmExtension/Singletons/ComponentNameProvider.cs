using System.Collections.Generic;
using System.Linq;

namespace BlazmExtension.Singletons
{
    public static class ComponentNameProvider
    {
        private static List<string> ComponentNames { get; set; } = new List<string>();

        public static void SetComponentNames(IEnumerable<string> componentNames)
        {
            if (componentNames != null)
            {
                ComponentNames = componentNames.ToList();
            }
        }
        public static IEnumerable<string> GetComponentNames()
        {
            return ComponentNames;
        }
    }
}

using System.Runtime.InteropServices;

namespace BlazmExtension.Dialogs.ComponentReferences
{
    [Guid("9d25178e-8686-452e-8491-7be334532c87")]
    public class ComponentReferencesWindow : ToolWindowPane
    {
        public ComponentReferencesWindow() : base(null)
        {
            this.Caption = "Component References";
            this.Content = new ComponentReferencesControl();
        }
    }
}

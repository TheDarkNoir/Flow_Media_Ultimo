using System.Diagnostics;

namespace FlowMediaWebMVC
{
    [DebuggerDisplay("Usuario: {nombre_usuario,nq}")]
    public partial class Usuario
    {
        // partial class used only to control debugger display and avoid deep traversal causing StackOverflow
    }
}

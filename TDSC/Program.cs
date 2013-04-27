#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace TheyDontStopComing {
#if WINDOWS || LINUX
    public static class Program {
        [STAThread]
        static void Main() {
            using(var game = new TDSC())
                game.Run();
        }
    }
#endif
}

using log4net;
using Terraria;
using Terraria.ModLoader;

namespace OptimizedCursor;

public class OptimizedCursor : Mod
{
    // I hate having to parse around the ILog object everywhere I want to log
    public static ILog LoggerInstance { get; private set; }
    
    public override void Load()
    {
        LoggerInstance = Logger;
    }

    public override void Unload()
    {
        LoggerInstance = null;
    }
}
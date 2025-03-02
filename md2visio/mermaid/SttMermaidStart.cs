﻿using md2visio.mermaid.@internal;

namespace md2visio.mermaid
{
    internal class SttMermaidStart : SynState
    {
        public override SynState NextState()
        {
            return Run(Ctx);
        }

        public static SynState Run(SynContext ctx)
        {
            if (!ctx.Until(@"^\s*(?<bquote>`{3,})\s*mermaid\s*(?=\n)")) 
                return EndOfFile;

            SynState state = new SttMermaidStart();
            state.Ctx = ctx;
            state.Fragment = ctx.MatchedGroups["bquote"].Value;
            ctx.AddState(state);
            return state.Forward<SttChar>();
        }
    }
}

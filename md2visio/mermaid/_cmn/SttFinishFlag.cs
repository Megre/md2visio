﻿using md2visio.mermaid.graph.@internal;

namespace md2visio.mermaid._cmn
{
    internal class SttFinishFlag : SynState
    {
        public override SynState NextState()
        {
            // \n or ;
            return Save(Take().Buffer).ClearBufer().Forward<SttCtxChar>();
        }
    }
}

﻿using md2visio.mermaid._cmn;
using System.Text.RegularExpressions;

namespace md2visio.mermaid.journey
{
    internal class JoSttKeyword : SynState
    {
        public override SynState NextState()
        {
            //if (!IsKeyword(Ctx)) throw new SynException($"unknown keyword '{Buffer}'", Ctx);

            Save(Buffer).ClearBufer();
            if (JoSttKeywordParam.HasParam(Ctx)) return Forward<JoSttKeywordParam>();
            else return Forward<JoSttChar>();
        }

        public static bool IsKeyword(SynContext ctx)
        {
            return Regex.IsMatch(ctx.Cache.ToString(), "^(journey|title|section)$");
        }
    }
}

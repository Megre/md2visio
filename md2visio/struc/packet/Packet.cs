﻿using md2visio.struc.figure;
using md2visio.vsdx;

namespace md2visio.struc.packet
{
    internal class Packet : Figure
    {
        public string Title { get; set; } = string.Empty;

        public override void ToVisio(string path)
        {
            new VBuilderPac(this).Build(path);
        }
    }
}

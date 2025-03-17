﻿using md2visio.struc.figure;
using md2visio.struc.packet;
using Microsoft.Office.Interop.Visio;
using System.Drawing;
using System.Net.Sockets;

namespace md2visio.vsdx
{
    internal class VShapeDrawerPac : VShapeDrawer
    {
        int lineIndex = 0;
        int bitNumInLine = 0;
        VBoundary drawBound = new VBoundary();
        List<DrawnLine> drawnLines = new List<DrawnLine>();
        double hSpaceMM = 1, vSpaceMM = 5; // mm
        SizeF minHeight = SizeF.Empty, oneBitSize = SizeF.Empty;
        Dictionary<int, Shape> alignPoints = new Dictionary<int, Shape>();

        public VShapeDrawerPac(Application visioApp) : base(visioApp)
        {
        }

        public void DrawPacket(Packet packet, List<INode> sortedNodes)
        {
            minHeight = MinHeight(packet);

            // prepare
            for(int i=0; i<sortedNodes.Count; ++i)
            {
                PacBlock block = (PacBlock) sortedNodes[i];
                DrawBlock(sortedNodes, i, block.BitNum);
            }

            // resize blocks
            oneBitSize = OneBitSize(packet);
            (DrawnLine? line, VBoundary lineBound) = GrowSize();
            if (line==null || lineBound.IsEmpty()) return;

            foreach(DrawnLine L in drawnLines)
            {
                if (L == line) continue;

                double defWidth = DefaultLineWidth(L);
                VBoundary vbound = GrowLine(L);
                lineBound.Expand(vbound);
                
                if(L == drawnLines.Last()) DrawTitle(packet, lineBound);
            }
        }

        void DrawTitle(Packet packet, VBoundary lineBound)
        {
            string? title = packet.FrontMatter.GetString("title");
            if (!string.IsNullOrEmpty(title))
            {
                Shape shape = DropText(title, lineBound.PinX, lineBound.PinY);
                AlignTop(shape, lineBound.Bottom - MM2VisioUnit(shape));
            }
        }

        double DefaultLineWidth(DrawnLine line)
        {
            double width = 0;
            foreach(INode node in line.Nodes)
            {
                if (node.VisioShape != null && width != 0)
                    width += MM2VisioUnit(node.VisioShape) * hSpaceMM;

                PacBlock block = (PacBlock)(node);
                width += oneBitSize.Width * block.BitNum; 
            }

            return width;
        }

        void DrawBlock(List<INode> sortedNodes, int index, int bitNum)
        {
            PacBlock block = (PacBlock)sortedNodes[index];
            int bitNum2Draw = BitNum2Draw(bitNum);

            DrawnLine line = CurLine();
            Shape shape = DropShape(block);
            line.AddNode(block);
            
            bitNumInLine += bitNum2Draw;
            if(bitNumInLine == 32)
            {
                lineIndex++;
                bitNumInLine = 0;
            }

            if(bitNum2Draw < bitNum)
            {
                PacBlock newBlock = block.Split(bitNum2Draw);
                sortedNodes.Insert(index + 1, newBlock);
            }
        }

        int BitNum2Draw(int bitNum)
        {
            if(bitNumInLine + bitNum <= 32) return bitNum;
            return 32 - bitNumInLine;
        }

        DrawnLine CurLine()
        {
            if(bitNumInLine > 0) return drawnLines.Last();

            DrawnLine line = new DrawnLine();
            drawnLines.Add(line);
            return line;
        }

        Shape DropShape(PacBlock block)
        {
            Shape shape = visioPage.Drop(GetMaster("[]"), 0, 0);
            SetShapeSheet(shape, "LeftMargin", "0");
            SetShapeSheet(shape, "RightMargin", "0");
            shape.Text = block.Label;
            AdjustSize(shape);
            SetFillForegnd(shape, new Tuple<int, int, int>(239, 239, 239));
            if(Height(shape) < minHeight.Height) 
                SetHeight(shape, $"{minHeight.Height}");

            if (bitNumInLine == 0)
            {
                drawBound.AlignLeft(0);
                drawBound.AlignTop(-(MM2VisioUnit(shape) * vSpaceMM + minHeight.Height) * lineIndex);
                drawBound.Right = drawBound.Left + Width(shape);
            }
            else
            {
                drawBound.AlignLeft(drawBound.Right + MM2VisioUnit(shape) * hSpaceMM);
                drawBound.Right = drawBound.Left + Width(shape);
            }

            AlignLeftTop(shape, drawBound.Left, drawBound.Top);            

            return block.VisioShape = shape;
        }

        SizeF OneBitSize(Packet packet)
        {
            SizeF oneBitSize = new Size(0, 0);
            Shape shape = visioPage.Drop(GetMaster("[]"), 0, 0);
            string fontName = FontName(visioApp, shape);
            double fontSize = FontSize(shape);

            foreach (DrawnLine line in drawnLines)
            {
                foreach (INode node in line.Nodes)
                {
                    PacBlock block = (PacBlock)node;

                    if (node.VisioShape == null) continue;
                    oneBitSize.Width = (float) Math.Max(oneBitSize.Width, 
                        Width(node.VisioShape) / block.BitNum);
                    oneBitSize.Height = (float) Math.Max(oneBitSize.Height, Height(node.VisioShape));
                }
            }            
            
            shape.Delete();
            return oneBitSize;
        }

        SizeF MinHeight(Packet packet)
        {
            SizeF minHeight = new Size(0, 0);
            Shape shape = visioPage.Drop(GetMaster("[]"), 0, 0);
            string fontName = FontName(visioApp, shape);
            double fontSize = FontSize(shape);

            foreach (INode node in packet.InnerNodes.Values)
            {
                PacBlock block = (PacBlock)node;
                SizeF size = MeasureTextSizeMM(block.Label, fontName, fontSize);

                minHeight.Width = Math.Max(minHeight.Width,
                    (size.Width + (float)LeftMargin(shape) + (float)RightMargin(shape)) / block.Label.Length);
                minHeight.Height = Math.Max(minHeight.Height,
                    size.Height + (float)TopMargin(shape) + (float)BottomMargin(shape));                
            }

            shape.Delete();
            return minHeight;
        }

        (DrawnLine?, VBoundary) GrowSize()
        {
            int maxBlockNumInLine = 0;
            DrawnLine? line = null;
            foreach (DrawnLine dLine in drawnLines)
            {
                if(dLine.Nodes.Count > maxBlockNumInLine)
                {
                    line = dLine; 
                    maxBlockNumInLine = dLine.Nodes.Count;
                }
            }

            return (line, GrowLine(line));
        }
        VBoundary GrowLine(DrawnLine? line)
        {
            if (line == null) return VBoundary.Empty;

            VBoundary bound = new VBoundary();
            VBoundary lineBound = new VBoundary();
            int bitIndex = 0;
            foreach (INode node in line.Nodes)
            {
                Shape? shape = node.VisioShape;                
                if(shape == null) continue;

                PacBlock block = (PacBlock) node;
                int drawnBitNum = block.BitNum; 
                if (bitIndex == 0) bound.AlignLeft(0);
                else if (alignPoints.ContainsKey(bitIndex)) // left
                {
                    bound.AlignLeft(new VShapeBoundary(alignPoints[bitIndex]).Left);
                }
                else
                    bound.AlignLeft(bound.Right + MM2VisioUnit(shape) * hSpaceMM);

                if (alignPoints.ContainsKey(bitIndex + drawnBitNum - 1)) // right
                    bound.Right = new VShapeBoundary(alignPoints[bitIndex + drawnBitNum - 1]).Right;
                else
                    bound.Right = bound.Left + oneBitSize.Width * drawnBitNum +
                        LeftMargin(shape) + RightMargin(shape);

                SetWidth(shape, bound.Width);
                AlignLeft(shape, bound.Left);
                DrawBitIndex(block);

                Add2AlignPoints(line, shape, bitIndex, drawnBitNum);
                lineBound.Expand(shape);

                bitIndex += drawnBitNum;
            }

            return lineBound;
        }

        void DrawBitIndex(PacBlock block)
        {
            if(block.VisioShape == null) return;

            VBoundary bound = new VShapeBoundary(block.VisioShape);
            Shape text = DropBitIndexText(block.BitStart);

            double bottom = bound.Top + MM2VisioUnit(text)*0.5;
            if (block.BitNum == 1)
            {
                MoveTo(text, bound.PinX, bound.PinY);
                AlignBottom(text, bottom);
            }
            else
            {
                visioApp.DoCmd((short)VisUICmds.visCmdTextHAlignLeft);
                AlignLeft(text, bound.Left);
                AlignBottom(text, bottom);

                text = DropBitIndexText(block.BitEnd);
                visioApp.DoCmd((short)VisUICmds.visCmdTextHAlignRight);
                AlignRight(text, bound.Right);
                AlignBottom(text, bottom);
            }
        }

        Shape DropBitIndexText(int bitIndex)
        {
            Shape text = DropText(bitIndex.ToString(), 0, 0);
            SetShapeSheet(text, "LeftMargin", "0");
            SetShapeSheet(text, "RightMargin", "0");
            SetShapeSheet(text, "TopMargin", "0");
            SetShapeSheet(text, "BottomMargin", "0");
            SetShapeSheet(text, "Char.Size", $"{FontSize(text) * 0.7}");
            visioApp.DoCmd((short)VisUICmds.visCmdTextVAlignBottom);
            return text;
        }

        void Add2AlignPoints(DrawnLine line, Shape shape, int bitIndex, int bitNum)
        {
            if (!alignPoints.ContainsKey(bitIndex))
            {
                alignPoints[bitIndex] = shape;
            }
            if(!alignPoints.ContainsKey(bitIndex + bitNum - 1))
            {
                alignPoints[bitIndex + bitNum - 1] = shape;
            }
        }
    } // VShapeDrawerPac


    class DrawnLine
    {
        List<INode> nodes = new List<INode>();
        public List<INode> Nodes { get => nodes; }

        public DrawnLine() { 
        }
        
        public void AddNode(INode node)
        {
            nodes.Add(node); 
        }
    }
}

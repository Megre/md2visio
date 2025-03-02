﻿using md2visio.figure;
using md2visio.vsdx;
using Microsoft.Office.Interop.Visio;
using System.Drawing;
using System.Text.RegularExpressions;

namespace md2visio.graph
{
    internal class GNode: INode, IEmpty
    {
        public static double SPACE = 0.3;
        public static double PADDING = 0.15;
        public static GNode Empty = new GNode();

        protected string id = string.Empty;

        List<GEdge> outputEdges = new List<GEdge>();
        List<GEdge> inputEdges  = new List<GEdge>();
        List<GNode> outputNodes = new List<GNode>();
        List<GNode> inputNodes  = new List<GNode>();
        GNodeShape  nodeShape   = new GNodeShape();

        public GNode() : this(Graph.Empty, string.Empty) { }

        public GNode(Container container, string id)
        {
            Container = container;
            ID = id;
        }

        public Shape? VisioShape {  get; set; }
        public GNodeShape NodeShape { 
            get => nodeShape; 
            set { 
                nodeShape = value;
            } 
        }
        public List<GEdge> OutputEdges { get { return outputEdges; } }
        public List<GEdge> InputEdges { get { return inputEdges; } }
        public string ID
        {
            get { return id; }
            set
            {
                id = value;
                if (Label.Length == 0) Label = id;
            }
        }

        public string Label
        {
            get { return nodeShape.Label; }
            set { nodeShape.Label = value; }
        }

        public string ShapeStart
        {
            get { return nodeShape.Start; }
        }

        public string ShapeClose
        {
            get { return nodeShape.Close; }
        }

        public Container Container { get; set; }            

        public PointF ShiftPos(GNode fixedNode, RelativePos rpos)
        {
            GNode node = this;
            PointF pos = new PointF(0, 0);
            Shape? shape = node.VisioShape, fixedShape = fixedNode.VisioShape;

            if (shape != null && fixedShape != null)
            {
                double w = VShapeFactory.ShapeSheetIU(fixedShape, "Width");
                double h = VShapeFactory.ShapeSheetIU(fixedShape, "Height");
                double x = VShapeFactory.ShapeSheetIU(fixedShape, "PinX");
                double y = VShapeFactory.ShapeSheetIU(fixedShape, "PinY");
                int reverse = (rpos == RelativePos.FRONT ? -1 : 1);
                
                double sw = VShapeFactory.ShapeSheetIU(shape, "Width");
                double sh = VShapeFactory.ShapeSheetIU(shape, "Height");
                GGrowthDirection grow = node.Container.GrowDirect;
                if (grow.H == 0)
                {
                    pos.X = (float) x; // 水平居中对齐
                    pos.Y = (float)(y + grow.V*reverse*(h/2 + sh/2 + SPACE));
                }
                else if (grow.V == 0) {
                    pos.Y = (float)y; // 垂直居中对齐
                    if (grow.H > 0) pos.X = (float)(x + grow.H*reverse*(w/2 + sw/2 + SPACE));
                }
            }
            return pos;
        }
        public void AddOutEdge(GEdge edge)
        {
            outputEdges.Add(edge);
        }

        public void AddInEdge(GEdge edge)
        {
            inputEdges.Add(edge);
        }

        public List<GNode> OutputNodes()
        {
            if (outputEdges.Count == 0) return outputNodes;
            if (outputNodes.Count == 0)
            {
                foreach (GEdge edge in outputEdges) outputNodes.Add(edge.To);
            }
            return outputNodes; 
        }

        public List<GNode> InputNodes()
        {
            if (InputEdges.Count == 0) return inputNodes;
            if (inputNodes.Count == 0)
            {
                foreach (GEdge edge in InputEdges) inputNodes.Add(edge.From);
            }
            return inputNodes;
        }

        public bool IsEmpty() { return this == Empty; }

        static public bool IsShapeFragment(string fragment)
        {
            return IsShapeStartFragment(fragment) ||
                IsShapeCloseFragment(fragment);
        }

        static public bool IsShapeStartFragment(string fragment)
        {
            return Regex.IsMatch(fragment, @"^(>|\[{1,2}|\{{1,2}|\({1,3}|\[\(|\[\\|\[/|\(\[)$");
        }

        static public bool IsShapeCloseFragment(string fragment)
        {
            return Regex.IsMatch(fragment, @"^(\]{1,2}|\}{1,2}|\){1,3}|\)\]|\\\]|/\]|\]\))$");
        }

        static public string ShapeCloseFragmentPattern(string startFragment)
        {
            switch (startFragment)
            {
                case "[[": return @"\]\]";               
                case "[":  return @"\]";
                case "{{": return @"\}\}";
                case "{":  return @"\}";
                case "(((": return @"\)\)\)";
                case "((":  return @"\)\)";
                case "(":   return @"\)";
                case ">":  return @"\]";
                case "[(": return @"\)\]";
                case @"[\":
                case @"[/": return @"[\\/]]";
                case "([":  return @"\]\)";
            }
            return "]";
        }

        public override string ToString()
        {
            return $"{nodeShape.Start}{id}{nodeShape.Close}";
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj == null || GetType() != obj.GetType()) return false;

            return id == ((GNode) obj)?.id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (id != null ? id.GetHashCode() : 0);
                hash = hash * 23 + (id != null ? id.GetHashCode() : 0);
                return hash;
            }
        }
    }
}

﻿using md2visio.figure;
using md2visio.graph;
using Microsoft.Office.Interop.Visio;

namespace md2visio.journey
{
    internal class JoSection: INode, IEmpty
    {
        public static readonly JoSection Empty = new JoSection();
        string Title {  get; set; } = string.Empty;
        public string ID { get; set; } = string.Empty;
        public string Label { get => ID; set => ID = value; }
        public Shape? VisioShape { get; set; }
        public Container Container { get; set; } = Journey.Empty;
        public List<JoTask> Tasks { get => joTasks; }

        public List<GEdge> InputEdges => throw new NotImplementedException();

        public List<GEdge> OutputEdges => throw new NotImplementedException();

        List<JoTask> joTasks = new List<JoTask>();

        public JoSection() { }
        public JoSection(string title) { Title = title.Trim(); }

        public void AddTask(JoTask joTask)
        {
            joTask.Section = this;
            joTasks.Add(joTask);
        }

        public HashSet<string> JoinerSet()
        {
            HashSet<string> set = new HashSet<string>();
            foreach(JoTask joTask in joTasks)
            {
                foreach (string joiner in joTask.Joiners) 
                    set.Add(joiner);
            }
            return set;
        }

        public bool IsEmpty()
        {
            return this == Empty;
        }
    }
}

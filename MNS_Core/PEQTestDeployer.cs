﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MNS_GraphicsLib;
using Location;

namespace ModularNetworkSimulator
{
    public class PEQTestDeployer : IDeployer
    { // RandomTestDeployer sets a certain number of nodes randomly about the field (assuring no overlaps)
      // Node 0 (Sink) is set at Xmax and (Ymax-Ymin)/2
      // Nodes 1-5 (Sources) are set at Xmin and (n-1)*(Ymax-Ymin)/4

        static List<PanelObj> setupPanel()
        {
            List<PanelObj> list = new List<PanelObj>();
            PanelObj obj = new PanelObj();
            obj.name = "NumNodesLabel";
            obj.type = FormType.label;
            obj.text = "# Nodes";
            obj.width = 58;
            obj.xSlot = 0;
            obj.ySlot = 0;
            list.Add(obj);

            obj = new PanelObj();
            obj.name = "NumNodes";
            obj.type = FormType.intBox;
            obj.value = "100";
            obj.width = 106;
            obj.xSlot = 1;
            obj.ySlot = 0;
            list.Add(obj);

            obj = new PanelObj();
            obj.name = "PaddingLabel";
            obj.type = FormType.label;
            obj.text = "Edge Padding (m)";
            obj.width = 58;
            obj.xSlot = 0;
            obj.ySlot = 1;
            list.Add(obj);

            obj = new PanelObj();
            obj.name = "Padding";
            obj.type = FormType.doubleBox;
            obj.value = "0";
            obj.width = 106;
            obj.xSlot = 1;
            obj.ySlot = 1;
            list.Add(obj);

            obj = new PanelObj();
            obj.name = "MinDistanceLabel";
            obj.type = FormType.label;
            obj.text = "Min Node Distance (m)";
            obj.width = 58;
            obj.xSlot = 0;
            obj.ySlot = 2;
            list.Add(obj);

            obj = new PanelObj();
            obj.name = "MinDistance";
            obj.type = FormType.doubleBox;
            obj.value = "100";
            obj.width = 106;
            obj.xSlot = 1;
            obj.ySlot = 2;
            list.Add(obj);

            return list;
        }
        public List<PanelObj> SetupPanel
        { get { return setupPanel(); } }

        public List<PanelObj> panelObjs;
        public List<PanelObj> PanelObjs
        {
            get { return panelObjs; }
            set { panelObjs = value; }
        }
        PanelObjHelper panelObjsHelper;

        int numNodes = 25;
        double padding = 100;
        double minDistance = 10;

        bool isInitialized;
        public bool IsInitialized
        {
            get { return isInitialized; }
        }

        INodes nodes;
        public INodes Nodes
        {
            get { return nodes; }
        }

        INodeFactory nodeFactory;
        public INodeFactory NodeFactory
        {
            get { return nodeFactory; }
        }

        XYDoubleLocation[] field = new XYDoubleLocation[2];
        public ILocation[] Field
        {
            get { return field; }
        }

        IRandomizerFactory randFactory;
        public IRandomizerFactory RandFactory { set { randFactory = value; } }
        IRandomizer rand;

        public PEQTestDeployer()
        {
            isInitialized = false;
        }

        public PEQTestDeployer(INodes nodes, INodeFactory nodeFactory, ILocation[] field,
            IRandomizerFactory randFactory)
        {
            Initialize(nodes, nodeFactory, field, randFactory);
        }

        public void Initialize(INodes nodes, INodeFactory nodeFactory, ILocation[] field,
            IRandomizerFactory randFactory)
        {
            this.nodes = nodes;
            this.nodeFactory = nodeFactory;
            this.randFactory = randFactory;
            if (field is XYDoubleLocation[])
                this.field = (XYDoubleLocation[])field;
            else
                throw new InvalidCastException("RandomDeployer must take XYDoubleLocation field");
            panelObjsHelper = new PanelObjHelper(panelObjs);
            numNodes = panelObjsHelper.GetIntByName("NumNodes");
            padding = panelObjsHelper.GetDoubleByName("Padding");
            minDistance = panelObjsHelper.GetDoubleByName("MinDistance");
            isInitialized = true;
        }

        public void Deploy()
        {
            if (!isInitialized)
                throw new InvalidOperationException("RandomDeployer not initialized!");

            List<XYDoubleLocation> pointList = new List<XYDoubleLocation>();

            rand = randFactory.CreateRandomizer();

            bool continueFlag;
            XYDoubleLocation initial, final, current;
            initial = (XYDoubleLocation)field[0];
            final = (XYDoubleLocation)field[1];
            current = new XYDoubleLocation();

            // Sink
            current = new XYDoubleLocation(final.X, (final.Y - initial.Y)/2);
            current.SetField(field);
            nodes.AddNode(nodeFactory.CreateNode(current));
            pointList.Add(current);
            nodes.GetNodeByID(0).IsSink = true;

            // Sources
            for (int i = 1; i < 6; i++)
            {
                current = new XYDoubleLocation(initial.X, (i-1)*(final.Y-initial.Y)/4);
                current.SetField(field);
                nodes.AddNode(nodeFactory.CreateNode(current));
                pointList.Add(current);
            }

            // Node Field
            for (int i = 6; i < numNodes; i++)
            {
                continueFlag = false;
                while (!continueFlag)
                {
                    continueFlag = true;
                    current = new XYDoubleLocation(
                        rand.NextDouble() * (final.X - initial.X - 2 * padding) + initial.X + padding,
                        rand.NextDouble() * (final.Y - initial.Y - 2 * padding) + initial.Y + padding);
                    foreach (XYDoubleLocation point in pointList)
                    {
                        if (current.Distance(point) < minDistance)
                            continueFlag = false;
                    }
                }
                pointList.Add(current);
                current.SetField(field);
                nodes.AddNode(nodeFactory.CreateNode(current));
            }
        }
    }
}

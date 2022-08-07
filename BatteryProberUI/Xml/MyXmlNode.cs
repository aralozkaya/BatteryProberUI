/*
	BatteryProberUI - Implementation of the deprecated Battery Prober project using WPF (C#) with a CLI interface (C++)
    Copyright (C) 2022 Ibrahim Aral Ozkaya

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System.Collections.Generic;
using System.Xml;

namespace BatteryProberUI
{
    public class MyXmlNode
    {
        public MyXmlNode(XmlNode node, MyXmlNode? parent = null)
        {
            this.node = node;
            mParent = parent;
            isRoot = mParent == null;
            mChildren = new List<MyXmlNode>();
            foreach (XmlNode child in node.ChildNodes)
            {
                MyXmlNode mChild = new MyXmlNode(child, this);
                mChildren.Add(mChild);
            }
            isLeaf = !node.HasChildNodes;
            isRoot = node.OwnerDocument == null;
        }

        private readonly XmlNode node;
        private readonly MyXmlNode? mParent;
        private readonly List<MyXmlNode> mChildren;

        public string Name { get { return node.Name; } }
        public MyXmlNode? Parent { get { if (!isRoot) return mParent; else return null; } }
        public List<MyXmlNode>? Children { get { if (!isLeaf) return mChildren; else return null; } }
        public string? Value { get { if (isLeaf) return node.Value; else return null; } set { if (isLeaf) node.Value = value; } }

        public bool isLeaf;
        public bool isRoot;

        public List<MyXmlNode> GetChildrenByName(string tag)
        {
            List<MyXmlNode> result = new();

            foreach (MyXmlNode node in mChildren)
            {
                if (node.Name == tag) result.Add(node);
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControls.MapTileImageProxy
{
    internal class CacheQueue<TKey,TValue>
    {
        private class Node
        {
            public Node Left;
            public Node Right;
            public TKey Key;
        }

        private Dictionary<TKey, Tuple<TValue, Node>> data;
        private Node head, tail;
        private const int cacheSize = 10000;

        public CacheQueue()
        {
            this.data = new Dictionary<TKey, Tuple<TValue, Node>>();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Tuple<TValue, Node> internalValue;

            if(this.data.TryGetValue(key, out internalValue))
            {
                value = internalValue.Item1;
                this.MoveToFront(internalValue.Item2);
                return true;
            }

            value = default(TValue);
            return false;
        }

        public void Add(TKey key , TValue value)
        {
            Tuple<TValue,Node> internalValue;

            if (this.data.TryGetValue(key, out internalValue))
            {
                this.RemoveNode(internalValue.Item2);
            }

            var node = this.AddToFront(key);
            this.data[key] = new Tuple<TValue, Node>(value,node);

            if(this.data.Count > cacheSize)
            {
                this.RemoveLast();
            }
        }

        private void MoveToFront(Node node)
        {
            if(this.head == node) return;

            var left = node.Left;
            var right = node.Right;

            if (this.tail == node) this.tail = node.Left;

            if (left != null) left.Right = right;
            if (right != null) right.Left = left;

            node.Left = null;
            node.Right = this.head;
            this.head.Left = node;
            this.head = node;
        }

        private void RemoveLast()
        {
            if (this.tail == null) return;
            this.data.Remove(this.tail.Key);
            this.RemoveNode(this.tail);
        }

        private void RemoveNode(Node node)
        {
            var left = node.Left;
            var right = node.Right;

            if (left != null) left.Right = right;
            if (right != null) right.Left = left;

            if(node == this.head)
            {
                this.head = right;
            }
            if(node == this.tail)
            {
                this.tail = left;
            }
        }

        private Node AddToFront(TKey key)
        {
            var node = new Node();
            node.Left = null;
            node.Right = this.head;
            node.Key = key;

            if(this.head == null)
            {
                this.head = this.tail = node;
            }
            else
            {
                this.head = node;
            }

            return node;
        }
    }
}

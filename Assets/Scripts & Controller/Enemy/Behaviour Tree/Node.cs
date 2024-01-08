using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    // Single element in the tree
    // Can access children and parent
    // Can store, retrieve and clear data
    public class Node
    {
        // State of the node - all in namespace BehaviorTree can access
        protected NodeState state;

        // With a parent it is possible to backtrack
        public Node parent;
        protected List<Node> children = new List<Node>();

        // Mapping of named variables that can be of any type
        private Dictionary<string, object> _dataContext = new Dictionary<string, object>();

        public Node()
        {
            parent = null;
        }

        public Node(List<Node> children)
        {
            foreach (Node child in children)   
            {
                _Attach(child);
            }
        }

        // creates edge between node and new child
        private void _Attach(Node node)
        {
            node.parent = this;
            children.Add(node);
        }

        // each node can implement its own evaluate function
        public virtual NodeState Evaluate() => NodeState.FAILURE;

        // set data with key
        public void SetData(string key, object value)
        {
            _dataContext[key] = value;
            // Debug.Log("Saving " + key + " in " + this.GetType().Name);
        }

        // get data with key recursively
        // key will be searched in the whole branch
        public object GetData(string key)
        {
            object value = null;
            if (_dataContext.TryGetValue(key, out value))
            {
                return value;
            }
            
            Node node = parent;
            while (node != null)
            {
                value = node.GetData(key);
                if (value != null)
                {
                    return value;
                }
                node = node.parent;
            }
            return null;
        }

        // Method to get the children as an enumerable collection
        public IEnumerable<Node> GetChildren()
        {
            return children;
        }

        // Same logic as get data
        public bool ClearData(string key)
        {
            if (_dataContext.ContainsKey(key))
            {
                _dataContext.Remove(key);
                return true;
            }

            Node node = parent;
            while (node != null)
            {
                bool cleared = node.ClearData(key);
                if (cleared)
                {
                    return true;
                }
                node = node.parent;
            }
            return false;
        }

        // Clear all data to reset tree
        public virtual void ClearData()
        {
            _dataContext.Clear();
        }
    }
}

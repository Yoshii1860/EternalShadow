using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    #region Behavior Tree
    
    /// Represents the possible states of a behavior tree node.
    
    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    #endregion




    #region Node

    /// Represents a single element in the behavior tree.
    public class Node
    {
        #region Fields

        // State of the node - accessible to all in the BehaviorTree namespace
        protected NodeState state;

        // With a parent, it is possible to backtrack
        public Node parent;
        protected List<Node> children = new List<Node>();

        // Mapping of named variables that can be of any type
        private Dictionary<string, object> _dataContext = new Dictionary<string, object>();

        #endregion




        #region Constructors
        
        /// Default constructor for a node with no parent.
        
        public Node()
        {
            parent = null;
        }

        
        /// Constructor for a node with specified children.
        
        /// <param name="children">List of child nodes.</param>
        public Node(List<Node> children)
        {
            foreach (Node child in children)
            {
                _Attach(child);
            }
        }

        #endregion




        #region Private Methods

        
        /// Creates an edge between the node and a new child.
        
        /// <param name="node">Child node to attach.</param>
        private void _Attach(Node node)
        {
            node.parent = this;
            children.Add(node);
        }

        #endregion




        #region Public Methods

        
        /// Evaluates the node's logic. To be implemented by each specific node type.
        
        /// <returns>Resulting state after evaluation.</returns>
        public virtual NodeState Evaluate() => NodeState.FAILURE;

        
        /// Sets data with a specified key.
        public void SetData(string key, object value)
        {
            _dataContext[key] = value;
        }

        
        /// Gets data with a specified key recursively.
        /// Key will be searched in the whole branch.
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

        
        /// Gets the children as an enumerable collection.
        public IEnumerable<Node> GetChildren()
        {
            return children;
        }

        
        /// Clears data with a specified key recursively.
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

        
        /// Clears all data to reset the tree.
        
        public virtual void ClearData()
        {
            _dataContext.Clear();
        }

        #endregion
    }

    #endregion
}
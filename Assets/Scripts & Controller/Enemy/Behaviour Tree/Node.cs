using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    #region Behavior Tree
    
    /// <summary>
    /// Represents the possible states of a behavior tree node.
    /// </summary>
    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    #endregion

    #region Node

    /// <summary>
    /// Represents a single element in the behavior tree.
    /// </summary>
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

        /// <summary>
        /// Default constructor for a node with no parent.
        /// </summary>
        public Node()
        {
            parent = null;
        }

        /// <summary>
        /// Constructor for a node with specified children.
        /// </summary>
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

        /// <summary>
        /// Creates an edge between the node and a new child.
        /// </summary>
        /// <param name="node">Child node to attach.</param>
        private void _Attach(Node node)
        {
            node.parent = this;
            children.Add(node);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Evaluates the node's logic. To be implemented by each specific node type.
        /// </summary>
        /// <returns>Resulting state after evaluation.</returns>
        public virtual NodeState Evaluate() => NodeState.FAILURE;

        /// <summary>
        /// Sets data with a specified key.
        /// </summary>
        /// <param name="key">Key for the data.</param>
        /// <param name="value">Value of the data.</param>
        public void SetData(string key, object value)
        {
            _dataContext[key] = value;
            // Debug.Log("Saving " + key + " in " + this.GetType().Name);
        }

        /// <summary>
        /// Gets data with a specified key recursively.
        /// Key will be searched in the whole branch.
        /// </summary>
        /// <param name="key">Key for the data.</param>
        /// <returns>Data associated with the key.</returns>
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

        /// <summary>
        /// Gets the children as an enumerable collection.
        /// </summary>
        /// <returns>Enumerable collection of child nodes.</returns>
        public IEnumerable<Node> GetChildren()
        {
            return children;
        }

        /// <summary>
        /// Clears data with a specified key recursively.
        /// </summary>
        /// <param name="key">Key for the data to be cleared.</param>
        /// <returns>True if the data is cleared, false otherwise.</returns>
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

        /// <summary>
        /// Clears all data to reset the tree.
        /// </summary>
        public virtual void ClearData()
        {
            _dataContext.Clear();
        }

        #endregion
    }

    #endregion
}
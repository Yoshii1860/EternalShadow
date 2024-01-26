using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// The base class for behavior trees.
    /// </summary>
    public abstract class Tree : MonoBehaviour
    {
        #region Fields

        // Recursively contains the whole tree
        protected Node _root = null;

        #endregion

        #region Unity Lifecycle Methods

        /// <summary>
        /// Initializes the behavior tree by setting up the root node.
        /// </summary>
        protected void Start()
        {
            _root = SetupTree();
        }

        /// <summary>
        /// Continuously evaluates the behavior tree if it exists.
        /// </summary>
        void Update()
        {
            if (_root != null)
            {
                _root.Evaluate();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets data of all nodes in the tree.
        /// </summary>
        protected void ClearNodeDataRecursive(Node node)
        {
            node.ClearData();
            foreach (var child in node.GetChildren())
            {
                ClearNodeDataRecursive(child);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Sets up and returns the root node of the behavior tree.
        /// </summary>
        /// <returns>The root node of the behavior tree.</returns>
        protected abstract Node SetupTree();

        #endregion
    }
}
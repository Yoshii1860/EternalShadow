using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    /// The base class for behavior trees.
    public abstract class Tree : MonoBehaviour
    {
        #region Fields

        // Recursively contains the whole tree
        protected Node _root = null;

        #endregion




        #region Unity Lifecycle Methods

        /// Initializes the behavior tree by setting up the root node.
        protected void Start()
        {
            _root = SetupTree();
        }

        /// Continuously evaluates the behavior tree if it exists.
        void Update()
        {
            if (_root != null)
            {
                _root.Evaluate();
            }
        }

        #endregion




        #region Public Methods

        /// Resets data of all nodes in the tree.
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

        /// Sets up and returns the root node of the behavior tree.
        protected abstract Node SetupTree();

        #endregion
    }
}
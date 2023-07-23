using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public abstract class Tree : MonoBehaviour
    {
        // Recursively contains the whole tree
        protected Node _root = null;

        // Build behavior tree
        protected void Start()
        {
            _root = SetupTree();
        }

        // If it has a tree, it will evaluate it continuously
        void Update()
        {
            if (_root != null)
            {
                _root.Evaluate();
            }
        }

        // Reset data of all nodes
        protected void ClearNodeDataRecursive(Node node)
        {
            node.ClearData();
            foreach (var child in node.GetChildren())
            {
                ClearNodeDataRecursive(child);
            }
        }

        protected abstract Node SetupTree();
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// The Selector node represents a control flow node in the behavior tree.
    /// It iterates through its children and returns success if any child succeeds or runs.
    /// </summary>
    public class Selector : Node
    {
        #region Constructors

        /// <summary>
        /// Default constructor for Selector.
        /// </summary>
        public Selector() : base() { }

        /// <summary>
        /// Constructor for Selector with initial children.
        /// </summary>
        /// <param name="children">List of child nodes.</param>
        public Selector(List<Node> children) : base(children) { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Evaluate the Selector node by iterating through its children.
        /// If any child succeeds or runs, stop and return success/running.
        /// </summary>
        /// <returns>The state of the Selector node after evaluation.</returns>
        public override NodeState Evaluate()
        {
            foreach (Node node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        continue;
                    case NodeState.SUCCESS:
                        state = NodeState.SUCCESS;
                        return state;
                    case NodeState.RUNNING:
                        state = NodeState.RUNNING;
                        return state;
                    default:
                        continue;
                }
            }

            state = NodeState.FAILURE;
            return state;
        }

        #endregion
    }
}
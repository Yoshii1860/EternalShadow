using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    /// <summary>
    /// The Sequence node represents a control flow node in the behavior tree.
    /// It iterates through its children and returns failure if any child fails.
    /// </summary>
    public class Sequence : Node
    {
        #region Constructors

        /// <summary>
        /// Default constructor for Sequence.
        /// </summary>
        public Sequence() : base() { }

        /// <summary>
        /// Constructor for Sequence with initial children.
        /// </summary>
        /// <param name="children">List of child nodes.</param>
        public Sequence(List<Node> children) : base(children) { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Evaluate the Sequence node by iterating through its children.
        /// If any child fails, stop and return failure.
        /// </summary>
        /// <returns>The state of the Sequence node after evaluation.</returns>
        public override NodeState Evaluate()
        {
            bool isAnyChildRunning = false;

            foreach (Node node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        state = NodeState.FAILURE;
                        return state;
                    case NodeState.SUCCESS:
                        continue;
                    case NodeState.RUNNING:
                        isAnyChildRunning = true;
                        continue;
                    default:
                        state = NodeState.SUCCESS;
                        return state;
                }
            }

            // Check if some are running and block us in the running state
            // or if all are successful and we can return success
            state = isAnyChildRunning ? NodeState.RUNNING : NodeState.SUCCESS;
            return state;
        }

        #endregion
    }
}
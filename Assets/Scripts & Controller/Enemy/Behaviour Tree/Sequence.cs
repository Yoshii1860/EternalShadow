using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    /// The Sequence node represents a control flow node in the behavior tree.
    /// It iterates through its children and returns failure if any child fails.
    public class Sequence : Node
    {
        #region Constructors

        /// Default constructor for Sequence.
        public Sequence() : base() { }

        /// Constructor for Sequence with initial children.
        public Sequence(List<Node> children) : base(children) { }

        #endregion




        #region Public Methods

        /// Evaluate the Sequence node by iterating through its children.
        /// If any child fails, stop and return failure.
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
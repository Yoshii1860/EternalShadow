using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    /// The Selector node represents a control flow node in the behavior tree.
    /// It iterates through its children and returns success if any child succeeds or runs.
    public class Selector : Node
    {
        #region Constructors

        /// Default constructor for Selector.
        public Selector() : base() { }

        /// Constructor for Selector with initial children.
        public Selector(List<Node> children) : base(children) { }

        #endregion





        #region Public Methods

        /// Evaluate the Selector node by iterating through its children.
        /// If any child succeeds or runs, stop and return success/running.
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
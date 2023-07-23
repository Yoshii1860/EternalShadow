using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{  
    public class Sequence : Node
    {
        public Sequence() : base() { }
        public Sequence(List<Node> children) : base(children) { }

        // iterate through all children and check the state after evaluating
        // If any child fails, stop and return failure
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

            // check if some are running and block us in the running state
            // or if all are successfull and we can return success
            state = isAnyChildRunning ? NodeState.RUNNING : NodeState.SUCCESS;
            return state;
        }
    }
}

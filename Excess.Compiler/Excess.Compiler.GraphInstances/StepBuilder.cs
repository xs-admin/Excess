using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Excess.Compiler.Core;

namespace Excess.Compiler.GraphInstances
{
    public class StepBuilder
    {
        Dictionary<object, Step> _steps;
        public StepBuilder(Dictionary<object, Step> steps)
        {
            _steps = steps;
        }


        public StepContainer Result { get { return buildResult(); } }

        List<StepChain> _chains = new List<StepChain>();
        public void AddConnection(Connection connection)
        {
            StepChain left = null;
            StepChain right = null;
            foreach (var chain in _chains)
            {
                if (chain.Append(connection))
                {
                    if (left == null)
                        left = chain;
                    else
                    {
                        Debug.Assert(right == null);
                        right = chain;
                    }
                }
            }

            if (left != null && right != null)
            {
                if (left.Append(right))
                    _chains.Remove(right);
                else if (right.Append(left))
                    _chains.Remove(left);
                else
                    Debug.Assert(false);
            }
            else if (left == null && right == null)
                _chains.Add(new StepChain(connection, this));
        }

        public void StartChain(Connection link)
        {
            var step = GetStep(link.Source);
            _chains.Add(new StepChain(link, this, step, link.InputConnector));
        }

        private Step GetStep(string node)
        {
            return _steps[node];
        }

        private StepContainer buildResult()
        {
            StepContainer result = null;
            foreach (var chain in _chains)
            {
                var container = new StepContainer(chain.Steps);
                if (chain.Parent == null)
                {
                    Debug.Assert(result == null);
                    result = container;
                }
                else
                {
                    chain.Parent.Inner[chain.ParentConnector] = container;
                }
            }

            Debug.Assert(result != null);
            return result;
        }

        private class StepChain
        {
            StepBuilder _builder;

            public Step Parent { get; private set; }
            public string ParentConnector { get; private set; }

            public string Left { get; private set; }
            public string Right { get; private set; }
            public IEnumerable<Step> Steps { get { return _steps; } }

            List<Step> _steps = new List<Step>();

            public StepChain(Connection link, StepBuilder builder)
            {
                Left = link.Source;
                Right = link.Target;

                _builder = builder;
                _steps.Add(_builder.GetStep(Left));
                _steps.Add(_builder.GetStep(Right));
            }

            public StepChain(Connection link, StepBuilder builder, Step parent, string parentConnector)
            {
                Parent = parent;
                ParentConnector = parentConnector;

                Left = null;
                Right = link.Target;

                _builder = builder;
                _steps.Add(_builder.GetStep(Right));
            }

            public bool Append(Connection link)
            {
                if (checkLeft(link))
                {
                    Left = link.Source;
                    _steps.Insert(0, _builder.GetStep(Left));
                }
                else if (checkRight(link))
                {
                    Right = link.Target;
                    _steps.Add(_builder.GetStep(Right));
                }
                else
                    return false;

                return true;
            }

            public bool Append(StepChain chain)
            {
                if (chain.Left != Right)
                    return false;

                Right = chain.Right;
                _steps.AddRange(chain.Steps.Skip(1));

                return true;
            }

            private bool checkLeft(Connection link)
            {
                return link.Target == Left;
            }

            private bool checkRight(Connection link)
            {
                return link.Source == Right;
            }

        }
    }
}

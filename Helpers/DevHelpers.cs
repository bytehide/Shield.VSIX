using System;
using System.Collections.Generic;
using System.Linq;

namespace ShieldVSExtension.Helpers
{
    static class DevHelpers
    {
        internal struct DisposeAction : IDisposable
        {
            readonly Action _action;

            public DisposeAction(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }

        static readonly IDisposable NullDisposeAction = new DisposeAction(() => { });

        public static void ForEach<ElementT>(this IEnumerable<ElementT> sequence, Action<ElementT> action)
        {
            foreach (var element in sequence)
            {
                action(element);
            }
        }
        public static IEnumerable<NodeT> SortTopologically<NodeT>(this IEnumerable<NodeT> roots, Func<NodeT, IEnumerable<NodeT>> edges)
        {
            return SortTopologicallyReverse(roots, edges).Reverse();
        }

        // returns roots last.

        public static IEnumerable<NodeT> SortTopologicallyReverse<NodeT>(this IEnumerable<NodeT> roots, Func<NodeT, IEnumerable<NodeT>> edges)
        {
            var res = new List<NodeT>();
            var marked = new HashSet<NodeT>();
            foreach (var n in roots)
                AddNode(res, marked, n, edges);

            return res;
        }

        static void AddNode<NodeT>(ICollection<NodeT> list, HashSet<NodeT> marked, NodeT node, Func<NodeT, IEnumerable<NodeT>> edges)
        {
            if (!marked.Add(node))
                return;

            foreach (var more in edges(node))
                AddNode(list, marked, more, edges);

            list.Add(node);
        }
    }
}

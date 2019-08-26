using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DieFaceDistributer
{

    public class TreeNode<T> : IEnumerable // From https://stackoverflow.com/a/10442244 contributed by Ronnie Overby, edited by me
    {
        private readonly T _value;
        private readonly List<TreeNode<T>> _children = new List<TreeNode<T>>();

        public TreeNode(T value)
        {
            _value = value;
        }

        public TreeNode<T> this[int i]
        {
            get { return  _children[i]; }
        }

        public TreeNode<T> Parent { get; private set; }

        public T Value { get { return _value; } }

        public ReadOnlyCollection<TreeNode<T>> Children
        {
            get { return _children.AsReadOnly(); }
        }

        public TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value) { Parent = this };
            _children.Add(node);
            return node;
        }

        public IEnumerable<TreeNode<T>> AddChildren(params T[] values)
        {
            return values.Select(AddChild);
        }

        public IEnumerable<TreeNode<T>> AddChildren(IEnumerable<T> values)
        {
            return values.Select(AddChild);
        }

        public bool RemoveChild(TreeNode<T> node)
        {
            return _children.Remove(node);
        }

        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in _children)
                child.Traverse(action);
        }

        public IEnumerator GetEnumerator()
        {
            yield return Value;
            foreach (TreeNode<T> child in _children)
            {
                yield return child.Value;
            }
        }

        public IEnumerable<T> Flatten()
        {
            return new[] { Value }.Concat(_children.SelectMany(x => x.Flatten()));
        }
    }
}

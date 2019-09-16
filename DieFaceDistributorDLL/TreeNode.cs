using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DieFaceDistributer
{

    public class TreeNode<T>  // From https://stackoverflow.com/a/10442244 contributed by Ronnie Overby, edited by me
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

        public TreeNode<T> AddChild(TreeNode<T> value)
        {
            value.Parent = this;
            _children.Add(value);
            return value;
        }

        public IEnumerable<TreeNode<T>> AddChildren(params T[] values)
        {
            foreach(T value in values)
            {
                AddChild(new TreeNode<T>(value));
            }
            return Children;
        }

        public IEnumerable<TreeNode<T>> AddChildren(IEnumerable<T> values)
        {
            foreach(T value in values)
            {
                AddChild(new TreeNode<T>(value));
            }
            return Children;
        }

        public IEnumerable<TreeNode<T>> AddChildren(IEnumerable<TreeNode<T>> values)
        {
            foreach(TreeNode<T> value in values)
            {
                AddChild(value);
            }
            return values;
        }

        public bool RemoveChild(TreeNode<T> node)
        {
            return _children.Remove(node);
        }

        public IEnumerable<TreeNode<T>> DeepestChildren()
        {
            if (_children.Count == 0) yield return this;
            foreach (TreeNode<T> child in _children)
            {
                foreach(TreeNode<T> descendant in child.DeepestChildren())
                {
                    yield return descendant;
                }
            }
            yield break;
        }

        public IEnumerable<T> Parents()
        {
            TreeNode<T> parent = this.Parent;
            while (parent != null)
            {
                yield return parent.Value;
                parent = parent.Parent;
            }
            yield break;
        }

        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in _children)
                child.Traverse(action);
        }
        /*
        public IEnumerator GetEnumerator()
        {
            yield return Value;
            foreach (TreeNode<T> child in _children)
            {
                yield return child.Value;
            }
        }
        
        
        public IEnumerator<T> GetEnumerator()
        {
            yield return Value;
            foreach (TreeNode<T> child in _children)
            {
                yield return child.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return Value;
            foreach (TreeNode<T> child in _children)
            {
                yield return child.Value;
            }
        }*/

        public IEnumerable<T> Flatten()
        {
            return new[] { Value }.Concat(_children.SelectMany(x => x.Flatten()));
        }        
    }
}

namespace GameLeaderboard;

public class AugmentedRedBlackTree<T>
{
    private enum Color { Red, Black }

    private class Node
    {
        public T Value { get; set; }
        public Node? Left { get; set; }
        public Node? Right { get; set; }
        public Node? Parent { get; set; }
        public Color Color { get; set; }
        public int Size { get; set; } // Augmentation: subtree size

        public Node(T value, Color color = Color.Red)
        {
            Value = value;
            Color = color;
            Size = 1;
        }
    }

    private Node? _root;
    private readonly IComparer<T> _comparer;

    public int Count => _root?.Size ?? 0;

    public AugmentedRedBlackTree(IComparer<T> comparer)
    {
        _comparer = comparer;
    }
    
    public void BulkInsert(List<T> items)
    {
        if (items.Count == 0) return;

        // Sort items first - O(n log n)
        items.Sort(_comparer);

        // Clear existing tree
        _root = null;

        // Build balanced tree from sorted array - O(n)
        _root = BuildBalancedTree(items, 0, items.Count - 1, null);

        if (_root != null)
            _root.Color = Color.Black;
    }

    private Node? BuildBalancedTree(List<T> sortedItems, int start, int end, Node? parent)
    {
        if (start > end) return null;

        int mid = start + (end - start) / 2;
        var item = sortedItems[mid];

        var node = new Node(item, Color.Black) { Parent = parent };

        node.Left = BuildBalancedTree(sortedItems, start, mid - 1, node);
        node.Right = BuildBalancedTree(sortedItems, mid + 1, end, node);

        // Calculate size
        node.Size = 1 + GetSize(node.Left) + GetSize(node.Right);

        return node;
    }

    public void Insert(T item)
    {
        if (_root == null)
        {
            _root = new Node(item, Color.Black);
            return;
        }

        // Standard BST insertion
        Node? parent = null;
        Node? current = _root;

        while (current != null)
        {
            parent = current;
            int comparison = _comparer.Compare(item, current.Value);

            if (comparison < 0)
                current = current.Left;
            else if (comparison > 0)
                current = current.Right;
            else
                return; // Duplicate - don't insert
        }

        // Create new node
        var newNode = new Node(item);
        newNode.Parent = parent;

        if (_comparer.Compare(item, parent!.Value) < 0)
            parent.Left = newNode;
        else
            parent.Right = newNode;

        // Update sizes up the tree
        UpdateSizesUpward(newNode);

        // Fix Red-Black Tree properties
        FixInsert(newNode);
    }

    public int GetRank(T item)
    {
        Node? node = FindNode(item);
        if (node == null) return -1;

        int rank = GetSize(node.Left) + 1; // Rank within its subtree

        // Traverse up and add left subtrees when coming from right
        while (node != _root)
        {
            if (node == node.Parent?.Right)
            {
                rank += GetSize(node.Parent.Left) + 1;
            }
            node = node.Parent!;
        }

        return rank;
    }

    public T? GetItemAtRank(int rank)
    {
        if (rank < 1 || rank > Count)
            return default;

        return GetNodeAtRank(_root, rank)!.Value;
    }

    public IEnumerable<T> InOrderTraversal()
    {
        return InOrderTraversalHelper(_root);
    }

    public bool Delete(T item)
    {
        Node? node = FindNode(item);
        if (node == null) return false;

        DeleteNode(node);
        return true;
    }

    private Node? FindNode(T item)
    {
        Node? current = _root;

        while (current != null)
        {
            int comparison = _comparer.Compare(item, current.Value);

            if (comparison == 0)
                return current;
            else if (comparison < 0)
                current = current.Left;
            else
                current = current.Right;
        }

        return null;
    }

    private void DeleteNode(Node target)
    {
        Node? replacement;
        Node? fixNode;
        Node? fixParent;
        Color originalColor = target.Color;

        if (target.Left == null)
        {
            replacement = target.Right;
            fixNode = replacement;
            fixParent = target.Parent;
            Transplant(target, replacement);
        }
        else if (target.Right == null)
        {
            replacement = target.Left;
            fixNode = replacement;
            fixParent = target.Parent;
            Transplant(target, replacement);
        }
        else
        {
            // Node has two children: find successor
            var successor = Minimum(target.Right);
            originalColor = successor.Color;
            fixNode = successor.Right;

            if (successor.Parent == target)
            {
                fixParent = successor;
            }
            else
            {
                fixParent = successor.Parent;
                Transplant(successor, successor.Right);
                UpdateSizesUpward(fixParent);
                successor.Right = target.Right;
                successor.Right.Parent = successor;
            }

            Transplant(target, successor);
            successor.Left = target.Left;
            successor.Left.Parent = successor;
            successor.Color = target.Color;
            UpdateSizesUpward(successor);
        }

        UpdateSizesUpward(fixParent);

        if (originalColor == Color.Black)
        {
            FixDelete(fixNode, fixParent);
        }
    }

    // Move the subtree inside within RB-tree
    private void Transplant(Node u, Node? v)
    {
        if (u.Parent == null)
            _root = v;
        // u is left child
        else if (u == u.Parent.Left)
            u.Parent.Left = v;
        else
            u.Parent.Right = v;

        if (v != null)
            v.Parent = u.Parent;
    }

    private static Node Minimum(Node node)
    {
        while (node.Left != null)
            node = node.Left;
        return node;
    }

    private Node? GetNodeAtRank(Node? node, int rank)
    {
        if (node == null) return null;

        int leftSize = GetSize(node.Left);
        int currentRank = leftSize + 1;

        if (rank == currentRank)
        {
            // Знайшли потрібний вузол
            return node;
        }
        else if (rank < currentRank)
        {
            // Шукаємо в лівому піддереві
            return GetNodeAtRank(node.Left, rank);
        }
        else
        {
            // Шукаємо в правому піддереві
            // Коригуємо ранг: віднімаємо розмір лівого піддерева + 1 (поточний вузол)
            return GetNodeAtRank(node.Right, rank - currentRank);
        }
    }

    private static IEnumerable<T> InOrderTraversalHelper(Node? node)
    {
        if (node == null) yield break;

        foreach (var item in InOrderTraversalHelper(node.Left))
            yield return item;

        yield return node.Value;

        foreach (var item in InOrderTraversalHelper(node.Right))
            yield return item;
    }

    private static int GetSize(Node? node) => node?.Size ?? 0;

    private void UpdateSizesUpward(Node? node)
    {
        while (node != null)
        {
            node.Size = 1 + GetSize(node.Left) + GetSize(node.Right);
            node = node.Parent;
        }
    }

    private void FixInsert(Node node)
    {
        while (node.Parent?.Color == Color.Red)
        {
            // whether to look for uncle in left sibling or right sibling 
            if (node.Parent == node.Parent.Parent?.Left)
            {
                var uncle = node.Parent.Parent.Right;

                if (uncle?.Color == Color.Red)
                {
                    // Case 2: Uncle is red - recolor
                    node.Parent.Color = Color.Black;
                    uncle.Color = Color.Black;
                    node.Parent.Parent.Color = Color.Red;
                    node = node.Parent.Parent;
                }
                else
                {
                    // Case 3b: Uncle is black, node is right child
                    if (node == node.Parent.Right)
                    {
                        node = node.Parent;
                        RotateLeft(node);
                    }

                    // Case 3a: Uncle is black, node is left child
                    node.Parent!.Color = Color.Black;
                    node.Parent.Parent!.Color = Color.Red;
                    RotateRight(node.Parent.Parent);
                }
            }
            else
            {
                // Same cases mirrored
                var uncle = node.Parent.Parent?.Left;

                if (uncle?.Color == Color.Red)
                {
                    node.Parent.Color = Color.Black;
                    uncle.Color = Color.Black;
                    node.Parent.Parent!.Color = Color.Red;
                    node = node.Parent.Parent;
                }
                else
                {
                    if (node == node.Parent.Left)
                    {
                        node = node.Parent;
                        RotateRight(node);
                    }

                    node.Parent!.Color = Color.Black;
                    node.Parent.Parent!.Color = Color.Red;
                    RotateLeft(node.Parent.Parent);
                }
            }
        }
        
        // Case 1
        _root!.Color = Color.Black;
    }

    private void FixDelete(Node? node, Node? parent)
    {
        while (node != _root && (node?.Color ?? Color.Black) == Color.Black)
        {
            if (node == parent?.Left)
            {
                var sibling = parent!.Right;

                // Case 1
                if (sibling?.Color == Color.Red)
                {
                    sibling.Color = Color.Black;
                    parent.Color = Color.Red;
                    RotateLeft(parent);
                    sibling = parent.Right;
                }

                // Case 2
                if ((sibling?.Left?.Color ?? Color.Black) == Color.Black &&
                    (sibling?.Right?.Color ?? Color.Black) == Color.Black)
                {
                    if (sibling != null) sibling.Color = Color.Red;
                    node = parent;
                    parent = node.Parent;
                }
                else
                {
                    
                    // Case 3
                    if ((sibling?.Right?.Color ?? Color.Black) == Color.Black)
                    {
                        if (sibling?.Left != null) sibling.Left.Color = Color.Black;
                        if (sibling != null) sibling.Color = Color.Red;
                        RotateRight(sibling!);
                        sibling = parent.Right;
                    }
                    
                    // Case 4
                    if (sibling != null) sibling.Color = parent.Color;
                    parent.Color = Color.Black;
                    if (sibling?.Right != null) sibling.Right.Color = Color.Black;
                    RotateLeft(parent);
                    node = _root;
                }
            }
            else
            {
                var sibling = parent?.Left;

                if (sibling?.Color == Color.Red)
                {
                    sibling.Color = Color.Black;
                    parent!.Color = Color.Red;
                    RotateRight(parent);
                    sibling = parent.Left;
                }

                if ((sibling?.Right?.Color ?? Color.Black) == Color.Black &&
                    (sibling?.Left?.Color ?? Color.Black) == Color.Black)
                {
                    if (sibling != null) sibling.Color = Color.Red;
                    node = parent;
                    parent = node?.Parent;
                }
                else
                {
                    if ((sibling?.Left?.Color ?? Color.Black) == Color.Black)
                    {
                        if (sibling?.Right != null) sibling.Right.Color = Color.Black;
                        if (sibling != null) sibling.Color = Color.Red;
                        RotateLeft(sibling!);
                        sibling = parent?.Left;
                    }

                    if (sibling != null) sibling.Color = parent!.Color;
                    if (parent != null) parent.Color = Color.Black;
                    if (sibling?.Left != null) sibling.Left.Color = Color.Black;
                    RotateRight(parent!);
                    node = _root;
                }
            }
        }

        if (node != null) node.Color = Color.Black;
    }

    private void RotateLeft(Node x)
    {
        // y стає новим коренем цього піддерева
        Node? y = x.Right;
        if (y == null) return;

        // Переносимо ліве піддерево y як праве піддерево x
        x.Right = y.Left;
        if (y.Left != null)
            y.Left.Parent = x;

        // Оновлюємо батька y
        y.Parent = x.Parent;
        
        if (x.Parent == null)
        {
            // x був коренем дерева
            _root = y;
        }
        else if (x == x.Parent.Left)
        {
            // x був лівим нащадком
            x.Parent.Left = y;
        }
        else
        {
            // x був правим нащадком
            x.Parent.Right = y;
        }

        // Робимо x лівим нащадком y
        y.Left = x;
        x.Parent = y;

        // ВАЖЛИВО: Оновлюємо Size
        // Спочатку оновлюємо x (нижній вузол)
        x.Size = 1 + GetSize(x.Left) + GetSize(x.Right);
        // Потім оновлюємо y (верхній вузол)
        y.Size = 1 + GetSize(y.Left) + GetSize(y.Right);
    }

    private void RotateRight(Node y)
    {
        // x стає новим коренем цього піддерева
        Node? x = y.Left;
        if (x == null) return;

        // Переносимо праве піддерево x як ліве піддерево y
        y.Left = x.Right;
        if (x.Right != null)
            x.Right.Parent = y;

        // Оновлюємо батька x
        x.Parent = y.Parent;
        
        if (y.Parent == null)
        {
            // y був коренем дерева
            _root = x;
        }
        else if (y == y.Parent.Right)
        {
            // y був правим нащадком
            y.Parent.Right = x;
        }
        else
        {
            // y був лівим нащадком
            y.Parent.Left = x;
        }

        // Робимо y правим нащадком x
        x.Right = y;
        y.Parent = x;

        // ВАЖЛИВО: Оновлюємо Size
        // Спочатку оновлюємо y (нижній вузол)
        y.Size = 1 + GetSize(y.Left) + GetSize(y.Right);
        // Потім оновлюємо x (верхній вузол)
        x.Size = 1 + GetSize(x.Left) + GetSize(x.Right);
    }
}
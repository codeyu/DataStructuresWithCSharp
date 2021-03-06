using System;
namespace DataStructures._3.BalancedBinaryTree
{
    public class BinarySearchTree<K, V> where K : IComparable<K>
    {    //成员变量
        private AVLNode<K, V> _root; //头指针
        private AVLNode<K, V>[] path = new AVLNode<K, V>[32]; //记录访问路径上的结点
        private int p; //表示当前访问到的结点在_path上的索引
        
        public bool Add(K key, V value) //添加一个元素
        {   //如果是空树，则新结点成为二叉排序树的根
            if (_root == null)
            {
                _root = new AVLNode<K, V>(key, value, null, null);
                _root.Balance = 0;
                return true;
            }
            p = 0;
            //prev为上一次访问的结点，current为当前访问结点
            AVLNode<K, V> prev = null, current = _root;
            while (current != null)
            {
                path[p++] = current; //将路径上的结点插入数组
                //如果插入值已存在，则插入失败
                if (key.CompareTo(current.Key) == 0)
                {
                    return false;
                }
                prev = current;
                //当插入值小于当前结点，则继续访问左子树，否则访问右子树
                current = (key.CompareTo(prev.Key) < 0 ) ? prev.Left : prev.Right;
            }
            current = new AVLNode<K, V>(key, value, null, null);; //创建新结点
            current.Balance = 0;
            if (key.CompareTo(prev.Key) < 0) //如果插入值小于双亲结点的值
            {
                prev.Left = current; //成为左孩子
            }
            else //如果插入值大于双亲结点的值
            {
                prev.Right = current; //成为右孩子
            }
            path[p] = current; //将新元素插入数组path的最后
            //修改插入点至根结点路径上各结点的平衡因子
            int Balance = 0;
            while (p > 0)
            {   //Balance表示平衡因子的改变量，当新结点插入左子树，则平衡因子+1
                //当新结点插入右子树，则平衡因子-1
                Balance = (key.CompareTo(path[p - 1].Key) < 0) ? 1 : -1;
                path[--p].Balance += Balance; //改变当父结点的平衡因子
                Balance = path[p].Balance; //获取当前结点的平衡因子
                //判断当前结点平衡因子，如果为0表示该子树已平衡，不需再回溯
                //而改变祖先结点平衡因子，此时添加成功，直接返回
                if (Balance == 0)
                {
                    return true;
                }
                else if (Balance == 2 || Balance == -2) //需要旋转的情况
                {
                    RotateSubTree(Balance);
                    return true;
                }
            }
            return true;
        }
        //删除指定值
        public bool Remove(K key) 
        {
            p = -1;
            //parent表示双亲结点，node表示当前结点
            AVLNode<K, V> node = _root;
            //寻找指定值所在的结点
            while (node != null)
            {
                path[++p] = node;
                //如果找到，则调用RemoveNode方法删除结点
                if (key.CompareTo(node.Key) == 0)
                {
                    RemoveNode(node);//现在p指向被删除结点
                    return true; //返回true表示删除成功
                }
                if (key.CompareTo(node.Key) < 0)
                {   //如果删除值小于当前结点，则向左子树继续寻找
                    node = node.Left;
                }
                else
                {   //如果删除值大于当前结点，则向右子树继续寻找
                    node = node.Right;
                }
            }
            return false; //返回false表示删除失败
        }
        //删除指定结点
        private void RemoveNode(AVLNode<K, V>  node)
        {
            AVLNode<K, V>  tmp = null;
            //当被删除结点存在左右子树时
            if (node.Left != null && node.Right != null)
            {
                tmp = node.Left; //获取左子树
                path[++p] = tmp;
                while (tmp.Right != null) //获取node的中序遍历前驱结点，并存放于tmp中
                {   //找到左子树中的最右下结点
                    tmp = tmp.Right;
                    path[++p] = tmp;
                }
                //用中序遍历前驱结点的值代替被删除结点的值
                node.Key = tmp.Key;
                if (path[p - 1] == node)
                {
                    path[p - 1].Left = tmp.Left;
                }
                else
                {
                    path[p - 1].Right = tmp.Left;
                }
            }
            else //当只有左子树或右子树或为叶子结点时
            {   //首先找到惟一的孩子结点
                tmp = node.Left;
                if (tmp == null) //如果只有右孩子或没孩子
                {
                    tmp = node.Right;
                }
                if (p > 0)
                {
                    if (path[p - 1].Left == node)
                    {   //如果被删结点是左孩子
                        path[p - 1].Left = tmp;
                    }
                    else
                    {   //如果被删结点是右孩子
                        path[p - 1].Right = tmp;
                    }
                }
                else  //当删除的是根结点时
                {
                    _root = tmp;
                }
            }
            //删除完后进行旋转，现在p指向实际被删除的结点
            K key = node.Key;
            while (p > 0)
            {   //Balance表示平衡因子的改变量，当删除的是左子树中的结点时，平衡因子-1
                //当删除的是右子树的孩子时，平衡因子+1
                int Balance = (key.CompareTo(path[p - 1].Key) <= 0) ? -1 : 1;
                path[--p].Balance += Balance; //改变当父结点的平衡因子
                Balance = path[p].Balance; //获取当前结点的平衡因子
                if (Balance != 0) //如果Balance==0，表明高度降低，继续后上回溯
                {
                    //如果Balance为1或-1则说明高度未变，停止回溯，如果为2或-2，则进行旋转
                    //当旋转后高度不变，则停止回溯
                    if (Balance == 1 || Balance == -1 || !RotateSubTree(Balance))
                    {
                        break;
                    }
                }
            }
        }
        //旋转以root为根的子树，当高度改变，则返回true；高度未变则返回false
        private bool RotateSubTree(int Balance) 
        {
            bool tallChange = true;
            AVLNode<K, V> root = path[p], newRoot = null;
            if (Balance == 2) //当平衡因子为2时需要进行旋转操作
            {
                int leftBF = root.Left.Balance;
                if (leftBF == -1) //LR型旋转
                {
                    newRoot = LR(root);
                }
                else if (leftBF == 1)
                {
                    newRoot = LL(root); //LL型旋转
                }
                else //当旋转根左孩子的Balance为0时，只有删除时才会出现
                {
                    newRoot = LL(root);
                    tallChange = false;
                }
            }
            if (Balance == -2) //当平衡因子为-2时需要进行旋转操作
            {
                int rightBF = root.Right.Balance; //获取旋转根右孩子的平衡因子
                if (rightBF == 1) 
                {
                    newRoot = RL(root); //RL型旋转
                }
                else if (rightBF == -1)
                {
                    newRoot = RR(root); //RR型旋转
                }
                else //当旋转根左孩子的Balance为0时，只有删除时才会出现
                {
                    newRoot = RR(root);
                    tallChange = false;
                }
            }
            //更改新的子树根
            if (p > 0)
            {
                if (root.Key.CompareTo(path[p - 1].Key) < 0)
                {
                    path[p - 1].Left = newRoot;
                }
                else
                {
                    path[p - 1].Right = newRoot;
                }
            }
            else
            {
                _root = newRoot; //如果旋转根为AVL树的根，则指定新AVL树根结点
            }
            return tallChange;
        }
        //root为旋转根，rootPrev为旋转根双亲结点
        private AVLNode<K, V> LL(AVLNode<K, V> root) //LL型旋转，返回旋转后的新子树根
        {
            AVLNode<K, V> rootNext = root.Left;
            root.Left = rootNext.Right;
            rootNext.Right = root;
            if (rootNext.Balance == 1)
            {
                root.Balance = 0;
                rootNext.Balance = 0;
            }
            else //rootNext.Balance==0的情况，删除时用
            {
                root.Balance = 1;
                rootNext.Balance = -1;
            }
            return rootNext; //rootNext为新子树的根
        }
        private AVLNode<K, V> LR(AVLNode<K, V> root) //LR型旋转，返回旋转后的新子树根
        {
            AVLNode<K, V> rootNext = root.Left;
            AVLNode<K, V> newRoot = rootNext.Right;
            root.Left = newRoot.Right;
            rootNext.Right = newRoot.Left;
            newRoot.Left = rootNext;
            newRoot.Right = root;
            switch (newRoot.Balance) //改变平衡因子
            {
                case 0:
                    root.Balance = 0;
                    rootNext.Balance = 0;
                    break;
                case 1:
                    root.Balance = -1;
                    rootNext.Balance = 0;
                    break;
                case -1:
                    root.Balance = 0;
                    rootNext.Balance = 1;
                    break;
            }
            newRoot.Balance = 0;
            return newRoot; //newRoot为新子树的根
        }
        private AVLNode<K, V> RR(AVLNode<K, V> root) //RR型旋转，返回旋转后的新子树根
        {
            AVLNode<K, V> rootNext = root.Right;
            root.Right = rootNext.Left;
            rootNext.Left = root;
            if (rootNext.Balance == -1)
            {
                root.Balance = 0;
                rootNext.Balance = 0;
            }
            else //rootNext.Balance==0的情况，删除时用
            {
                root.Balance = -1;
                rootNext.Balance = 1;
            }
            return rootNext; //rootNext为新子树的根
        }
        private AVLNode<K, V> RL(AVLNode<K, V> root) //RL型旋转，返回旋转后的新子树根
        {
            AVLNode<K, V> rootNext = root.Right;
            AVLNode<K, V> newRoot = rootNext.Left;
            root.Right = newRoot.Left;
            rootNext.Left = newRoot.Right;
            newRoot.Right = rootNext;
            newRoot.Left = root;
            switch (newRoot.Balance) //改变平衡因子
            {
                case 0:
                    root.Balance = 0;
                    rootNext.Balance = 0;
                    break;
                case 1:
                    root.Balance = 0;
                    rootNext.Balance = -1;
                    break;
                case -1:
                    root.Balance = 1;
                    rootNext.Balance = 0;
                    break;
            }
            newRoot.Balance = 0;
            return newRoot; //newRoot为新子树的根
        }
    }
}

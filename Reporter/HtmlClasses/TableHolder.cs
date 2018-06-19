using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalReporter
{
    public class HtmlTable :HtmlNode
    {
        public List<HtmlNode> TitleRows { get; set; }
        public List<HtmlNode> CellRows { get; set; }

        public void AddCellRow(HtmlNode rowNode)
        {
            Children.Add(rowNode);
            CellRows.Add(rowNode);
        }

        public void AddTitleRow(HtmlNode titleNode)
        {
            Children.Add(titleNode);
            TitleRows.Add(titleNode);
        }

        public void RemoveCellRow(HtmlNode rowNode)
        {
            CellRows.Remove(rowNode);
            Children.Remove(rowNode);
        }

        public void RemoveTitleRow(HtmlNode rowNode)
        {
            TitleRows.Remove(rowNode);
            Children.Remove(rowNode);
        }

        
        public HtmlTable(HtmlNode tableNode):base("table")//복사하여 테이블 속성만 가지고 있음.
        {
            if (tableNode.Name.ToLower().Equals("table") == false)
            {
                throw new Exception("the element [" + tableNode.Name + "] is not a table!");
            }
            Name = tableNode.Name;

            TitleRows = new List<HtmlNode>();
            CellRows = new List<HtmlNode>();
            
            foreach (HtmlNode child in tableNode.Children)
            {
                HtmlNode clone = child.Clone();
                Children.Add(clone);
                if (clone.Name.ToLower().Equals("tr"))
                {
                    bool isTitle = false;
                    foreach (HtmlNode td in clone.Children)
                    {
                        if (td.InnerText.IndexOf("[%H]") >= 0)
                        {
                            isTitle = true;
                            //td.InnerText = td.InnerText.Replace("[%H]", "");//없앰.
                            break;
                        }
                    }

                    if (isTitle)
                    {
                        TitleRows.Add(clone);
                    }
                    else
                    {
                        CellRows.Add(clone);
                    }
                }
            }
            
            foreach (String attr in tableNode.Attributes.Keys)
            {
                Attributes[attr] = tableNode.Attributes[attr];
            }
        }

        public override HtmlNode Clone()
        {
            HtmlTable table = new HtmlTable(this);
            return table;
        }

        /// <summary>
        /// child만 지운다.
        /// </summary>
        internal override void Clear()
        {
            Children.Clear();
            TitleRows.Clear();
            CellRows.Clear();
        }
        
    }

    /// <summary>
    /// table을 내부에 가지고 있는 태그. 테이블이 아니다.
    /// </summary>
    public class TableHolder : HtmlNode
    {
        

        HtmlTable _table = null;

        public HtmlTable Table
        {
            get
            {
                return _table;
            }
        }

        public override HtmlNode Clone()
        {
            //HtmlNode tempNode = base.Clone();//attribute와 chidren 복사.

            TableHolder holder = new TableHolder(this);
            /*
            holder.Attributes = new Dictionary<string, string>(Attributes);
            if (_innerText != null)
            {
                holder.InnerText = _innerText.Text;
            }
            
            foreach (HtmlNode child in Children)
            {
                holder.Children.Add(child.Clone());
            }
            holder._table = holder.GetChildNodeByName("table") as HtmlTable;

            if(_tableNameHolder.Count>0){
                 List<HtmlNode> nodes = new List<HtmlNode>();

                 holder.GetNodesByContent("[%TABLE_TITLE", nodes);
                 holder._tableNameHolder = nodes;
            }
            */
            return holder;
        }

       
        internal override void Clear()
        {
            base.Clear();
            _table = null;
            TableTitles.Clear();
        }

        private TableHolder(String name)
            : base(name)
        {

        }
        List<HtmlNode> _tableNameHolder = new List<HtmlNode>();
        public List<HtmlNode> TableTitles
        {
            get {
                return _tableNameHolder;
            }
        }
        public TableHolder(HtmlNode holderNode):base(holderNode.Name)
        {
            List<HtmlNode> funcNodes = new List<HtmlNode>();
            _tableNameHolder.Clear();
            SplitFunctionNodes(this, holderNode, funcNodes);
            foreach (HtmlNode child in Children)
            {
                if (child is HtmlTable)
                {
                    _table = child as HtmlTable;
                    break;
                }
            }
            foreach (HtmlNode node in funcNodes)
            {
                if (node.Children.Count==0 && node.InnerText.IndexOf("[%TABLE_TITLE") >= 0)
                {
                    _tableNameHolder.Add(node);
                }
            }
        }

       

       

        /// <summary>
        /// 테이블이 나뉘어졌으므로 테이블이 마치고 나와야 할 부분은 나오지 않아야 한다.
        /// </summary>
        internal void RemoveBottomOfTable()
        {
            List<HtmlNode> newChildren = new List<HtmlNode>();
            foreach(HtmlNode node in Children){
                newChildren.Add(node);
                if (node == Table) break;
            }
            _children = newChildren;
        }

        /// <summary>
        /// 테이블이 나뉘어졌으므로 이미 나온 Title은 나오지 않아야 한다.
        /// </summary>
        internal void RemoveTopOfTable()
        {
            List<HtmlNode> newChildren = new List<HtmlNode>();
            bool aboveOfTable = true;
            foreach (HtmlNode node in Children)
            {
                if (node == Table) aboveOfTable = false;
                if (aboveOfTable == false)
                {
                    newChildren.Add(node);
                }
            }
            _children = newChildren;
        }

        /// <summary>
        /// 테이블의 내용cell은 currentTable이 되는 순간 사라져야 한다.
        /// currentTableBase에서 복사해서 추가한다.
        /// </summary>
        internal void RemoveContentCells()
        {
            if (Table != null)
            {
                List<HtmlNode> rowsToRemove = new List<HtmlNode>();
                foreach (HtmlNode tr in Table.Children)
                {
                    if (Table.TitleRows.Contains(tr))
                    {
                    }
                    else
                    {
                        rowsToRemove.Add(tr);
                    }
                }
                foreach (HtmlNode tr in rowsToRemove)
                {
                    Table.Children.Remove(tr);
                }
                Table.CellRows.Clear();
            }
            
        }

        public int GetLineCountAfterTable()
        {
            int count = 0;
            foreach (HtmlNode node in Children)
            {
                
                if (node == Table) break;
                else count += node.GetLinesInNode();
            }
            return count;
        }

        public int GetLineCountBeforeTable()
        {
            int count = 0;
            bool aboveOfTable = true;
            foreach (HtmlNode node in Children)
            {
                if (node == Table) aboveOfTable = false;
                if (aboveOfTable == false)
                {
                    count += node.GetLinesInNode();
                }
            }
            return count;
        }

    }
}

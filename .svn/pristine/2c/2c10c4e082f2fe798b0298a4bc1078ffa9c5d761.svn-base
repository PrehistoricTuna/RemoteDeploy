using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace RemoteDeploy.View
{
    /// <summary>
    /// 委托-用作设置DataGridView中列的选中状态
    /// </summary>
    /// <param name="state"></param>
    public delegate void DataGridViewCheckBoxHeaderEventHander(bool state);

    /// <summary>
    /// 定义复选框列的选中状态
    /// </summary>
    public class datagridviewCheckboxHeaderEventArgs : EventArgs
    {
        public bool CheckedState{get;set;}

    }

    /// <summary>
    /// 重写DataGridView 增加全选按钮
    /// </summary>
    public class DataGridViewCheckBoxHeaderCell : DataGridViewColumnHeaderCell
    {
        System.Drawing.Point checkBoxLocation;
        System.Drawing.Size checkBoxSize;
        bool _checked = false;
        System.Drawing.Point _cellLocation = new System.Drawing.Point();
        System.Windows.Forms.VisualStyles.CheckBoxState _cbState =
            System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
        public event DataGridViewCheckBoxHeaderEventHander OnCheckBoxClicked;

        protected override void Paint(
            Graphics graphics,
            System.Drawing.Rectangle clipBounds,
            System.Drawing.Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates dataGridViewElementState,
            object value,
            object formattedValue,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex,
                dataGridViewElementState, value,
                formattedValue, errorText, cellStyle,
                advancedBorderStyle, paintParts);

            System.Drawing.Point p = new System.Drawing.Point();
            System.Drawing.Size s = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal);

            p.X = cellBounds.Location.X +
                (cellBounds.Width / 2) - (s.Width / 2) - 1;
            p.Y = cellBounds.Location.Y +
                (cellBounds.Height / 2) - (s.Height / 2);

            _cellLocation = cellBounds.Location;
            checkBoxLocation = p;
            checkBoxSize = s;
            if (_checked)
                _cbState = System.Windows.Forms.VisualStyles.
                    CheckBoxState.CheckedNormal;
            else
                _cbState = System.Windows.Forms.VisualStyles.
                    CheckBoxState.UncheckedNormal;

            CheckBoxRenderer.DrawCheckBox
            (graphics, checkBoxLocation, _cbState);
        }

        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            System.Drawing.Point p = new System.Drawing.Point(e.X + _cellLocation.X, e.Y + _cellLocation.Y);
            if (p.X >= checkBoxLocation.X && p.X <=
                checkBoxLocation.X + checkBoxSize.Width
            && p.Y >= checkBoxLocation.Y && p.Y <=
                checkBoxLocation.Y + checkBoxSize.Height)
            {
                _checked = !_checked;

                datagridviewCheckboxHeaderEventArgs ex = new datagridviewCheckboxHeaderEventArgs();
                ex.CheckedState = _checked;

                object sender = new object();

                if (OnCheckBoxClicked != null)
                {
                    OnCheckBoxClicked(_checked);
                    this.DataGridView.InvalidateCell(this);
                }
            }
            base.OnMouseClick(e);
        }
    }

}

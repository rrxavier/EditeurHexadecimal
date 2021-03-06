﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace editeurHexadecimal {

    /// <summary>
    /// View of the program
    /// </summary>
    public partial class frmMain : Form {
        HexaEditModel model;
        int firstRowIndex = 0; //index of the first row (in comparison with the whole data set, not just the part shown in the view
        Size previousSize; //Size of the form before it was resized
        bool firstTime = true; //Indicate that the program has just been launched and that we access this variable for the first time
        List<Point> redCellPosList = new List<Point>(); //List of the cells that must have a red font 
        const int NB_ROW = 17; // Number of rows shown in each dataGridView
        const int OFFSET_COLUMN_WIDTH = 55;

        /// <summary>
        /// Form constructor
        /// </summary>
        public frmMain() {
            InitializeComponent();
            firstTime = false;
        }

        /// <summary>
        /// frmMain loading, doesn't do much because the files will be loaded later
        /// </summary>
        /// <param name="sender">frmMain (event sender)</param>
        /// <param name="e">event argument</param>
        private void Form1_Load(object sender, EventArgs e) {
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(askRange);
            previousSize = new Size(this.Size.Width, this.Size.Height);
        }

        /// <summary>
        /// Click event of the button that let the user choose a file.
        /// Read the file and get it's content.
        /// Create and apply style to the rows, columns and cells of the dataGridViews
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenFile_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                model = new HexaEditModel(openFileDialog1.FileName);
                updateGridView();

                if (hexaGridView.Rows.Count > 0) {
                    hexaGridView.Columns[0].Width = OFFSET_COLUMN_WIDTH; //hexaGridView.Columns[0] = offset column
                    hexaGridView.Columns[0].ReadOnly = true;
                    hexaGridView.Columns[0].DefaultCellStyle.BackColor = Color.Gray;
                    hexaGridView.Columns[0].DefaultCellStyle.SelectionBackColor = Color.Gray;
                    hexaGridView.Columns[0].DefaultCellStyle.SelectionForeColor = SystemColors.ControlText;

                    setHexaDimension(true);
                    setAsciiDimension(true);

                    lblAttribute.Text = model.MyFileData.Attributes;
                    lblCreatedDate.Text = model.MyFileData.CreationTime.ToString();
                    lblLastAccess.Text = model.MyFileData.LastAccessTime.ToString();
                    lblModifyOn.Text = model.MyFileData.LastWriteTime.ToString();
                    lblName.Text = model.MyFileData.Name;
                    lblShortName.Text = model.MyFileData.ShortName;
                    lblSize.Text = model.MyFileData.FileSize.ToString();

                    asciiGridView.Rows[0].Cells[0].Selected = false; //It was automatically selected for an unknown reason

                    nupPaging.Maximum = model.Hexadecimal.Count() / NB_ROW;
                    lblMax.Text = (model.Hexadecimal.Count() / NB_ROW).ToString() + " )";
                    nupPaging.Enabled = true;
                    btnSave.Enabled = true;
                } else {
                    MessageBox.Show("Vous avez choisi un fichier vide");
                    nupPaging.Enabled = false;
                    btnSave.Enabled = false;
                }

                redCellPosList.Clear();
            }
        }

        /// <summary>
        /// Call the functions determining the dimension of the hexaGridView.
        /// Exist so the dev always set the width and height together when setting dimension.
        /// </summary>
        /// <param name="isInitializing">True if it is the first time we set the dimension</param>
        private void setHexaDimension(bool isInitializing = false) {
            setHexaColumnsWidth(isInitializing);
            setHexaRowsHeight();
        }

        /// <summary>
        /// Call the functions determining the dimension of the asciiGridView.
        /// Exist so the dev always set the width and height together when setting dimension.
        /// </summary>
        /// <param name="isInitializing">True if it is the first time we set the dimension</param>
        private void setAsciiDimension(bool isInitializing = false) {
            setAsciiColumnsWidth(isInitializing);
            setAsciiRowsHeight();
        }

        /// <summary>
        /// Set the width of the columns in the hexa table
        /// </summary>
        /// <param name="isInitializing">True if it is the first time we set the dimension</param>
        private void setHexaColumnsWidth(bool isInitializing = false) {
            //doesn't take into account the offset column because it has a set width
            int columnsWidth = (hexaGridView.Width - hexaGridView.Columns[0].Width) / 16;

            foreach (DataGridViewColumn column in hexaGridView.Columns) {
                if (isInitializing) { 
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    column.MinimumWidth = 22;
                }

                column.Width = columnsWidth;
            }

            //override the now false offset column width with the wanted value
            if(isInitializing)
                hexaGridView.Columns[0].MinimumWidth = OFFSET_COLUMN_WIDTH; 
        }

        /// <summary>
        /// Set the height of the rows in the hexa table
        /// </summary>
        private void setHexaRowsHeight() {
            int rowHeight = (hexaGridView.Height - hexaGridView.ColumnHeadersHeight) / NB_ROW;

            foreach (DataGridViewRow row in hexaGridView.Rows) 
                row.Height = rowHeight;
        }

        /// <summary>
        /// Set the height of the rows in the ascii table
        /// </summary>
        private void setAsciiRowsHeight() {
            int rowHeight = asciiGridView.Height / 17;

            foreach (DataGridViewRow row in asciiGridView.Rows)
                row.Height = rowHeight;
        }

        /// <summary>
        /// Set the width of the columns in the ascii table
        /// </summary>
        /// <param name="isInitializing">True if it is the first time we set the dimension</param>
        private void setAsciiColumnsWidth(bool isInitializing = false) {
            int columnsWidth = asciiGridView.Width / 16;

            foreach (DataGridViewColumn column in asciiGridView.Columns) {
                if (isInitializing) {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    column.MinimumWidth = 22;
                }

                column.Width = columnsWidth;
            }
        }

        /// <summary>
        /// Ask for the current plage of data
        /// </summary>
        private void updateGridView() {
            hexaGridView.DataSource = model.GetHexaDataTable(firstRowIndex, NB_ROW);
            asciiGridView.DataSource = model.GetAsciiDataTable(firstRowIndex, NB_ROW);

            if(asciiGridView.Rows.Count > 0)
                asciiGridView.Rows[0].Cells[0].Selected = false; //It was automatically selected for an unknown reason

            updateCellForeColor();
        }

        /// <summary>
        /// Ask for the next page of data (taking the direction of the mouse wheel scroll into account)
        /// and update the view to show it
        /// </summary>
        /// <param name="down">True if the scroll is going down, ask for the next set of data (instead of precedent)</param>
        private void updateGridView(bool down) {
            if (down)
                firstRowIndex += NB_ROW;
            else if(!down)
                firstRowIndex -= NB_ROW;

            hexaGridView.DataSource = model.GetHexaDataTable(firstRowIndex, NB_ROW);
            asciiGridView.DataSource = model.GetAsciiDataTable(firstRowIndex, NB_ROW);
            asciiGridView.Rows[0].Cells[0].Selected = false; //It was automatically selected for an unknown reason

            updateCellForeColor();
        }

        /// <summary>
        /// Check in which direction the mouse wheel scroll is going
        /// </summary>
        /// <param name="sender">The mouse sending the event</param>
        /// <param name="e">Arguments of the event</param>
        private void askRange(object sender, MouseEventArgs e) {
            if (e.Delta < 0) //scroll down
                updateGridView(true);
            else if(firstRowIndex - NB_ROW >= 0)
                updateGridView(false);
        }

        /// <summary>
        /// Call a function to update the labels with info on the content of the clicked cell
        /// And highlight it's corresponding cell in the other table
        /// </summary>
        /// <param name="sender">The gridview sending the event</param>
        /// <param name="e">arguments of the event</param>
        private void gridView_CellClick(object sender, DataGridViewCellEventArgs e) {
            //Regarding -1/+1 : There is one less column in the ascii table

            if (((DataGridView)sender).Name == "hexaGridView") {
                if (e.ColumnIndex != 0 && e.RowIndex != -1) { //column 0 and row -1 are offset 
                    updateLabel(new Point(e.ColumnIndex, e.RowIndex + firstRowIndex), false);
                    asciiGridView.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].Selected = true;
                }
            } else if (((DataGridView)sender).Name == "asciiGridView") {
                updateLabel(new Point(e.ColumnIndex + 1, e.RowIndex + firstRowIndex), true);
                hexaGridView.Rows[e.RowIndex].Cells[e.ColumnIndex + 1].Selected = true; 
            }
        }

        /// <summary>
        /// Update some label with the converted value of the content of the clicked cell in other unit
        /// </summary>
        /// <param name="cellPt">Position of the click cell (comparing to the entire table, not only to the shown part in the view)</param>
        /// <param name="asciiTable">True if it is the ascii table that has been clicked</param>
        private void updateLabel(Point cellPt, bool asciiTable){
            //Regarding -1 : There is one less column in the ascii table

            //Get the ascii equivalent of the byte in the ascii table
            lblChar.Text = model.GetAsciiDataTable().Rows[cellPt.Y][cellPt.X - 1].ToString();

            lblBinary.Text = model.ConvertHexaToBinary(cellPt);
            lblOctal.Text = model.ConvertHexaToOctal(cellPt);
            lbl8BitsSigned.Text = model.ConvertHexaTo8BitsSigned(cellPt);
            lbl8BitsNS.Text = model.ConvertHexaTo8BitsUnsigned(cellPt);
            lbl16BitSigned.Text = model.ConvertHexaTo16BitsSigned(cellPt);
            lbl16BitsNS.Text = model.ConvertHexaTo16BitsUnsigned(cellPt);
            lbl32BitsSigned.Text = model.ConvertHexaTo32BitsSigned(cellPt);
            lbl32BitsNS.Text = model.ConvertHexaTo32BitsUnsigned(cellPt);
            lbl64BitsSigned.Text = model.ConvertHexaTo64BitsSigned(cellPt);
            lblFloat.Text = model.ConvertHexaToFloat(cellPt);
            lblDouble.Text = model.ConvertHexaToDouble(cellPt);

            tctrlData.SelectedIndex = 1; //Show the tab with these labels
        }

        /// <summary>
        /// Try to make the form and it's content responsive
        /// </summary>
        /// <param name="sender">The form sending the event</param>
        /// <param name="e">Argument of the event</param>
        private void Form1_SizeChanged(object sender, EventArgs e) {
            //This event is called when the form load the first time, but we musn't resize it then
            if (!firstTime) { 
                frmMain form = sender as frmMain;
                int heightDiff = form.Size.Height - previousSize.Height;
                int widthDiff = form.Size.Width - previousSize.Width;

                hexaGridView.Height += (heightDiff / 2) + (heightDiff % 2);
                hexaGridView.Width += (widthDiff / 2) + (widthDiff % 2);

                asciiGridView.Height += heightDiff / 2 + (heightDiff % 2);
                asciiGridView.Width += widthDiff / 2 + (widthDiff % 2);

                //always 20px after hexaGridView
                asciiGridView.Location = new Point(hexaGridView.Location.X + hexaGridView.Width + 20, asciiGridView.Location.Y);

                previousSize = new Size(form.Size.Width, form.Size.Height);

                DataGridViewCellStyle hexaDefaultCellStyle = hexaGridView.DefaultCellStyle;
                DataGridViewCellStyle asciiDefaultCellStyle = asciiGridView.DefaultCellStyle;

                if (heightDiff < 0 || widthDiff < 0) {
                    if (hexaDefaultCellStyle.Font.Size - 0.25F >= 8.25F) { 
                        hexaDefaultCellStyle.Font = new Font(hexaDefaultCellStyle.Font.FontFamily, hexaDefaultCellStyle.Font.Size - 0.25F);
                        asciiDefaultCellStyle.Font = new Font(hexaDefaultCellStyle.Font.FontFamily, hexaDefaultCellStyle.Font.Size - 0.25F);
                    }
                } else {
                    if (hexaDefaultCellStyle.Font.Size + 0.25F <= 12F) {
                        hexaDefaultCellStyle.Font = new Font(hexaDefaultCellStyle.Font.FontFamily, hexaDefaultCellStyle.Font.Size + 0.25F);
                        asciiDefaultCellStyle.Font = new Font(hexaDefaultCellStyle.Font.FontFamily, hexaDefaultCellStyle.Font.Size + 0.25F);
                    }
                }

                setHexaDimension();
                setAsciiDimension();

                lblNupInfo1.Location = new Point(lblNupInfo1.Location.X, hexaGridView.Location.Y + 20 + hexaGridView.Height);
                lblNupInfo2.Location = new Point(lblNupInfo2.Location.X, hexaGridView.Location.Y + 20 + hexaGridView.Height);

                // Still kind of buggy, work of we move slowy but not otherwise, doesn't fit perfectly in the space available.
                //Maybe add to the offset the remaining space and correct the division (not sur they're good).
            }
        }

        /// <summary>
        /// Update the table with the edited data from the user
        /// </summary>
        /// <param name="sender">The datagridview that send the event</param>
        /// <param name="e">Arguments of the event</param>
        public void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
            DataGridView dgv = sender as DataGridView;
            Point cellPos = new Point(e.ColumnIndex, e.RowIndex + firstRowIndex);

            if (dgv.Name == "hexaGridView")
            {
                // test that the user didn't erase the content of the cell or write something longer than 2 character 
                if (hexaGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() != String.Empty && hexaGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Length <= 2) {
                    updateBeforeEdit(e.RowIndex, e.ColumnIndex, cellPos);
                    model.ChangeValueHex(cellPos, hexaGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                    updateAfterEdit(e.RowIndex, e.ColumnIndex, cellPos);
                    
                } else {
                    updateGridView(); // cancel user deletion
                }
            } else {
                // test that the user didn't erase the content of the cell or write something longer than 1 character 
                if (asciiGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() != String.Empty && asciiGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Length <= 1) {
                    updateBeforeEdit(e.RowIndex, e.ColumnIndex + 1, new Point(e.ColumnIndex + 1, e.RowIndex));
                    model.ChangeValueAscii(cellPos, Convert.ToChar(asciiGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value));
                    updateAfterEdit(e.RowIndex, e.ColumnIndex + 1, new Point(e.ColumnIndex + 1, e.RowIndex));
                } else {
                    updateGridView(); // cancel user deletion
                }
            }

            //TO_DO: there's still 1 bug : when we click on another cell while still editing one, the programm crash
        }

        /// <summary>
        /// Update the color of the edited cell and keep their location in a list
        /// </summary>
        /// <param name="rowIndex">Row index of the edited cell (in the view)</param>
        /// <param name="columnIndex">Column index of the edited cell (in the view)</param>
        /// <param name="cellPos">Cell position (regarding the entire table and not just the view)</param>
        private void updateBeforeEdit(int rowIndex, int columnIndex, Point cellPos) {
            if (hexaGridView.Rows[rowIndex].Cells[columnIndex].Value.ToString() != model.Hexadecimal[rowIndex][columnIndex].ToString())
            {
                hexaGridView.Rows[rowIndex].Cells[columnIndex].Style.ForeColor = Color.Red;
                asciiGridView.Rows[rowIndex].Cells[columnIndex - 1].Style.ForeColor = Color.Red;
                redCellPosList.Add(cellPos);
            }
        }

        /// <summary>
        /// update the value of the 2 cells (in hexaGridView & asciiGridView with the new one)
        /// </summary>
        /// <param name="rowIndex">Row index of the edited cell (in the view)</param>
        /// <param name="columnIndex">Column index of the edited cell (in the view)</param>
        /// <param name="cellPos">Cell position (regarding the entire table and not just the view)</param>
        private void updateAfterEdit(int rowIndex, int columnIndex, Point cellPos)
        {
            hexaGridView.Rows[rowIndex].Cells[columnIndex].Value = model.Hexadecimal[rowIndex][columnIndex].ToString();
            asciiGridView.Rows[rowIndex].Cells[columnIndex - 1].Value = model.GetAsciiDataTable().Rows[rowIndex].ItemArray[columnIndex - 1].ToString();
        }

        /// <summary>
        /// Check if there was any edited data in the newly shown page and color them if there is
        /// </summary>
        private void updateCellForeColor() {
            foreach (DataGridViewRow row in hexaGridView.Rows)
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (redCellPosList.Contains(new Point(cell.ColumnIndex, cell.RowIndex + firstRowIndex)))
                    {
                        cell.Style.ForeColor = Color.Red;
                        asciiGridView.Rows[cell.RowIndex].Cells[cell.ColumnIndex - 1].Style.ForeColor = Color.Red;
                    }
                }
        }

        /// <summary>
        /// Save the changes made to the file
        /// </summary>
        /// <param name="sender">The clicked button</param>
        /// <param name="e">Argument of the event</param>
        private void btnSave_Click(object sender, EventArgs e) {
            if (DialogResult.OK == saveFileDialog1.ShowDialog()) {
                if (model != null)
                    model.Save(saveFileDialog1.FileName);
            }
        }

        /// <summary>
        /// Update the gridviwe with the desired set of data given by the numeric up down component value
        /// </summary>
        /// <param name="sender">The numeric up down component whose had it's value changed</param>
        /// <param name="e">Argument of the event</param>
        private void nupPaging_ValueChanged(object sender, EventArgs e) {
            firstRowIndex = (int)nupPaging.Value * NB_ROW;
            updateGridView();
        }

        /// <summary>
        /// Return the cell to it's original value if the delete or backspace keys are pressed
        /// </summary>
        /// <param name="sender">The DatagridView on which a key has been pressed</param>
        /// <param name="e">Arguments of the event</param>
        private void gridView_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Delete || e.KeyChar == (char)Keys.Back) {
                if ((sender as DataGridView).Name == "hexaGridView") {
                    DataGridViewCell clickedCell = hexaGridView.SelectedCells[0];
                    Point cellPoint = new Point(clickedCell.ColumnIndex, clickedCell.RowIndex);
                    model.UndoChange(cellPoint);
                    redCellPosList.Remove(cellPoint);
                } else {
                    DataGridViewCell clickedCell = asciiGridView.SelectedCells[0];
                    Point cellPoint = new Point(clickedCell.ColumnIndex + 1, clickedCell.RowIndex);
                    model.UndoChange(cellPoint);
                    redCellPosList.Remove(cellPoint);
                }

                updateGridView();
            }
        }

    }
}

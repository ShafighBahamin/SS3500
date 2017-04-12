﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;

namespace SS {
  public partial class SSForm : Form {
    // Spreadsheet Data associated with current Form
    private Spreadsheet personalSpreadsheet;
    private string fileName = null;

    /// <summary>
    /// Constructor for new blank Form
    /// </summary>
    public SSForm() {
      // When a new Form is created, a server connection is required
      ConnectionDialogBox(); 

      InitializeComponent();
      personalSpreadsheet = new Spreadsheet(validAddress, s => s.ToUpper(), "PS6");
      this.Text = "NewSpreadsheet";
      addressBox.Text = "A1";


    }

    /// <summary>
    /// Opens an existing Spreadsheet from a file.
    /// The constuctor will update the GUI on what needs to be displayed.
    /// </summary>
    /// <param name="filePath">Path and name of file</param>
    public SSForm(string filePath) {
      this.fileName = filePath;
      InitializeComponent();

      personalSpreadsheet = new Spreadsheet(filePath, validAddress, s => s.ToUpper(), "PS6");

      int col, row;
      object cellVal;
      foreach (string cellName in personalSpreadsheet.GetNamesOfAllNonemptyCells())//Assigns the string rep for each cell that is not empty in spreadsheet.
      {
        addressToGrid(cellName, out col, out row);// Get the current address based on cell name. 
        cellVal = personalSpreadsheet.GetCellValue(cellName);
        spreadsheetPanel1.SetValue(col, row, cellVal.ToString());// Set the value of cell using grid address. 

      }
      string focusCellValue;
      spreadsheetPanel1.GetValue(0, 0, out focusCellValue);//Gets the starting positions value. 
      valueBox.Text = focusCellValue; // Displays the default's value. 
      addressBox.Text = "A1";
      contentBox.Text = personalSpreadsheet.GetCellContents(gridToAddress(0, 0)).ToString();
      fileName = filePath;
      this.Text = fileName;

    }

    /// <summary>
    /// Displays a Connection Dialog Box
    /// </summary>
    /// <returns>the name of the file to open</returns>
    private string ConnectionDialogBox() {
      string filename = null;
      // open a connection dialog box (make a new custom form?)
      //  -> request input: server name
      // attempt to connect to server
      // upon success: open filename dialog box
      //  -> request input: filename
      // return the filename

      return filename;
    }

    /// <summary>
    /// Given the address to a cell; it is converted and returns
    ///     the row and column to that cell in zero-index version.
    ///     (ie: "B71" -> (1,70))
    /// </summary>
    /// <param name="address">Cell Address</param>
    /// <param name="row">Row of Cell (zero indexed)</param>
    /// <param name="col">column of Cell (zero indexed)</param>
    private void addressToGrid(string address, out int col, out int row) {
      char colChar = address[0]; // get first character of string
      col = (int)colChar - 65;
      string rowStr = address.TrimStart(colChar); // get integer portion of string
      row = int.Parse(rowStr) - 1;
    }

    /// <summary>
    /// Given a row and column; convert to address form (ie: (5,11) -> "F12")
    /// </summary>
    /// <param name="row">Row containing Cell</param>
    /// <param name="col">Column containing Cell</param>
    /// <returns>Address relating to Cell</returns>
    private string gridToAddress(int col, int row) {
      string address = char.ConvertFromUtf32(col + 65); // convert Column
      address += (row + 1).ToString();    // Convert Row

      return address;

    }

    /// <summary>
    /// Determines if the address exists on the spreadsheet
    ///     -> intended for IsValid delegate
    /// </summary>
    /// <param name="address">Address Attempt</param>
    /// <returns>Leagal Address Boolean</returns>
    private bool validAddress(string address) {
      if (Regex.IsMatch(address, @"(^[a-zA-Z][1-9][0-9]?$)"))
        return true;
      else
        throw new InvalidNameException();
    }


    /// <summary>
    /// When you select New it will create an empty spreadsheet windows application. 
    /// Using the first SSForm constructor. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void newToolStripMenuItem_Click(object sender, EventArgs e) {
      SpreadsheetContext.getAppContext().RunForm(new SSForm());

    }

    /// <summary>
    /// When selected it will Close down the current Spreadsheet Windows Application Form. 
    /// If the current form is the last one open it will shut down the application. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void closeToolStripMenuItem_Click(object sender, EventArgs e) {
      Close();
    }

    /// <summary>
    /// Closes all Open Spreadsheets
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
      Application.Exit();
    }

    /// <summary>
    /// If unsaved data is detected, this give the user a chance to cancel the closing procedure
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SSForm_FormClosing(object sender, FormClosingEventArgs e) {
      if (personalSpreadsheet.Changed || (fileName != null && !System.IO.File.Exists(fileName))) {
        DialogResult askClose = MessageBox.Show("Spreadsheet contains unsaved data. Are you sure you want to close?",
            "Unsaved Data Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (askClose != DialogResult.Yes) {
          e.Cancel = true;
        }
      }
    }

    /// <summary>
    /// Selecting Open the app will load up the selected spreadsheet.
    ///     After the spreadsheet has been made from the constructor,
    ///     The panel will get all cells that have content in the spreadsheet
    ///     and draw them onto the GUI. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void openToolStripMenuItem_Click(object sender, EventArgs e) {
      using (OpenFileDialog OFD = new OpenFileDialog()) {
        OFD.Filter = "SPRD(*.sprd)|*.sprd|All Files|*.*";
        OFD.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // Shows the documents folder for the system.
        if (OFD.ShowDialog() == DialogResult.OK)//Will not do work if cancel is selected instead.
        {
          string version = personalSpreadsheet.GetSavedVersion(OFD.FileName);
          if (version != "PS6") {
            DialogResult badOpen = MessageBox.Show("Spreadsheet contains conflict with version information or format.",
            "Open File Failure Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          } else
            SpreadsheetContext.getAppContext().RunForm(new SSForm(OFD.FileName));
        }

      }
    }

    /// <summary>
    /// Will populate the banner with content if it is present and highlight the text 
    /// so that it can be edited. If there is no content to be found then the focus is 
    /// redirected to the content box without displaying anything. 
    /// </summary>
    /// <param name="sender"></param>
    private void spreadsheetPanel1_SelectionChanged(SpreadsheetPanel sender) {
      contentBox.ResetText();

      int col, row;
      string address;
      string value;
      string content = contentBox.Text;
      //Gets the currently selected cell's location and Value. 
      spreadsheetPanel1.GetSelection(out col, out row);

      if (spreadsheetPanel1.GetValue(col, row, out value)) {
        //Assuming the cell now has a value it displays the adress and corresponding value. 
        addressBox.Text = address = gridToAddress(col, row);
        valueBox.Text = value;

        if (personalSpreadsheet.GetCellContents(address) is Formula)//Checks if the content is of type formula. 
          contentBox.Text = "=" + personalSpreadsheet.GetCellContents(address).ToString();
        else
          contentBox.Text = personalSpreadsheet.GetCellContents(address).ToString();

        contentBox.SelectAll();//Highlights the text inside the banner. 
      }
      contentBox.Focus();
    }

    /// <summary>
    /// When the Help menu is pulled down and About is selected it will display information.
    /// This information will instruct the user how to work the Spreadsheet Windows Application Form. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
      MessageBox.Show("Below are instructions and features for the Spreadsheet Application: \n\n" +
          "-Use arrow keys: Right,Left,Up,Down to traverse the spreadsheet. \n" +
          "-Selecting a cell with your mouse will highlight the cell to be changed. \n" +
          "-Additional features include: Exit,Save As, Menu Item Hotkeys, and a status update included in the application form name. \n\n " +
          "-Save As will open a built in File Dialog to chose a file/location to save the file. \n" +
          "-Exit will close out of all open windows and shut off the applicaiton.\n" +
          "-Menu Item Hotkeys can be found to the right of the action they corrispond too. \n" +
          "-To maximize and or revert the size of the screen use hotkey F11.",
          "Spreadsheet Information and Controls",
          MessageBoxButtons.OK, MessageBoxIcon.None);//Can choose what kind of symbol is displayed. 
    }

    /// <summary>
    /// Basic functionality of setting a cell's contents
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContentButton_Click(object sender, EventArgs e) {
      int col, row;
      spreadsheetPanel1.GetSelection(out col, out row);
      string address = gridToAddress(col, row);
      string content = contentBox.Text;
      HashSet<string> cellsToUpdate = null;

      try {
        // set the contents and determine cells to recalculate
        cellsToUpdate = (HashSet<string>)personalSpreadsheet.SetContentsOfCell(address, content);
        foreach (string cell in cellsToUpdate) {
          //get col, row
          addressToGrid(cell, out col, out row);

          object value = personalSpreadsheet.GetCellValue(cell);
          if (value is FormulaError) {
            value = ((FormulaError)value).Reason; // display the reason for error
          }
          //set value to display at cell
          spreadsheetPanel1.SetValue(col, row, value.ToString());

        }
        addressBox.Text = address;
        string cellVal;
        addressToGrid(address, out col, out row);
        spreadsheetPanel1.GetValue(col, row, out cellVal);
        valueBox.Text = cellVal;

        if (personalSpreadsheet.Changed && (!Regex.IsMatch(this.Text, @"(\(unsaved\))$")))
          this.Text = this.Text + " (unsaved)";
      }
      catch (Exception err) //something went wrong while setting the contents
      {
        MessageBox.Show(err.Message, "Error Detected!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        contentBox.SelectAll();
        contentBox.Focus();
      }

    }

    /// <summary>
    /// User selects to Save Current Spreadsheet.
    ///     If Spreadsheet already has FileName, save to that filename
    ///     If Spreadsheet is untitled, run SaveAs method.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
      if (fileName != null) // if the file has been saved before...
      {
        if (System.IO.File.Exists(fileName)) // check if the file is still there
        {
          personalSpreadsheet.Save(fileName);
          this.Text = fileName;
        } else  // if not, call saveAs
         {
          saveAsToolStripMenuItem_Click(sender, e);
        }
      } else // if not, call saveAs
       {
        saveAsToolStripMenuItem_Click(sender, e);
      }
    }

    /// <summary>
    /// Shows a File Dialog to allow user to define a file name and path.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) {
      using (SaveFileDialog SFD = new SaveFileDialog()) // Show new SaveFileDialog
      {
        SFD.Filter = "SPRD(*.sprd)|*.sprd|All Files|*.*";
        SFD.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (SFD.ShowDialog() == DialogResult.OK) // Filepath selected
        {
          if (SFD.CheckPathExists) // validate path is on machine
          {
            if (!SFD.CheckFileExists) // if file doesn't exist, go ahead and save
            {
              fileName = SFD.FileName;
              personalSpreadsheet.Save(fileName);
            }

            this.Text = fileName;
          }
        }

      }
    }

    /// <summary>
    /// Looks for key presses while in the content dialog box
    ///     Enter: calls the content button method
    ///     Others can be added for more features.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void contentBox_KeyDown(object sender, KeyEventArgs e) {
      int col, row;
      switch (e.KeyCode) {
        case Keys.Enter: // accept contents change
          ContentButton_Click(sender, e);
          e.Handled = true;
          e.SuppressKeyPress = true;
          break;

        case Keys.Up://Change grid to location one row above selected, unless it's at the top(0) row. 
          spreadsheetPanel1.GetSelection(out col, out row);
          if (row <= 0)
            row = 0;
          else
            row--;
          spreadsheetPanel1.SetSelection(col, row);
          spreadsheetPanel1_SelectionChanged(spreadsheetPanel1);
          e.Handled = true;
          e.SuppressKeyPress = true;
          break;

        case Keys.Down://Change grid to location one row below selected, unless it's at the botton(99) row. 
          spreadsheetPanel1.GetSelection(out col, out row);
          if (row >= 99)
            row = 99;
          else
            row++;
          spreadsheetPanel1.SetSelection(col, row);
          spreadsheetPanel1_SelectionChanged(spreadsheetPanel1);
          e.Handled = true;
          e.SuppressKeyPress = true;
          break;

        case Keys.Left://Change grid to location one col left of selected, unless it's at the left most collum(A/0).
          spreadsheetPanel1.GetSelection(out col, out row);
          if (col <= 0)
            col = 0;
          else
            col--;
          spreadsheetPanel1.SetSelection(col, row);
          spreadsheetPanel1_SelectionChanged(spreadsheetPanel1);
          e.Handled = true;
          e.SuppressKeyPress = true;
          break;

        case Keys.Right://Change grid to location one col right of selected, unless it's at the right most collum(Z/26).
          spreadsheetPanel1.GetSelection(out col, out row);
          if (col >= 26)
            col = 26;
          else
            col++;
          spreadsheetPanel1.SetSelection(col, row);
          spreadsheetPanel1_SelectionChanged(spreadsheetPanel1);
          e.Handled = true;
          e.SuppressKeyPress = true;
          break;

        case Keys.F11:
          if (!(this.WindowState == FormWindowState.Maximized))
            this.WindowState = FormWindowState.Maximized;
          else
            this.WindowState = FormWindowState.Normal;
          break;
      }
    }

    /// <summary>
    /// If/When spreadsheetPannel ever gets focus, immediately give focs to the contentBox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void spreadsheetPanel1_Enter(object sender, EventArgs e) {
      contentBox.SelectAll();
      contentBox.Focus();
    }
  }
}

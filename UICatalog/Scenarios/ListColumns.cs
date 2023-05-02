﻿using System;
using System.Collections.Generic;
using System.Data;
using Terminal.Gui;
using static Terminal.Gui.ListColumnView;
using static Terminal.Gui.TableView;
using System.Collections;

namespace UICatalog.Scenarios {

	[ScenarioMetadata (Name: "ListColumns", Description: "Implements data table using the ListColumnView control.")]
	[ScenarioCategory ("TableView")]
	[ScenarioCategory ("Controls")]
	[ScenarioCategory ("Dialogs")]
	[ScenarioCategory ("Text and Formatting")]
	[ScenarioCategory ("Top Level Windows")]
	public class ListColumns : Scenario {
		ListColumnView listColView;
		DataTable currentTable;
		private MenuItem miCellLines;
		private MenuItem miExpandLastColumn;
		private MenuItem miAlwaysUseNormalColorForVerticalCellLines;
		private MenuItem miSmoothScrolling;
		private MenuItem miAlternatingColors;
		private MenuItem miCursor;
		private MenuItem miTopline;
		private MenuItem miBottomline;
		private MenuItem miOrientVertical;
		private MenuItem miScrollParallel;

		ColorScheme alternatingColorScheme;

		public override void Setup ()
		{
			Win.Title = this.GetName ();
			Win.Y = 1; // menu
			Win.Height = Dim.Fill (1); // status bar
			Application.Top.LayoutSubviews ();

			this.listColView = new ListColumnView () {
				X = 0,
				Y = 0,
				Width = Dim.Fill (),
				Height = Dim.Fill (1),
				Style = new TableStyle {
					ShowHeaders = false,
					ShowHorizontalHeaderOverline = false,
					ShowHorizontalHeaderUnderline = false,
					ShowHorizontalBottomline = false
				}
			};

			var menu = new MenuBar (new MenuBarItem [] {
				new MenuBarItem ("_File", new MenuItem [] {
					new MenuItem ("Open_ListExample", "", () => OpenSimpleList (true)),
					new MenuItem ("_CloseExample", "", () => CloseExample ()),
					new MenuItem ("_Quit", "", () => Quit()),
				}),
				new MenuBarItem ("_View", new MenuItem [] {
					miTopline = new MenuItem ("_TopLine", "", () => ToggleTopline ()) { Checked = listColView.Style.ShowHorizontalHeaderOverline, CheckType = MenuItemCheckStyle.Checked },
					miBottomline = new MenuItem ("_BottomLine", "", () => ToggleBottomline ()) { Checked = listColView.Style.ShowHorizontalBottomline, CheckType = MenuItemCheckStyle.Checked },
					miCellLines = new MenuItem ("_CellLines", "", () => ToggleCellLines ()) { Checked = listColView.Style.ShowVerticalCellLines, CheckType = MenuItemCheckStyle.Checked },
					miExpandLastColumn = new MenuItem ("_ExpandLastColumn", "", () => ToggleExpandLastColumn ()) { Checked = listColView.Style.ExpandLastColumn, CheckType = MenuItemCheckStyle.Checked },
					miAlwaysUseNormalColorForVerticalCellLines = new MenuItem ("_AlwaysUseNormalColorForVerticalCellLines", "", () => ToggleAlwaysUseNormalColorForVerticalCellLines ()) { Checked = listColView.Style.AlwaysUseNormalColorForVerticalCellLines, CheckType = MenuItemCheckStyle.Checked },
					miSmoothScrolling = new MenuItem ("_SmoothHorizontalScrolling", "", () => ToggleSmoothScrolling ()) { Checked = listColView.Style.SmoothHorizontalScrolling, CheckType = MenuItemCheckStyle.Checked },
					miAlternatingColors = new MenuItem ("Alternating Colors", "", () => ToggleAlternatingColors ()) { CheckType = MenuItemCheckStyle.Checked},
					miCursor = new MenuItem ("Invert Selected Cell First Character", "", () => ToggleInvertSelectedCellFirstCharacter ()) { Checked = listColView.Style.InvertSelectedCellFirstCharacter,CheckType = MenuItemCheckStyle.Checked},
				}),
				new MenuBarItem ("_List", new MenuItem [] {
					//new MenuItem ("_Hide Headers", "", HideHeaders),
					miOrientVertical = new MenuItem ("_OrientVertical", "", () => ToggleVerticalOrientation ()) { Checked = listColView.ListStyle.VerticalOrientation, CheckType = MenuItemCheckStyle.Checked },
					miScrollParallel = new MenuItem ("_ScrollParallel", "", () => ToggleScrollParallel ()) { Checked = listColView.ListStyle.ScrollParallel, CheckType = MenuItemCheckStyle.Checked },
					new MenuItem ("Set _Max Cell Width", "", SetListMaxWidth),
					new MenuItem ("Set Mi_n Cell Width", "", SetListMinWidth),
				}),
			});

			Application.Top.Add (menu);

			var statusBar = new StatusBar (new StatusItem [] {
				new StatusItem(Key.F2, "~F2~ OpenListExample", () => OpenSimpleList (true)),
				new StatusItem(Key.F3, "~F3~ CloseExample", () => CloseExample ()),
				new StatusItem(Application.QuitKey, $"{Application.QuitKey} to Quit", () => Quit()),
			});
			Application.Top.Add (statusBar);

			Win.Add (listColView);

			var selectedCellLabel = new Label () {
				X = 0,
				Y = Pos.Bottom (listColView),
				Text = "0,0",
				Width = Dim.Fill (),
				TextAlignment = TextAlignment.Right

			};

			Win.Add (selectedCellLabel);

			listColView.SelectedCellChanged += (s, e) => { selectedCellLabel.Text = $"{listColView.SelectedRow},{listColView.SelectedColumn}"; };
			listColView.KeyPress += TableViewKeyPress;

			SetupScrollBar ();

			alternatingColorScheme = new ColorScheme () {

				Disabled = Win.ColorScheme.Disabled,
				HotFocus = Win.ColorScheme.HotFocus,
				Focus = Win.ColorScheme.Focus,
				Normal = Application.Driver.MakeAttribute (Color.White, Color.BrightBlue)
			};

			// if user clicks the mouse in TableView
			listColView.MouseClick += (s, e) => {

				listColView.ScreenToCell (e.MouseEvent.X, e.MouseEvent.Y, out int? clickedCol);

				/*
				if (clickedCol != null) {
					if (e.MouseEvent.Flags.HasFlag (MouseFlags.Button1Clicked)) {

						// left click in a header
						SortList (clickedCol);
					} else if (e.MouseEvent.Flags.HasFlag (MouseFlags.Button3Clicked)) {

						// right click in a header
						ShowHeaderContextMenu (clickedCol, e);
					}
				}
				*/
			};

			listColView.AddKeyBinding (Key.Space, Command.ToggleChecked);
		}

		// Disable sorting for now
		/*
		private void SortList (DataColumn clickedCol)
		{
			var sort = GetProposedNewSortOrder (clickedCol, out var isAsc);

			SortList (clickedCol, sort, isAsc);
		}

		private void SortList (DataColumn clickedCol, string sort, bool isAsc)
		{
			// set a sort order
			listColView.Table.DefaultView.Sort = sort;

			// copy the rows from the view
			var sortedCopy = listColView.Table.DefaultView.ToTable ();
			listColView.Table.Rows.Clear ();
			foreach (DataRow r in sortedCopy.Rows) {
				listColView.Table.ImportRow (r);
			}

			foreach (DataColumn col in listColView.Table.Columns) {

				// remove any lingering sort indicator
				col.ColumnName = TrimArrows (col.ColumnName);

				// add a new one if this the one that is being sorted
				if (col == clickedCol) {
					col.ColumnName += isAsc ? '▲' : '▼';
				}
			}

			listColView.ListUpdate ();
		}

		private string TrimArrows (string columnName)
		{
			return columnName.TrimEnd ('▼', '▲');
		}
		private string StripArrows (string columnName)
		{
			return columnName.Replace ("▼", "").Replace ("▲", "");
		}
		private string GetProposedNewSortOrder (DataColumn clickedCol, out bool isAsc)
		{
			// work out new sort order
			var sort = listColView.Table.DefaultView.Sort;

			if (sort?.EndsWith ("ASC") ?? false) {
				sort = $"{clickedCol.ColumnName} DESC";
				isAsc = false;
			} else {
				sort = $"{clickedCol.ColumnName} ASC";
				isAsc = true;
			}

			return sort;
		}

		private void ShowHeaderContextMenu (DataColumn clickedCol, MouseEventEventArgs e)
		{
			var sort = GetProposedNewSortOrder (clickedCol, out var isAsc);

			var contextMenu = new ContextMenu (e.MouseEvent.X + 1, e.MouseEvent.Y + 1,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ($"Sort {StripArrows(sort)}","",()=>SortList(clickedCol,sort,isAsc)),
				})
			);

			contextMenu.Show ();
		}

		private DataColumn GetColumn ()
		{
			if (listColView.Table == null)
				return null;

			if (listColView.SelectedColumn < 0 || listColView.SelectedColumn > listColView.Table.Columns.Count)
				return null;

			return listColView.Table.Columns [listColView.SelectedColumn];
		}
		*/

		private void SetupScrollBar ()
		{
			var _scrollBar = new ScrollBarView (listColView, true); // (listColView, true, true);

			_scrollBar.ChangedPosition += (s, e) => {
				listColView.RowOffset = _scrollBar.Position;
				if (listColView.RowOffset != _scrollBar.Position) {
					_scrollBar.Position = listColView.RowOffset;
				}
				listColView.SetNeedsDisplay ();
			};
			/*
			_scrollBar.OtherScrollBarView.ChangedPosition += (s,e) => {
				listColView.ColumnOffset = _scrollBar.OtherScrollBarView.Position;
				if (listColView.ColumnOffset != _scrollBar.OtherScrollBarView.Position) {
					_scrollBar.OtherScrollBarView.Position = listColView.ColumnOffset;
				}
				listColView.SetNeedsDisplay ();
			};
			*/

			listColView.DrawContent += (s, e) => {
				_scrollBar.Size = listColView.Table?.Rows ?? 0;
				_scrollBar.Position = listColView.RowOffset;
				//_scrollBar.OtherScrollBarView.Size = listColView.Table?.Columns - 1 ?? 0;
				//_scrollBar.OtherScrollBarView.Position = listColView.ColumnOffset;
				_scrollBar.Refresh ();
			};

		}

		private void TableViewKeyPress (object sender, KeyEventEventArgs e)
		{
			if (e.KeyEvent.Key == Key.DeleteChar) {

				// set all selected cells to null
				foreach (var pt in listColView.GetAllSelectedCells ()) {
					currentTable.Rows [pt.Y] [pt.X] = DBNull.Value;
				}

				listColView.ListUpdate ();
				e.Handled = true;
			}

		}

		private void ToggleTopline ()
		{
			miTopline.Checked = !miTopline.Checked;
			listColView.Style.ShowHorizontalHeaderOverline = (bool)miTopline.Checked;
			listColView.Update ();
		}
		private void ToggleBottomline ()
		{
			miBottomline.Checked = !miBottomline.Checked;
			listColView.Style.ShowHorizontalBottomline = (bool)miBottomline.Checked;
			listColView.Update ();
		}
		private void ToggleExpandLastColumn ()
		{
			miExpandLastColumn.Checked = !miExpandLastColumn.Checked;
			listColView.Style.ExpandLastColumn = (bool)miExpandLastColumn.Checked;

			listColView.Update ();

		}

		private void ToggleAlwaysUseNormalColorForVerticalCellLines ()
		{
			miAlwaysUseNormalColorForVerticalCellLines.Checked = !miAlwaysUseNormalColorForVerticalCellLines.Checked;
			listColView.Style.AlwaysUseNormalColorForVerticalCellLines = (bool)miAlwaysUseNormalColorForVerticalCellLines.Checked;

			listColView.Update ();
		}
		private void ToggleSmoothScrolling ()
		{
			miSmoothScrolling.Checked = !miSmoothScrolling.Checked;
			listColView.Style.SmoothHorizontalScrolling = (bool)miSmoothScrolling.Checked;

			listColView.Update ();

		}
		private void ToggleCellLines ()
		{
			miCellLines.Checked = !miCellLines.Checked;
			listColView.Style.ShowVerticalCellLines = (bool)miCellLines.Checked;
			listColView.Update ();
		}
		private void ToggleAlternatingColors ()
		{
			//toggle menu item
			miAlternatingColors.Checked = !miAlternatingColors.Checked;

			if (miAlternatingColors.Checked == true) {
				listColView.Style.RowColorGetter = (a) => { return a.RowIndex % 2 == 0 ? alternatingColorScheme : null; };
			} else {
				listColView.Style.RowColorGetter = null;
			}
			listColView.SetNeedsDisplay ();
		}

		private void ToggleInvertSelectedCellFirstCharacter ()
		{
			//toggle menu item
			miCursor.Checked = !miCursor.Checked;
			listColView.Style.InvertSelectedCellFirstCharacter = (bool)miCursor.Checked;
			listColView.SetNeedsDisplay ();
		}

		private void ToggleVerticalOrientation ()
		{
			miOrientVertical.Checked = !miOrientVertical.Checked;
			listColView.ListStyle.VerticalOrientation = (bool)miOrientVertical.Checked;
			listColView.ListUpdate ();
		}

		private void ToggleScrollParallel ()
		{
			miScrollParallel.Checked = !miScrollParallel.Checked;
			listColView.ListStyle.ScrollParallel = (bool)miScrollParallel.Checked;
			listColView.ListUpdate ();
		}

		private void SetListMinWidth ()
		{
			RunListWidthDialog ("MinCellWidth", (s, v) => s.MinCellWidth = v, (s) => s.MinCellWidth);
		}

		private void SetListMaxWidth ()
		{
			RunListWidthDialog ("MaxCellWidth", (s, v) => s.MaxCellWidth = v, (s) => s.MaxCellWidth);
		}

		private void RunListWidthDialog (string prompt, Action<ListColumnView, int> setter, Func<ListColumnView, int> getter)
		{
			var accepted = false;
			var ok = new Button ("Ok", is_default: true);
			ok.Clicked += (s, e) => { accepted = true; Application.RequestStop (); };
			var cancel = new Button ("Cancel");
			cancel.Clicked += (s, e) => { Application.RequestStop (); };
			var d = new Dialog (ok, cancel) { Title = prompt };

			var tf = new TextField () {
				Text = getter (listColView).ToString (),
				X = 0,
				Y = 1,
				Width = Dim.Fill ()
			};

			d.Add (tf);
			tf.SetFocus ();

			Application.Run (d);

			if (accepted) {

				try {
					setter (listColView, int.Parse (tf.Text.ToString ()));
				} catch (Exception ex) {
					MessageBox.ErrorQuery (60, 20, "Failed to set", ex.Message, "Ok");
				}

				listColView.ListUpdate ();
			}
		}

		private void CloseExample ()
		{
			listColView.Table = null;
		}

		private void Quit ()
		{
			Application.RequestStop ();
		}

		private void OpenSimpleList (bool big)
		{
			SetTable (BuildSimpleList (big ? 1025 : 20));
		}

		private void SetTable (IList list)
		{
			listColView.ListData = list;
			currentTable = ((ListTableSource)listColView.Table).DataTable;
		}

		/*
		private void HideHeaders ()
		{
			listColView.Style.ShowHeaders = false;
			listColView.Style.ShowHorizontalHeaderOverline = false;
			listColView.Style.ShowHorizontalHeaderUnderline = false;
			listColView.Style.ShowHorizontalBottomline = false;
			listColView.ListUpdate ();
		}

		private void EditCurrentCell (object sender, CellActivatedEventArgs e)
		{
			if (e.Table == null)
				return;
			var o = currentTable.Rows [e.Row] [e.Col];

			var title = "Enter new value";

			var oldValue = currentTable.Rows [e.Row] [e.Col].ToString ();
			bool okPressed = false;

			var ok = new Button ("Ok", is_default: true);
			ok.Clicked += (s, e) => { okPressed = true; Application.RequestStop (); };
			var cancel = new Button ("Cancel");
			cancel.Clicked += (s, e) => { Application.RequestStop (); };
			var d = new Dialog (ok, cancel) { Title = title };

			var lbl = new Label () {
				X = 0,
				Y = 1,
				Text = currentTable.Columns [e.Col].ColumnName
			};

			var tf = new TextField () {
				Text = oldValue,
				X = 0,
				Y = 2,
				Width = Dim.Fill ()
			};

			d.Add (lbl, tf);
			tf.SetFocus ();

			Application.Run (d);

			if (okPressed) {

				try {
					currentTable.Rows [e.Row] [e.Col] = string.IsNullOrWhiteSpace (tf.Text.ToString ()) ? DBNull.Value : (object)tf.Text;
				} catch (Exception ex) {
					MessageBox.ErrorQuery (60, 20, "Failed to set text", ex.Message, "Ok");
				}

				listColView.ListUpdate ();
			}
		}
		*/

		/// <summary>
		/// Builds a simple list in which values are the index.  This helps testing that scrolling etc is working correctly and not skipping out values when paging
		/// </summary>
		/// <param name="cols"></param>
		/// <param name="rows"></param>
		/// <returns></returns>
		public static IList BuildSimpleList (int items)
		{
			var list = new List<object> ();

			for (int i = 0; i < items; i++) {
				list.Add ("Item " + i);
			}

			return list;
		}
	}
}
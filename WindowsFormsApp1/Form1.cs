using Antlr4.Runtime;
using FarsiLibrary.Win;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WindowsFormsApp1.Core.Parser;
using System.Text.RegularExpressions;
namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

    FastColoredTextBox CurrentTB
        {
            get
            {
                if (tsFiles.SelectedItem == null)
                    return null;
                return (tsFiles.SelectedItem.Controls[0] as FastColoredTextBox);
            }

            set
            {
                tsFiles.SelectedItem = (value.Parent as FATabStripItem);
                value.Focus();
            }
        }
        const float stepSizeFont = 1f;
        const string fontEditor = "Lucida Console";
        const float minSizeFont = 6f;
        const float maxSizeFont = 20f;

        private string url = "Resources/Regex.xml";

        public Form1()
        {
            InitializeComponent();

            toolTip1.SetToolTip(create_btn, "Создать");
            toolTip1.SetToolTip(open_btn, "Открыть");
            toolTip1.SetToolTip(save_btn, "Сохранить");
            toolTip1.SetToolTip(undo_btn, "Отменить");
            toolTip1.SetToolTip(redo_btn, "Повторить");
            toolTip1.SetToolTip(copy_btn, "Копировать");
            toolTip1.SetToolTip(cut_btn, "Вырезать");
            toolTip1.SetToolTip(paste_btn, "Вставить");
            toolTip1.SetToolTip(start_btn, "Запустить");
            toolTip1.SetToolTip(info_btn, "Информация");
            toolTip1.SetToolTip(help_btn, "Помощь");


            language_label.Text = InputLanguage.CurrentInputLanguage.Culture.DisplayName.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CreateTab(null);
        }

        private void CreateTab(string fileName)
        {
            try
            {
                var tb = new FastColoredTextBox();
                tb.Font = new Font(fontEditor, 9.75f);
                tb.Dock = DockStyle.Fill;
                tb.Paddings = new Padding(0, 5, 5, 5);
                tb.Language = Language.SQL;
                var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "Новый документ", tb);
                tab.Tag = fileName;
                if (fileName != null)
                    tb.OpenFile(fileName);
                tsFiles.AddTab(tab);
                tsFiles.SelectedItem = tab;
                tb.Focus();
                tb.DelayedTextChangedInterval = 1000;
                tb.DelayedEventsInterval = 500;
                
                tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                    CreateTab(fileName);
            }
        }

        private void splitContainer1_Paint(object sender, PaintEventArgs e)
        {
            var control = sender as SplitContainer;
            //paint the three dots'
            Point[] points = new Point[3];
            var w = control.Width;
            var h = control.Height;
            var d = control.SplitterDistance;
            var sW = control.SplitterWidth;

            //calculate the position of the points'
            if (control.Orientation == Orientation.Horizontal)
            {
                points[0] = new Point((w / 2), d + (sW / 2));
                points[1] = new Point(points[0].X - 10, points[0].Y);
                points[2] = new Point(points[0].X + 10, points[0].Y);
            }
            else
            {
                points[0] = new Point(d + (sW / 2), (h / 2));
                points[1] = new Point(points[0].X, points[0].Y - 10);
                points[2] = new Point(points[0].X, points[0].Y + 10);
            }

            foreach (Point p in points)
            {
                p.Offset(-2, -2);
                e.Graphics.FillEllipse(SystemBrushes.ControlDark,
                    new Rectangle(p, new Size(3, 3)));

                p.Offset(1, 1);
                e.Graphics.FillEllipse(SystemBrushes.ControlLight,
                    new Rectangle(p, new Size(3, 3)));
            }
        }

        private void faTabStrip1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void faTabStrip1_DragDrop(object sender, DragEventArgs e)
        {
            string[] filesPath = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach(string filePath in filesPath)
                CreateTab(filePath);
        }


        void open_btn_Click(object sender, EventArgs e) => openFile();
        void открытьToolStripMenuItem_Click(object sender, EventArgs e) => openFile();
        private void openFile()
        {
            if (ofdMain.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                CreateTab(ofdMain.FileName);
        }

        private bool Save(FATabStripItem tab, SaveFileDialog dialog, bool alwaysShowDialog = false)
        {
            var tb = (tab.Controls[0] as FastColoredTextBox);
            if (tab.Tag == null || alwaysShowDialog)
            {

                dialog.FileName = tab.Tag != null ? tab.Tag as string : tab.Title;

                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return false;
                tab.Title = Path.GetFileName(dialog.FileName);
                tab.Tag = dialog.FileName;
            }

            try
            {
                File.WriteAllText(tab.Tag as string, tb.Text);
                tb.IsChanged = false;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                    return Save(tab, sfdMain);
                else
                    return false;
            }

            tb.Invalidate();

            return true;
        }

        private void save_btn_Click(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
                Save(tsFiles.SelectedItem, sfdMain);
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
                Save(tsFiles.SelectedItem, sfdMain);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
                Save(tsFiles.SelectedItem, sfdSaveAs, true);
        }
        private void tsFiles_TabStripItemClosing(TabStripItemClosingEventArgs e)
        {
            if ((e.Item.Controls[0] as FastColoredTextBox).IsChanged)
            {
                switch (MessageBox.Show("Хотите ли вы сохранить файл - " + e.Item.Title + " ?", "Сохранение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information))
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        if (!Save(e.Item, sfdMain))
                            e.Cancel = true;
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void tmUpdateInterface_Tick(object sender, EventArgs e)
        {
            try
            {
                if (CurrentTB != null && tsFiles.Items.Count > 0)
                {
                    var tb = CurrentTB;
                    save_btn.Enabled = saveToolStripMenuItem.Enabled = saveAsToolStripMenuItem.Enabled = tb.IsChanged;
                    
                    reduce_btn.Enabled = increse_btn.Enabled = true;

                    undo_btn.Enabled = undoToolStripMenuItem.Enabled = tb.UndoEnabled;
                    redo_btn.Enabled = redoToolStripMenuItem.Enabled = tb.RedoEnabled;

                    paste_btn.Enabled = pasteToolStripMenuItem.Enabled = true;
                    cut_btn.Enabled = cutToolStripMenuItem.Enabled =
                    copy_btn.Enabled = copyToolStripMenuItem.Enabled = !tb.Selection.IsEmpty;

                    selectAllToolStripMenuItem.Enabled = tb.CanSelect;
                    start_btn.Enabled = true;
                }
                else
                {
                    save_btn.Enabled = saveToolStripMenuItem.Enabled = saveAsToolStripMenuItem.Enabled = false;

                    reduce_btn.Enabled = increse_btn.Enabled = false;

                    undo_btn.Enabled = undoToolStripMenuItem.Enabled = false;
                    redo_btn.Enabled = redoToolStripMenuItem.Enabled = false;

                    cut_btn.Enabled = cutToolStripMenuItem.Enabled =
                    copy_btn.Enabled = copyToolStripMenuItem.Enabled = false;
                    paste_btn.Enabled = pasteToolStripMenuItem.Enabled = false;
                    selectAllToolStripMenuItem.Enabled = false;
                    start_btn.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        void undo_btn_Click(object sender, EventArgs e)
        {
            if (CurrentTB.UndoEnabled)
                CurrentTB.Undo();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentTB.UndoEnabled)
                CurrentTB.Undo();
        }

        void redo_btn_Click(object sender, EventArgs e)
        {
            if (CurrentTB.RedoEnabled)
                CurrentTB.Redo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentTB.RedoEnabled)
                CurrentTB.Redo();
        }
        void copy_btn_Click(object sender, EventArgs e) => CurrentTB.Copy();
        void copyToolStripMenuItem_Click(object sender, EventArgs e) => CurrentTB.Copy();
        void cutToolStripMenuItem_Click(object sender, EventArgs e) => CurrentTB.Cut();
        void cut_btn_Click(object sender, EventArgs e) => CurrentTB.Cut();
        void deleteToolStripMenuItem_Click(object sender, EventArgs e) => CurrentTB.SelectedText = "";

        void exitToolStripMenuItem_Click(object sender, EventArgs e) => Close();
        void selectAllToolStripMenuItem_Click(object sender, EventArgs e) => CurrentTB.Selection.SelectAll();
        void paste_btn_Click(object sender, EventArgs e) => CurrentTB.Paste();
        void pasteToolStripMenuItem_Click(object sender, EventArgs e) => CurrentTB.Paste();
     
        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<FATabStripItem> list = new List<FATabStripItem>();
           
            foreach (FATabStripItem tab in tsFiles.Items)
                list.Add(tab);

            foreach (var tab in list)
            {
                TabStripItemClosingEventArgs args = new TabStripItemClosingEventArgs(tab);
                tsFiles_TabStripItemClosing(args);
                if (args.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                tsFiles.RemoveTab(tab);
            }
        }

        void reduce_btn_Click(object sender, EventArgs e)
            => changeTextSizeForAllTabs(getIncreseTextSize(CurrentTB));

        void increse_btn_Click(object sender, EventArgs e)
            => changeTextSizeForAllTabs(getReduceTextSize(CurrentTB));

        void tableLayoutPanel1_increse_btn_Click(object sender, EventArgs e)
            => changeTextForClassicTabs(getIncreseTextSize(output_tb, 12));

        void tableLayoutPanel1_reduce_btn_Click(object sender, EventArgs e)
           => changeTextForClassicTabs(getReduceTextSize(output_tb));

        void changeTextSizeForAllTabs(float size)
        {
            List<FATabStripItem> list = new List<FATabStripItem>();

            foreach (FATabStripItem tab in tsFiles.Items)
                list.Add(tab);

            foreach (var tab in list)
            {
                var tb = tab.Controls[0] as FastColoredTextBox;
                if(size != 0)
                {
                    tb.Font = new Font(fontEditor, size);
                }
            }
        }

        void changeTextForClassicTabs(float fontSize)
        {
            foreach (Control tab in tabControl2.Controls)
                if (fontSize != 0)
                {
                    var a = tab.Controls[0];
                    tab.Controls[0].Font = new Font(fontEditor, fontSize);
                    
                }
        }

        float getReduceTextSize(Control control, float minSize = minSizeFont)
        {
            if (control.Font.Size - stepSizeFont > minSize)
                return control.Font.Size - stepSizeFont;
            return minSize;
        }
        float getIncreseTextSize(Control control, float maxSize = maxSizeFont)
        {
            if (control.Font.Size + stepSizeFont < maxSize)
                return control.Font.Size + stepSizeFont;
            return maxSize;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dateLabel.Text = DateTime.Now.ToLongDateString();
            timeLabel.Text = DateTime.Now.ToLongTimeString();
        }

        private void help_btn_Click(object sender, EventArgs e) => new Help().Show();

        private void info_btn_Click(object sender, EventArgs e) => new Info().Show();

        void вызовСправкиToolStripMenuItem_Click(object sender, EventArgs e) => new Help().Show();

        void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e) => new Info().Show();

        private void Form1_InputLanguageChanged(object sender, InputLanguageChangedEventArgs e)
        {
            language_label.Text = InputLanguage.CurrentInputLanguage.Culture.DisplayName.ToString();
        }
        MarkerStyle RedStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(49, Color.Red)));

        private void start_btn_Click(object sender, EventArgs e)
        {
            try
            {
                AntlrInputStream input = new AntlrInputStream(CurrentTB.Text);

                EnumGrammarLexer lexer = new EnumGrammarLexer(input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                EnumGrammarParser parser = new EnumGrammarParser(tokens);
                BaseErrorListener errListener = new ErrorListener();
                parser.AddErrorListener(errListener);
                parser.expr();
                ShutingYardAlg semantic = new ShutingYardAlg(tokens);
                SemanticResult result = semantic.run();
                output_tb.Text = "Постфиксная запись: " + result.Operations + "\r\nРезультат: " + result.Result;
            }
            catch(RequireAnotherTokenException ex)
            {
                tabControl2.SelectedIndex = 0;
                dataGridView1.Rows.Clear();
                tabControl2.TabPages[2].Hide();

                output_tb.Text = ex.Message;
                CurrentTB.Selection.ClearStyle();
                string val = Regex.Match(ex.Message, "[0-9]+").Value;
                int number = int.Parse(val);

                FastColoredTextBoxNS.Range range = new FastColoredTextBoxNS.Range
                    (CurrentTB, new Place(number, 0), new Place(number + 1, 0));
                range.SetStyle(RedStyle);
            }



                /*Parser parser = new Parser();
                try
                {
                    parser.parse(CurrentTB.Text);
                    output_tb.Text = "";
                }
                catch (RequireAnotherTokenException ex)
                {
                    tabControl2.SelectedIndex = 0;
                    dataGridView1.Rows.Clear();
                    tabControl2.TabPages[2].Hide();

                    output_tb.Text = ex.Message;

                    string val = Regex.Match(ex.Message, "[0-9]+").Value;
                    int number = int.Parse(val);

                    FastColoredTextBoxNS.Range range = new FastColoredTextBoxNS.Range
                        (CurrentTB, new Place(number, 0), new Place(number+1, 0));
                    range.SetStyle(RedStyle);
                }
    */
                /*try
                {
                    TokenStream stream = lexer.getTokenStream(CurrentTB.Text);

                    tabControl2.SelectedIndex = 2;
                    output_tb.Text = "";

                    dataGridView1.Rows.Clear();
                    while (stream.hasNext())
                    {
                        Token token = stream.Next();
                        dataGridView1.Rows.Add(token.TokenType.Type, token.Value, token.Range.Start, token.Range.End);
                    }
                }
                catch(NotStatementException ex)
                {
                    tabControl2.SelectedIndex = 0;
                    dataGridView1.Rows.Clear();
                    tabControl2.TabPages[2].Hide();

                    output_tb.Text = ex.Message;
                }*/
            }
    }
}

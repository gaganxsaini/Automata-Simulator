using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using Microsoft.Windows.Controls.Ribbon;
using System.Xml.Linq;
using System.Linq;

namespace Automata_Simulator
{
    public abstract class Pda
    {
        public const int MAX = 1000;
        
        public TextBox initialState = null;
        public TextBox finalState = null;

        public Canvas canvasMain=null;
        public RibbonButton deleteRowButton=null;
        public RibbonToggleButton setInitialStateButton = null;
        public RibbonToggleButton setFinalStateButton = null;

        public pdaRow selectedRow = null;

        public List<string> inputs = new List<string>();
        public List<string> stackSymbols = new List<string>();

        public List<pdaRow> rows = new List<pdaRow>();

        public void setCanvas(Canvas c, RibbonButton btn, RibbonToggleButton rtb, RibbonToggleButton rtbF )
        {
            canvasMain = c;
            deleteRowButton = btn;
            setInitialStateButton = rtb;
            setFinalStateButton = rtbF;
        }

        public enum rowItems
        { 
            currentState, inoutSymbol, stackTop, lblArrow, Action, NextState
        }
        
        public abstract bool checkStringAcceptance(String str);
        
        public void deleteSelectedRow()
        {
            if ((TextBox)selectedRow.sp.Children[(int)rowItems.currentState] == initialState)
                initialState = null;

            if ((TextBox)selectedRow.sp.Children[(int)rowItems.NextState] == finalState)
                finalState = null;

            StackPanel sp=(StackPanel)canvasMain.Children[0];

            int i = sp.Children.IndexOf(selectedRow.sp);
            
            sp.Children.Remove(selectedRow.sp);
            this.rows.Remove(selectedRow);

            if (i < sp.Children.Count)
            {
                Label lbl = (Label)((StackPanel)sp.Children[i]).Children[(int)rowItems.lblArrow];
                mutexSelectRow(lbl, null);
            }
            else
                selectedRow = null;
        }

        public abstract pdaRow addNewRow();

        public void mutexSelectRow(object sender, MouseButtonEventArgs e)
        {
            if(e != null) //null when the function is called in the code
                e.Handled = true;// written here bcoz we have returns before last statement
         
            Label lbl = (Label)sender;

            if (selectedRow != null) // if a produciton is already selected
            {
                selectedRow.sp.Background = Brushes.White;

                if (selectedRow.sp == (StackPanel)lbl.Parent) // if selected production is clicked again
                {
                    selectedRow = null;
                    deleteRowButton.IsEnabled = false;
                    return;
                }
            }
            
            selectedRow = getRowByIndex((int)((StackPanel)lbl.Parent).Tag);
            selectedRow.sp.Background = Brushes.LightBlue;
            deleteRowButton.IsEnabled = true;
        }

        public pdaRow getRowByIndex(int i)
        {
            foreach (pdaRow row in rows)
            {
                if (row.index == i)
                    return row;
            }
            return null;
        }

        public virtual bool isErroneous() // override it
        {
            if (initialState == null)
            {
                MessageBox.Show("Set Initial State first");
                return true;
            }
            StackPanel s = ((StackPanel)canvasMain.Children[0]);
            foreach (StackPanel sp in s.Children)
            {
                TextBox txtCurrentState = (TextBox)sp.Children[0];
                ComboBox cbInputSymbol = (ComboBox)sp.Children[1];
                ComboBox cbStackTop = (ComboBox)sp.Children[2];
                ComboBox cbAction = (ComboBox)sp.Children[4];
                TextBox txtNextState = (TextBox)sp.Children[5];
                if (txtCurrentState.Text == "" || txtNextState.Text == "" || cbStackTop.SelectedIndex == -1
                    || cbInputSymbol.SelectedIndex == -1 || cbAction.SelectedIndex == -1)
                {
                    MessageBox.Show("Please fill all the rows"); 
                    return true;
                }

            }

            return false;

        }

        public void keyUpEventHandled(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        public void txtCurrentState_LostFocus(object sender, RoutedEventArgs e)
        {
            setInitialStateButton.IsChecked = false;
            setInitialStateButton.IsEnabled = false;
        }

        public void txtCurrentState_GotFocus(object sender, RoutedEventArgs e)
        {
            pdaRow temp = getRowByIndex((int)((StackPanel)((TextBox)sender).Parent).Tag);
            
            if (temp != selectedRow)
                mutexSelectRow(temp.lbl, null);
            //else the row is already selected, do not unselect it....


            setInitialStateButton.IsEnabled = true;
            bool k = setInitialStateButton.IsEnabled;
            bool p = ((RibbonGroup)setInitialStateButton.Parent).IsEnabled;
            if (initialState == (TextBox)sender)
                setInitialStateButton.IsChecked = true;
            else
                setInitialStateButton.IsChecked = false;

        }

        public void txtNextState_LostFocus(object sender, RoutedEventArgs e)
        {
            setFinalStateButton.IsChecked = false;
            setFinalStateButton.IsEnabled = false;
        }

        public void txtNextState_GotFocus(object sender, RoutedEventArgs e)
        {
            pdaRow temp = getRowByIndex((int)((StackPanel)((TextBox)sender).Parent).Tag);
            
            if(temp!=selectedRow) 
                mutexSelectRow(temp.lbl, null);
            //else the row is already selected, do not unselect it....


            setFinalStateButton.IsEnabled = true;
            bool k = setFinalStateButton.IsEnabled;

            if (finalState == (TextBox)sender)
                setFinalStateButton.IsChecked = true;
            else
                setFinalStateButton.IsChecked = false;
        }

        public virtual string saveToFile(XDocument xd = null, XElement root = null, string name="") //override it
        {
            // name is used(in the derived classes) to generate temp files automatically without requiring users to enter filename

            string str = "";
            foreach (String s in inputs)
            {
                str = str + s + ";";
            }

            str = str.TrimEnd(';');

            root.Add(new XAttribute("inputs", str));
            root.Add(new XAttribute("width", canvasMain.ActualWidth));
            root.Add(new XAttribute("height", canvasMain.ActualHeight));
            
            XElement xRows = new XElement("Rows");
            foreach (pdaRow r in rows)
            {
                XElement x = new XElement("Row");
                
                x.Add(new XAttribute("currentState", r.txtCurrentState.Text));
                
                if (r.txtCurrentState.Background == Brushes.LightGreen)
                    x.Add(new XAttribute("isInitial","True"));

                if(r.cbInputSymbol.SelectedIndex!=-1)
                    x.Add(new XAttribute("inputSymbol", r.cbInputSymbol.SelectedItem.ToString()));

                if(r.cbStackTop.SelectedIndex!=-1)
                    x.Add(new XAttribute("stackTopSymbol", r.cbStackTop.SelectedItem.ToString()));

                if(r.cbAction.SelectedIndex!=-1)
                    x.Add(new XAttribute("action", r.cbAction.SelectedItem.ToString()));

                x.Add(new XAttribute("nextState", r.txtNextState.Text));
                
                if(r.txtNextState.Background == Brushes.LightPink)
                    x.Add(new XAttribute("isFinal","True"));

                xRows.Add(x);
            }
            root.Add(xRows);

            xd.Add(root);

            return "";
        }

        public virtual void loadFromFile(XDocument doc) //override it
        {
            string[] i = doc.Root.Attribute("inputs").Value.Split(';');

            canvasMain.Height = Convert.ToDouble(doc.Root.Attribute("height").Value);
            canvasMain.Width = Convert.ToDouble(doc.Root.Attribute("width").Value);

            foreach (string s in i)
            {
                this.inputs.Add(s);
            }

            this.stackSymbols = this.inputs.ToList();
            if (!this.inputs.Contains("^"))
                this.stackSymbols.Add("^");//stack's last symbol (empty stack)


            var query = from row in doc.Root.Element("Rows").Elements("Row") 
                        select row;
            
            foreach (XElement s in query)
            {
                pdaRow r = this.addNewRow();
                r.txtCurrentState.Text = s.Attribute("currentState").Value;

                if (s.Attribute("isInitial") != null)
                {
                    if (s.Attribute("isInitial").Value == "True")
                    {
                        r.txtCurrentState.Background = Brushes.LightGreen;
                        initialState = r.txtCurrentState;
                    }
                }

                if(s.Attribute("inputSymbol")!=null)
                    r.cbInputSymbol.SelectedItem = s.Attribute("inputSymbol").Value;

                if (s.Attribute("stackTopSymbol") != null)
                    r.cbStackTop.SelectedItem = s.Attribute("stackTopSymbol").Value;

                if (s.Attribute("action") != null)
                    r.cbAction.SelectedItem = s.Attribute("action").Value;
                
                r.txtNextState.Text = s.Attribute("nextState").Value;
                
                if (s.Attribute("isFinal")!=null)
                {
                    if (s.Attribute("isFinal").Value == "True")
                    {
                        r.txtNextState.Background = Brushes.LightPink;
                        finalState = r.txtNextState;
                    }
                }
            }
        }
    }
   

    public class DPda : Pda
    {
        //string[,] table = new string[MAX, 6];

        string currentState = "";

        public override string saveToFile(XDocument xd = null, XElement root = null, string name="")
        {
            //Arg description:
            //Name :  to generate temp files automatically without requiring users to enter filename or to save the opened file

            if (name == "")
            {
                Microsoft.Win32.SaveFileDialog d = new Microsoft.Win32.SaveFileDialog();
                d.AddExtension = true;
                d.CheckPathExists = true;
                d.DefaultExt = "pda";
                d.Filter = "PDA files (*.pda)|*.pda";
                d.OverwritePrompt = true;

                if (d.ShowDialog() != false) // cancel button is not pressed
                    name = d.FileName;
                else
                    return "";
            }

            xd = new XDocument();
            root = new XElement("Pda");
            root.Add(new XAttribute("type", "DPda"));
 
            base.saveToFile(xd, root);
            xd.Save(name);
            return name;

        }

        public override void loadFromFile(XDocument doc)
        {
            if (doc.Root.Attribute("type").Value == "DPda")
                base.loadFromFile(doc);
        }
        
        public override bool checkStringAcceptance(string str)
        {
            Stack<string> stack = new Stack<string>();
            stack.Push("^");

            StackPanel s = ((StackPanel)canvasMain.Children[0]);
            currentState = initialState.Text;

            bool inputSymbolsHasNull=inputs.Contains("^");

            int i = 0;
            while (true)
            {
                if (i < str.Length)
                {
                    bool isRuleFound = false;
                    foreach (StackPanel sp in s.Children)
                    {
                        TextBox txtCurrentState = (TextBox)sp.Children[0];
                        ComboBox cbInputSymbol = (ComboBox)sp.Children[1];
                        ComboBox cbStackTop = (ComboBox)sp.Children[2];
                        ComboBox cbAction = (ComboBox)sp.Children[4];
                        TextBox txtNextState = (TextBox)sp.Children[5];

                        if (txtCurrentState.Text == currentState && cbStackTop.SelectedItem.ToString() == stack.Peek()
                            && cbInputSymbol.SelectedItem.ToString() == str[i].ToString())
                        {
                            isRuleFound = true;
                            if (cbAction.SelectedIndex == 0)//push
                                stack.Push(str[i].ToString());
                            else if (cbAction.SelectedIndex == 1)//pop
                                stack.Pop();

                            currentState = txtNextState.Text;
                            break;
                        }

                    }

                    if (!isRuleFound)
                        return false;
                    i++;
                }
                else if (inputSymbolsHasNull && stack.Peek()!="^")//i.e. null transitions && stack is not empty
                {
                    bool isRuleFound = false;
                    foreach (StackPanel sp in s.Children)
                    {
                        TextBox txtCurrentState = (TextBox)sp.Children[0];
                        ComboBox cbInputSymbol = (ComboBox)sp.Children[1];
                        ComboBox cbStackTop = (ComboBox)sp.Children[2];
                        ComboBox cbAction = (ComboBox)sp.Children[4];
                        TextBox txtNextState = (TextBox)sp.Children[5];

                        if (txtCurrentState.Text == currentState && cbStackTop.SelectedItem.ToString() == stack.Peek()
                            && cbInputSymbol.SelectedItem.ToString() == "^")
                        {
                            isRuleFound = true;
                            if (cbAction.SelectedIndex == 0)//push
                                stack.Push(str[i].ToString());
                            else if (cbAction.SelectedIndex == 1)//pop
                                stack.Pop();

                            currentState = txtNextState.Text;
                            break;
                        }

                    }

                    if (!isRuleFound)
                        return false;
                }
                else
                    break;
            }

            if (stack.Peek() == "^")
                return true;
            else
                return false;
        }

        public override pdaRow addNewRow()
        {
            if (canvasMain.Children.Count == 0)
            {
                StackPanel s = new StackPanel();
                s.SetValue(Canvas.LeftProperty, (double)20);
                s.SetValue(Canvas.TopProperty, (double)20);
                canvasMain.Children.Add(s);
            }
            StackPanel sp = ((StackPanel)canvasMain.Children[0]);

            if (sp.Children.Count == MAX - 1)
            {
                MessageBox.Show("You can not add more than " + MAX.ToString() + " rows");
                return null;
            }

            if (sp.ActualHeight > canvasMain.ActualHeight)
                canvasMain.Height = canvasMain.ActualHeight + 50;

            pdaRow row = new pdaRow();
            row.txtCurrentState.GotFocus += new RoutedEventHandler(txtCurrentState_GotFocus);
            row.txtCurrentState.LostFocus += new RoutedEventHandler(txtCurrentState_LostFocus);
            row.txtCurrentState.KeyUp += new KeyEventHandler(keyUpEventHandled);
            row.txtCurrentState.TextChanged += new TextChangedEventHandler(txtCurrentState_TextChanged);

            row.cbInputSymbol.ItemsSource = inputs;
            row.cbInputSymbol.SelectionChanged += new SelectionChangedEventHandler(cbInputSymbol_SelectionChanged);
            row.cbInputSymbol.KeyUp += new KeyEventHandler(keyUpEventHandled);

            row.cbStackTop.ItemsSource = stackSymbols;
            row.cbStackTop.SelectionChanged += new SelectionChangedEventHandler(cbStackTop_SelectionChanged);
            row.cbStackTop.KeyUp+= new KeyEventHandler(keyUpEventHandled);

            row.lbl.MouseLeftButtonDown += new MouseButtonEventHandler(mutexSelectRow);
            row.lbl.MouseLeftButtonUp += new MouseButtonEventHandler(lbl_MouseLeftButtonUp);//to supress canvas's event only

            row.txtNextState.GotFocus += new RoutedEventHandler(txtNextState_GotFocus);
            row.txtNextState.LostFocus += new RoutedEventHandler(txtNextState_LostFocus);
            row.txtNextState.KeyDown += new KeyEventHandler(tb2_KeyDown);
            row.txtNextState.KeyUp += new KeyEventHandler(keyUpEventHandled);
            row.txtNextState.TextChanged += new TextChangedEventHandler(txtNextState_TextChanged);
            
            row.cbAction.KeyUp += new KeyEventHandler(keyUpEventHandled);
            row.cbAction.SelectionChanged += isEditedHandler;

            rows.Add(row);
            sp.Children.Add(row.sp);

            return row;
        }

        void isEditedHandler(object sender, SelectionChangedEventArgs e)
        {
            fileStatus.isEdited = true;
        }

        void lbl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void txtNextState_TextChanged(object sender, TextChangedEventArgs e)
        {
            isEditedHandler(null, null);
            pdaRow currentRow = getRowByIndex((int)((StackPanel)((TextBox)sender).Parent).Tag);

            if (currentRow.txtCurrentState.Text == "" || currentRow.txtNextState.Text == ""
                || currentRow.cbInputSymbol.SelectedIndex == -1 || currentRow.cbStackTop.SelectedIndex == -1)
                return;

            foreach (StackPanel sp in ((StackPanel)canvasMain.Children[0]).Children)
            {
                pdaRow row=getRowByIndex((int)sp.Tag);

                if (currentRow == row)
                    continue;

                if (row.txtCurrentState.Text == "" || row.cbInputSymbol.SelectedIndex == -1 || row.cbStackTop.SelectedIndex == -1)
                    continue;


                if (currentRow.txtCurrentState.Text == row.txtCurrentState.Text && currentRow.cbInputSymbol.SelectedItem.ToString() == row.cbInputSymbol.SelectedItem.ToString()
                    && currentRow.cbStackTop.SelectedItem.ToString() == row.cbStackTop.SelectedItem.ToString())
                {
                    MessageBox.Show("In Dpda, you cannot add more than one transitions on same input symbols");
                    currentRow.txtNextState.Text = "";
                    return;
                
                }
            }

        }

        void cbStackTop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isEditedHandler(sender,e);

            pdaRow currentRow = getRowByIndex((int)((StackPanel)((ComboBox)sender).Parent).Tag);

            if (currentRow.txtCurrentState.Text == "" 
                || currentRow.cbInputSymbol.SelectedIndex == -1 || currentRow.cbStackTop.SelectedIndex == -1)
                return;

            foreach (StackPanel sp in ((StackPanel)canvasMain.Children[0]).Children)
            {
                pdaRow row = getRowByIndex((int)sp.Tag);

                if (currentRow == row)
                    continue;

                if (row.txtCurrentState.Text == "" || row.cbInputSymbol.SelectedIndex == -1 || row.cbStackTop.SelectedIndex == -1)
                    continue;


                if (currentRow.txtCurrentState.Text == row.txtCurrentState.Text && currentRow.cbInputSymbol.SelectedItem.ToString() == row.cbInputSymbol.SelectedItem.ToString()
                    && currentRow.cbStackTop.SelectedItem.ToString() == row.cbStackTop.SelectedItem.ToString())
                {
                    MessageBox.Show("In Dpda, you cannot add more than one transitions on same input symbols");
                    currentRow.cbStackTop.SelectedIndex = -1;
                    return;
                }
            }
        }

        void cbInputSymbol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isEditedHandler(sender, e);

            pdaRow currentRow = getRowByIndex((int)((StackPanel)((ComboBox)sender).Parent).Tag);

            if (currentRow.txtCurrentState.Text == "" 
                || currentRow.cbInputSymbol.SelectedIndex == -1 || currentRow.cbStackTop.SelectedIndex == -1)
                return;

            foreach (StackPanel sp in ((StackPanel)canvasMain.Children[0]).Children)
            {
                pdaRow row = getRowByIndex((int)sp.Tag);

                if (currentRow == row)
                    continue;

                if (row.txtCurrentState.Text == "" || row.cbInputSymbol.SelectedIndex == -1 || row.cbStackTop.SelectedIndex == -1)
                    continue;

                if (currentRow.txtCurrentState.Text == row.txtCurrentState.Text && currentRow.cbInputSymbol.SelectedItem.ToString() == row.cbInputSymbol.SelectedItem.ToString()
                    && currentRow.cbStackTop.SelectedItem.ToString() == row.cbStackTop.SelectedItem.ToString())
                {
                    MessageBox.Show("In Dpda, you cannot add more than one transitions on same input symbols");
                    currentRow.cbInputSymbol.SelectedIndex = -1;
                    return;

                }
            }
        }

        void txtCurrentState_TextChanged(object sender, TextChangedEventArgs e)
        {
            isEditedHandler(null, null);
            
            pdaRow currentRow = getRowByIndex((int)((StackPanel)((TextBox)sender).Parent).Tag);

            if (currentRow.txtCurrentState.Text == "" 
                || currentRow.cbInputSymbol.SelectedIndex == -1 || currentRow.cbStackTop.SelectedIndex == -1)
                return;


            foreach (StackPanel sp in ((StackPanel)canvasMain.Children[0]).Children)
            {
                pdaRow row = getRowByIndex((int)sp.Tag);

                if (currentRow == row)
                    continue;

                if (row.txtCurrentState.Text == "" || row.cbInputSymbol.SelectedIndex == -1 || row.cbStackTop.SelectedIndex == -1)
                    continue;

                if (currentRow.txtCurrentState.Text == row.txtCurrentState.Text && currentRow.cbInputSymbol.SelectedItem.ToString() == row.cbInputSymbol.SelectedItem.ToString()
                    && currentRow.cbStackTop.SelectedItem.ToString() == row.cbStackTop.SelectedItem.ToString())
                {
                    MessageBox.Show("In Dpda, you cannot add more than one transitions on same input symbols");
                    currentRow.txtCurrentState.Text = "";
                    return;

                }
            }
        }

        public void tb2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                StackPanel sp = (StackPanel)((TextBox)sender).Parent;
                StackPanel sp2 = (StackPanel)sp.Parent;
                if (sp2.Children.IndexOf(sp) == sp2.Children.Count - 1)
                {
                    addNewRow();
                }

            }
        }
    }

    public class NDPda : Pda
    {
        public override bool checkStringAcceptance(string str)
        {
        //    if (!inputs.Contains("^")) //without null moves
        //    {

        //        if (getIndexOfInput(str[0].ToString()) == -1)
        //            return false;

        //        foreach (State initialState in this.initialStates)
        //        {
        //            List<State> a = table[initialState.index, getIndexOfInput(str[0].ToString())];
        //            if (a == null)
        //                continue;
        //            if (str.Length == 1)
        //            {
        //                foreach (State s in a)
        //                    if (isFinal(s))
        //                        return true;
        //            }

        //            int indexInsideString = 1; //index of the next character to be scanned

        //            List<State> currentStates = a;

        //            while (indexInsideString < str.Length)
        //            {
        //                List<State> temp = new List<State>();

        //                foreach (State s in currentStates)
        //                {
        //                    int i = getIndexOfInput(str[indexInsideString].ToString());
        //                    if (i == -1)
        //                        return false;
        //                    List<State> d = table[s.index, i];
        //                    if (d != null)
        //                        foreach (State item in d)
        //                            temp.Add(item);
        //                }

        //                currentStates = temp;
        //                indexInsideString++;
        //            }

        //            foreach (State s in currentStates)
        //                if (isFinal(s))
        //                    return true;

        //        }

        //        return false;
        //    }
        //    else // with null moves
        //    {
        //        foreach (State initialState in this.initialStates)
        //        {
        //            List<State> a = nullClosure(initialState);

        //            if (str.Length == 0)
        //            {
        //                if (a == null)
        //                    continue;
        //                else
        //                {
        //                    foreach (State s in a)
        //                        if (isFinal(s))
        //                            return true;
        //                    return false;
        //                }
        //            }

        //            if (getIndexOfInput(str[0].ToString()) == -1)
        //                return false;

        //            int indexInsideString = 0; //index of the next character to be scanned

        //            List<State> currentStates = a;

        //            while (indexInsideString < str.Length)
        //            {
        //                List<State> temp = new List<State>();

        //                foreach (State s in currentStates)
        //                {
        //                    int i = getIndexOfInput(str[indexInsideString].ToString());
        //                    if (i == -1)
        //                        return false;
        //                    List<State> d = table[s.index, i];
        //                    if (d != null)
        //                        foreach (State item in d)
        //                        {
        //                            foreach (State st in nullClosure(item))
        //                            {
        //                                //if (st.index == item.index)
        //                                //  continue;
        //                                if (!temp.Contains(st))
        //                                    temp.Add(st);
        //                            }
        //                            //temp.Add(item);
        //                        }
        //                }

        //                currentStates = temp;
        //                indexInsideString++;
        //            }

        //            foreach (State s in currentStates)
        //                if (isFinal(s))
        //                    return true;

        //        }

                return false;
        }

        public override pdaRow addNewRow()
        {
            if (canvasMain.Children.Count == 0)
            {
                StackPanel s = new StackPanel();
                s.SetValue(Canvas.LeftProperty, (double)20);
                s.SetValue(Canvas.TopProperty, (double)20);
                canvasMain.Children.Add(s);
            }
            StackPanel sp = ((StackPanel)canvasMain.Children[0]);

            if (sp.Children.Count == MAX - 1)
            {
                MessageBox.Show("You can not add more than " + MAX.ToString() + " rows");
                return null;
            }

            if (sp.ActualHeight > canvasMain.ActualHeight)
                canvasMain.Height = canvasMain.ActualHeight + 50;

            pdaRow row = new pdaRow();
            row.txtCurrentState.GotFocus += new RoutedEventHandler(txtCurrentState_GotFocus);
            row.txtCurrentState.LostFocus += new RoutedEventHandler(txtCurrentState_LostFocus);
            row.txtCurrentState.KeyUp += new KeyEventHandler(keyUpEventHandled);

            row.cbInputSymbol.ItemsSource = inputs;
            row.cbInputSymbol.KeyUp += new KeyEventHandler(keyUpEventHandled);

            row.cbStackTop.ItemsSource = stackSymbols;
            row.cbStackTop.KeyUp += new KeyEventHandler(keyUpEventHandled);

            row.lbl.MouseLeftButtonDown += new MouseButtonEventHandler(mutexSelectRow);

            row.txtNextState.KeyDown += new KeyEventHandler(tb2_KeyDown);
            row.txtNextState.KeyUp += new KeyEventHandler(keyUpEventHandled);

            row.cbAction.KeyUp += new KeyEventHandler(keyUpEventHandled);

            rows.Add(row);
            sp.Children.Add(row.sp);

            return row;

        }

        public void tb2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                StackPanel sp = (StackPanel)((TextBox)sender).Parent;
                StackPanel sp2 = (StackPanel)sp.Parent;
                if (sp2.Children.IndexOf(sp) == sp2.Children.Count - 1)
                {
                    addNewRow();
                }

            }
        }
    }

    public class pdaRow
    {
        public static int nextRowIndex = 0;

        public StackPanel sp = new StackPanel();
        public int index;

        public TextBox txtCurrentState = new TextBox() { Width = 40, Height = 23, MaxLength = 1, ToolTip="Current State"};
        public ComboBox cbInputSymbol = new ComboBox() {ToolTip="Input Symbol", Width=40 };
        public ComboBox cbStackTop = new ComboBox() {ToolTip="Stack Top Symbol", Width=40 };
        public Label lbl = new Label() { Content = "->", ToolTip = "Click to Select the row" };
        public ComboBox cbAction = new ComboBox() { ToolTip = "Action", Width=125 };
        public TextBox txtNextState = new TextBox() { Width = 40, Height = 23, ToolTip="Next State" };
        
        public pdaRow()
        {
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(txtCurrentState);
            sp.Children.Add(cbInputSymbol);
            sp.Children.Add(cbStackTop);
            sp.Children.Add(lbl);

            cbAction.Items.Add("Push Input Symbol");
            cbAction.Items.Add("Pop Top Symbol");
            cbAction.Items.Add("Do Nothing");
            cbAction.SelectedIndex = 0;
            sp.Children.Add(cbAction);
            
            sp.Children.Add(txtNextState);

            index = nextRowIndex++;
            
            sp.Tag = index;
        }

    }

}

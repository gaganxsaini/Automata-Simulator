using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Windows.Controls.Ribbon;
using System.Xml.Linq;
using Automata_Simulator.Extensions;
using System.Windows.Media.Animation;
using System.Diagnostics;

namespace Automata_Simulator
{

    enum modes
    { 
        Grammar, Machines, Pda, None
    }

    class fileStatus
    {
        public static bool isEdited=false;
    }

    public partial class MainWindow : RibbonWindow
    {
        public bool isFiringFromStartScreen = false;

        string loadedFilePath = "";

        public MainWindow()
        {
            InitializeComponent();
            Grammar.setCanvas(canvasMain);
        }

        modes mode=modes.None;

        private void canvasMain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (diagram == null || mode != modes.Machines)
                return;
            
            if (action == actions.addState)
            {
                if (diagram is MooreTransitionDiagram)
                {
                    if (txtOutputSymbols.Text == "")
                    {
                        MessageBox.Show("Please enter input and output symbols first");
                        return;
                    }
                }

                Point pos = new Point(e.GetPosition(canvasMain).X, e.GetPosition(canvasMain).Y);
                State s = diagram.addState(pos);
                if (s != null)
                {
                    fileStatus.isEdited = true;
                    setStateEventHandlers(s);
                    diagram.mutexSelect(s);
                    updateUiOnStateSelectionChanged(true);
                }
            }
            else // deselect everything
            {
                if (diagram.selectedState != null)
                    diagram.mutexSelect(diagram.selectedState);
                else if (diagram.selectedTransition != null)
                    diagram.mutexSelect(diagram.selectedTransition);
                
                updateUiOnStateSelectionChanged(false);
                updateUiOnTransitionSelectionChanged(false);
                
                diagram.isDragging = false;
                canvasMain.Cursor = Cursors.Arrow;

                action = getAction();
                
                //if(btnAddTrans.IsChecked==true)
                  //  action = actions.addTransitionStartingPt;
            }
              
        }

        private void canvasMain_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (diagram != null && mode==modes.Machines)
            {
                diagram.isDragging = false;
                canvasMain.Cursor = Cursors.Arrow;
                action = getAction();
            }
        }

        private void canvasMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (diagram == null || mode != modes.Machines)
                return;

            if (diagram.isDragging == true)
            {
                if (action == actions.moveTrans)
                {
                    fileStatus.isEdited = true;

                    Path movingTransition = diagram.selectedTransition.figure;

                    QuadraticBezierSegment q = (QuadraticBezierSegment)(((PathGeometry)movingTransition.Data).Figures[0].Segments[0]);
                    Point startingPt = ((PathGeometry)movingTransition.Data).Figures[0].StartPoint;
                    Point endingPt = q.Point2;

                    PathGeometry gmetry = (PathGeometry)movingTransition.Data;
                    Point pt = new Point(e.GetPosition(canvasMain).X, e.GetPosition(canvasMain).Y);

                    q.Point1 = pt;

                    Point mid = new Point((startingPt.X + endingPt.X) / 2, (startingPt.Y + endingPt.Y) / 2);

                    Point mid2 = new Point((mid.X + pt.X) / 2, (mid.Y + pt.Y) / 2);

                    StackPanel sp = (StackPanel)diagram.selectedTransition.figure.Tag;
                    
                    sp.SetValue(Canvas.LeftProperty, mid2.X);
                    sp.SetValue(Canvas.TopProperty, mid2.Y);

                    Transition.addArrow(diagram.selectedTransition, canvasMain);

                }
                else if (action == actions.moveState)
                {
                    fileStatus.isEdited = true;
                    Ellipse movingState = diagram.selectedState.figure;
                    Point newPos = new Point(e.GetPosition(canvasMain).X - State.SIZE / 2, e.GetPosition(canvasMain).Y - State.SIZE / 2);
                    movingState.SetValue(Canvas.LeftProperty, newPos.X);
                    movingState.SetValue(Canvas.TopProperty, newPos.Y);

                    if (diagram.selectedState.type == stateType.initial || diagram.selectedState.type == stateType.both)
                    {
                        
                        double left = (double)movingState.GetValue(Canvas.LeftProperty);
                        double top = (double)movingState.GetValue(Canvas.TopProperty);

                        diagram.selectedState.startingArrow.SetValue(Canvas.LeftProperty, left - 19);
                        diagram.selectedState.startingArrow.SetValue(Canvas.TopProperty, top + 8);

                    }


                    Label stateLabel=(Label)(movingState.Tag);
                    
                    stateLabel.SetValue(Canvas.LeftProperty, movingState.GetValue(Canvas.LeftProperty));
                    stateLabel.SetValue(Canvas.TopProperty, movingState.GetValue(Canvas.TopProperty));

                    foreach (Transition t  in diagram.selectedState.transitions)
                    {
                        Path p=t.figure;
                        PathGeometry g = (PathGeometry)p.Data;
                        QuadraticBezierSegment q = (QuadraticBezierSegment)(((PathGeometry)p.Data).Figures[0].Segments[0]);
                        Point startingPt = g.Figures[0].StartPoint;
                        Point endingPt = q.Point2;

                        if (t.sourceState == t.destState) // to move the self loop 
                            g.Figures[0].StartPoint= q.Point2 = new Point(newPos.X + State.SIZE / 2, newPos.Y + State.SIZE / 2);
                        else if (t.sourceState == diagram.selectedState)
                            g.Figures[0].StartPoint = new Point(newPos.X + State.SIZE / 2, newPos.Y + State.SIZE / 2);
                        else
                            q.Point2 = new Point(newPos.X + State.SIZE / 2, newPos.Y + State.SIZE / 2);

                        Point pt = ((QuadraticBezierSegment)g.Figures[0].Segments[0]).Point1;

                        ((QuadraticBezierSegment)g.Figures[0].Segments[0]).Point1 = pt;

                        Point mid = new Point((startingPt.X + endingPt.X) / 2, (startingPt.Y + endingPt.Y) / 2);
                        Point mid2 = new Point((mid.X + pt.X) / 2, (mid.Y + pt.Y) / 2);
                        StackPanel sp = (StackPanel)t.figure.Tag;

                        sp.SetValue(Canvas.LeftProperty, mid2.X);
                        sp.SetValue(Canvas.TopProperty, mid2.Y);

                        Transition.addArrow(t, canvasMain);
                    }
                }
            }
        }


        private void btnH_Click(object sender, RoutedEventArgs e)
        {
            canvasMain.Width = canvasMain.ActualWidth + 200;
        }

        private void btnV_Click(object sender, RoutedEventArgs e)
        {
            canvasMain.Height = canvasMain.ActualHeight + 200;
        }

        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "";

            if (mode == modes.Machines)
            {
                if (loadedFilePath!="")
                    fileName = diagram.saveToFile(null, null, loadedFilePath);
                else
                    fileName = diagram.saveToFile();
            }
            else if (mode == modes.Grammar)
            {
                loadProductionsFromTextBoxes();

                if (grammar.productions.Count != 0)
                {
                    if (loadedFilePath != "")
                        fileName = grammar.saveToFile(null, null, loadedFilePath);
                    else
                        fileName = grammar.saveToFile();
                }
            }
            else if (mode == modes.Pda)
            {
                if (loadedFilePath != "")
                    fileName = pda.saveToFile(null, null, loadedFilePath);
                else
                    fileName = pda.saveToFile();
            }

            if (fileName != "")
            {
                fileStatus.isEdited = false;
                loadedFilePath = fileName;
                Application.Current.MainWindow.Title = Application.Current.MainWindow.Title + " - " + fileName;
            }
        }

        public void menuLoad_Click(object sender, RoutedEventArgs e)
        {
            string filename = "";

            if (sender is String) //file name passed via command line args   
            {
                canvasMain.Children.Clear();
                filename = (string)sender;
            }
            else
            {
                Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog();
                d.AddExtension = true;
                d.CheckFileExists = true;
                d.CheckPathExists = true;
                d.DefaultExt = "*.dfa";
                d.Filter = "DFA Machine (*.dfa)|*.dfa|NFA Machine (*.nfa)|*.nfa|Mealy Machine (*.ml)|*.ml|Moore Machine (*.mo)|*.mo|Turing Machine (*.tm)|*.tm|PDA (*.pda)|*.pda|Grammar (*.gmr)|*.gmr";

                if (d.ShowDialog() == false) // cancel button is pressed
                    return;

                filename = d.FileName;
            }

            if (isFiringFromStartScreen) // to disable the "do you want to clear" msg
            {
                isFiringFromStartScreen = false;
                canvasMain.Children.Clear();
            }

            if (saveOnNewOrExit() == MessageBoxResult.Cancel)
                return;

            XDocument doc = XDocument.Load(filename);

            switch (doc.Root.Attribute("type").Value)
            {
                case "Dfa":
                    newMachine(machines.Dfa);
                    break;
                case "Nfa":
                    newMachine(machines.Nfa);
                    break;
                case "Turing":
                    newMachine(machines.Turing);
                    break;
                case "Mealy":
                    newMachine(machines.Mealy);
                    break;
                case "Moore":
                    newMachine(machines.Moore);
                    break;
                case "Cfg":
                    menuGrammar_Click(menuGrammar, new RoutedEventArgs(RibbonApplicationSplitMenuItem.ClickEvent));
                    break;
                case "DPda":
                    menuDPda_Click(menuDPda, new RoutedEventArgs(RibbonApplicationSplitMenuItem.ClickEvent));
                    break;
                default:
                    MessageBox.Show("Invalid File");
                    return;
            }

            clearAllTextBoxes();

            if (mode == modes.Machines)
            {
                diagram.loadFromFile(doc);
                setInterfaceOnMachineLoad();
            }
            else if (mode == modes.Grammar)
            {
                grammar.loadFromFile(doc);
                loadGrammerIntoTextBoxes(grammar);
            }
            else if (mode == modes.Pda)
            {
                pda.loadFromFile(doc);
                setInterfaceOnPdaLoad();
            }

            if (filename != "")
            {
                fileStatus.isEdited = false;
                loadedFilePath = filename;
                Application.Current.MainWindow.Title = Application.Current.MainWindow.Title + " - " + filename;
            }

        }

        void clearAllTextBoxes()
        {
            txtInput.Text = "";
            txtInputSymbols.Text = "";
            txtOutput.Text = "";
            txtOutputSymbols.Text = "";
            txtRename.Text = "";
            txtStringToBeAccepted.Text = "";
        }

        

        private void canvasStartup_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
        }

        private void canvasStartup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void canvasStartup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void RibbonWindow_KeyUp(object sender, KeyEventArgs e)
        {
            // make sure in all textChanged event args e.handled is true and for those textboxes for whom no textchanged is defined
            // define it to make e.hanled=true
            if (mode == modes.Machines)
            {
                switch (e.Key)
                {
                    case Key.A:
                        if (btnSelect.IsEnabled)
                        {
                            btnSelect.IsChecked = true;
                            btnSelect_Click(btnSelect, null);
                        }
                        break;

                    case Key.B:
                        if (btnTransBreak.IsEnabled)
                            btnTransBreak_Click(btnTransBreak, null);
                        break;

                    case Key.C:
                        if(btnClear.IsEnabled)
                            btnClear_Click(btnClear, null);
                        break;

                    case Key.F:
                        if (btnFinal.IsEnabled)
                        {
                            btnFinal.IsChecked = !btnFinal.IsChecked;
                            btnFinal_Click(btnFinal, null);
                        }
                        break;

                    case Key.H:
                        if (btnStatistics.IsEnabled)
                            btnStatistics_Click(btnStatistics, null);
                        break;

                    case Key.I:
                        if (btnInitial.IsEnabled)
                        {
                            btnInitial.IsChecked = !btnInitial.IsChecked;
                            btnInitial_Click(btnInitial, null);
                        }
                        break;
                        
                    case Key.R:
                        if (btnTransReverse.IsEnabled)
                            btnTransReverse_Click(btnTransReverse, null);
                        break;
                        
                    case Key.S:
                        if (btnAddState.IsEnabled)
                        {
                            btnAddState.IsChecked = true;
                            btnAddState_Click(btnAddState, null); //to set the mode
                        }
                        break;

                    case Key.Delete:
                        if(btnDelSelection.IsEnabled)
                            btnDelSelection_Click(btnDelSelection, null);
                        break;

                    case Key.T:
                        if (btnAddTrans.IsEnabled)
                        {
                            btnAddTrans.IsChecked = true;
                            btnAddTrans_Click(btnAddTrans, null); //to set the mode
                        }
                        break;
                        
                    case Key.Down:
                        btnV_Click(btnV, null);
                        break;

                    case Key.Right:
                        btnH_Click(btnH, null);
                        break;
                }
            }

            else if (mode == modes.Pda)
            {
                switch (e.Key)
                {
                    case Key.C:
                        if (btnClear.IsEnabled)
                            btnClear_Click(btnClear, null);
                        break;
                        
                    case Key.Delete:
                        if (btnDelSelection.IsEnabled)
                            btnDelSelection_Click(btnDelSelection, null);
                        break;

                    case Key.Down:
                        btnV_Click(btnV, null);
                        break;

                    case Key.Right:
                        btnH_Click(btnH, null);
                        break;
                }
            }
            else if (mode == modes.Grammar)
            {

            }
        }

        private void keyUpEventHandled(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void menuExpotToImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Images (.jpg)|*.jpg";

            if (dlg.ShowDialog() == false)
                return;
            
            string filename = dlg.FileName;

            byte[] screenshot = canvasMain.GetJpgImage(1, 100);
            System.IO.FileStream fileStream = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite);
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(fileStream);
            binaryWriter.Write(screenshot);
            binaryWriter.Close();
        }


        private void btnConvertToMealy_Click(object sender, RoutedEventArgs e)
        {
            MealyTransitionDiagram diagTemp=null;

            if (diagram.states.Count != 0 && diagram.transitions.Count != 0)
                diagTemp = ((MooreTransitionDiagram)diagram).convertToMealy();
            else
                MessageBox.Show("Please draw the transition diagram first");

            if (diagTemp != null)//is null on errors
            {
                diagram = diagTemp;
                setInterfaceOnMachineLoad();
                  
                this.machine = machines.Mealy;
                btnConvertToMoore.Visibility = Visibility.Visible;
                btnConvertToMealy.Visibility = Visibility.Collapsed;
                this.ribbonMain.Title = "Mealy Machine";
                loadedFilePath = "";
            }
        }

        private void btnConvertToMoore_Click(object sender, RoutedEventArgs e)
        {
            MooreTransitionDiagram diagTemp = null;

            if (diagram.states.Count != 0 && diagram.transitions.Count != 0)
                diagTemp = (MooreTransitionDiagram)((MealyTransitionDiagram)diagram).convertToMoore();
            else
                MessageBox.Show("Please draw the transition diagram first");

            
            if (diagTemp != null)
            {
                diagram = diagTemp;
                setInterfaceOnMachineLoad();
                   
                this.machine = machines.Moore;

                btnConvertToMealy.Visibility = Visibility.Visible;
                btnConvertToMoore.Visibility = Visibility.Collapsed;
                this.ribbonMain.Title = "Moore Machine";
                loadedFilePath = "";
            }
        }

        private void btnPdaDelSelection_Click(object sender, RoutedEventArgs e)
        {
            fileStatus.isEdited = true;
            if (pda.rows.Count == 1)
            {
                pda.rows[0].cbAction.SelectedIndex = -1;
                pda.rows[0].cbInputSymbol.SelectedIndex = -1;
                pda.rows[0].cbStackTop.SelectedIndex = -1;
                pda.rows[0].txtCurrentState.Text = "";
                pda.rows[0].txtNextState.Text = "";
            }
            else
                pda.deleteSelectedRow();
        }

        private void removeDuplicateSymbols(TextBox txtBox)
        {
            string str = (String)txtBox.Text.Clone();
            txtBox.Text = "";
            foreach (String s in str.Split(';').Distinct<String>())
            {
                if (s != "")
                    txtBox.Text += s + ";";
            }

            txtBox.Text = txtBox.Text.Trim(';');
        }


        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            

            RoutedCommand saveCommand = new RoutedCommand();
            RoutedCommand saveAsCommand = new RoutedCommand();
            RoutedCommand openCommand = new RoutedCommand();
            RoutedCommand printCommand = new RoutedCommand();
            RoutedCommand newCommand = new RoutedCommand(); 
            
            saveCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(saveCommand, menuSave_Click));

            saveAsCommand.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(saveAsCommand, menuSaveAs_Click)); 

            openCommand.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(openCommand, menuLoad_Click));

            printCommand.InputGestures.Add(new KeyGesture(Key.P, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(printCommand,printCanvas));

            newCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(newCommand, menuNew_Click));


        }

        public void printCanvas(object sender, RoutedEventArgs e)
        {
            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() == true)
            {
                dialog.PrintVisual(canvasMain, "Figure");
            }
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Label_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Label)sender).Background = new SolidColorBrush(Color.FromRgb(128, 196, 35));
            e.Handled = true;
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Label)sender).Background = new SolidColorBrush(Color.FromRgb(129, 127, 127));
            e.Handled = true;
        }

        private void imgMainOpen_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Image)sender).Source = ((Image)this.Resources["open2"]).Source;
            e.Handled = true;
        }

        private void imgMainOpen_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Image)sender).Source = ((Image)this.Resources["open"]).Source;
            e.Handled = true;
        }

        private void lblMain_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) // event handler for main screen icons
        {
            isFiringFromStartScreen = false;

            canvasMain.Children.Clear();
            Label lbl = (Label)sender;
            switch (lbl.Content.ToString())
            { 
                case "DFA":
                    newMachine(machines.Dfa);
                    break;
                case "NFA":
                    newMachine(machines.Nfa);
                    break;
                case "Moore Machine":
                    newMachine(machines.Moore);
                    break;
                case "Mealy Machine":
                    newMachine(machines.Mealy);
                    break;
                case "Turing Machine":
                    newMachine(machines.Turing);
                    break;
                case "Grammar":
                    menuGrammar_Click(sender, new RoutedEventArgs(RibbonApplicationSplitMenuItem.ClickEvent));
                    break;
                case "PDA":
                    menuDPda_Click(sender, new RoutedEventArgs(RibbonApplicationSplitMenuItem.ClickEvent));
                    break;
            }
        }

        private void imgMainOpen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isFiringFromStartScreen = true;
            menuLoad_Click(menuLoad, new RoutedEventArgs(MenuItem.ClickEvent));
        }

        private void lblMainClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isFiringFromStartScreen = true;
            this.Close();
        }

        private void lblMainClose_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Label)sender).Background = new SolidColorBrush(Color.FromRgb(227, 6, 45));
            e.Handled = true;
        }

        private void lblMainHelp_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Label)sender).Background = new SolidColorBrush(Color.FromRgb(123, 206, 26));
            e.Handled = true;
        }

		private void lblMainHelp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(System.IO.Directory.GetCurrentDirectory()+"\\Help\\Frame\\index.html");
        }

        private void menuNew_Click(object sender, RoutedEventArgs e)
        {
            if (saveOnNewOrExit() == MessageBoxResult.Cancel)
                return;
            
            if (mode == modes.Machines)
                newMachine(machine);
            else if (mode == modes.Grammar)
                menuGrammar_Click(menuGrammar, new RoutedEventArgs(RibbonApplicationSplitMenuItem.ClickEvent));
            else if (mode == modes.Pda)
                menuDPda_Click(menuDPda, new RoutedEventArgs(RibbonApplicationSplitMenuItem.ClickEvent));
        }

        private void menuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "";
            if (mode == modes.Machines)
                fileName = diagram.saveToFile();
            else if (mode == modes.Grammar)
            {
                loadProductionsFromTextBoxes();
                if(grammar.productions.Count!=0)
                    fileName = grammar.saveToFile();
            }
            else if (mode == modes.Pda)
                fileName = pda.saveToFile();

            if (fileName != "")
            {
                loadedFilePath = fileName;
                fileStatus.isEdited = false;
                Application.Current.MainWindow.Title = Application.Current.MainWindow.Title + " - " + fileName;
            }
        }

        MessageBoxResult saveOnNewOrExit()
        {
            MessageBoxResult r = MessageBoxResult.Yes;

            if (mode == modes.Grammar) //don't ask user to save empty grammar
            {
                if (grammar.productions.Count == 0)
                {
                    canvasMain.Children.Clear();
                    canvasMain.Width = canvasMain.Height = double.NaN;
                    return r;
                }
            }

            if (loadedFilePath == "") //new unsaved file
            {
                if (canvasMain.Children.Count != 0)
                {
                    r = MessageBox.Show("Do you want to save the file", "save?", MessageBoxButton.YesNoCancel);
                    if (r == MessageBoxResult.Yes)
                        menuSaveAs_Click(null, null);
                    else if (r == MessageBoxResult.Cancel)
                        return r;

					if (loadedFilePath == "" && r != MessageBoxResult.No) //i.e. the user pressed cancel on savedialog
                        return MessageBoxResult.Cancel;

                    canvasMain.Children.Clear();//to remove the messgae "do you want to clear canvas" on new machine
                    canvasMain.Width = canvasMain.Height = double.NaN;
                }
            }
            else // unsaved changes
            {
                if (canvasMain.Children.Count != 0)
                {
                    if (fileStatus.isEdited)
                    {
                        r = MessageBox.Show("Do you want to save the changes to the file", "Save Changes?", MessageBoxButton.YesNoCancel);
                        if (r == MessageBoxResult.Yes)
                            menuSave_Click(null, null);
                        else if (r == MessageBoxResult.Cancel)
                            return r;
                    }
                    
                    canvasMain.Children.Clear();//to remove the messgae "do you want to clear canvas" on new machine
                    canvasMain.Width = canvasMain.Height = double.NaN;
                }
            }

            fileStatus.isEdited = false;
            return r;

        }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isFiringFromStartScreen)
                if (saveOnNewOrExit() == MessageBoxResult.Cancel)
                    e.Cancel = true;
        }

    }
    
}

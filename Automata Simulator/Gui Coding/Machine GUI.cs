using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Windows.Controls.Ribbon;

namespace Automata_Simulator
{
    public enum machines
    {
        Dfa, Nfa, Moore, Mealy, Turing, None
    }

    enum actions
    {
        addState, addTransitionStartingPt, addTransitionEndingPt, moveState, moveTrans, none, select
    }

    public partial class MainWindow : RibbonWindow
    {
        public TransitionDiagram diagram;

        public Pda pda;

        machines machine = machines.None;

        actions action = actions.none;

        void mutexClick(RibbonGroup grp, RibbonToggleButton current)
        {
            for (int i = 0; i < grp.Items.Count; i++)
            {
                RibbonToggleButton btn = (RibbonToggleButton)grp.Items[i];
                if (btn != current)
                    btn.IsChecked = false;
            }

            current.IsChecked = true;
        }

        actions getAction()
        {
            if (btnAddState.IsChecked == true)
                return actions.addState;
            else if (btnAddTrans.IsChecked == true)
            {
                if (action == actions.addTransitionEndingPt || action == actions.addTransitionStartingPt)
                    return action;
                else
                    return actions.addTransitionStartingPt;
            }
            else
                return actions.select;
        }

        private void btnAddState_Click(object sender, RoutedEventArgs e)
        {
            mutexClick((RibbonGroup)btnAddState.Parent, (RibbonToggleButton)sender);
            if (btnAddState.IsChecked == true)
                action = actions.addState;
            else
                action = actions.none;
        }

        private void btnAddTrans_Click(object sender, RoutedEventArgs e)
        {
            mutexClick((RibbonGroup)btnAddTrans.Parent, (RibbonToggleButton)sender);
            if (btnAddTrans.IsChecked == true)
                action = actions.addTransitionStartingPt;
            else
                action = actions.none;
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            mutexClick((RibbonGroup)btnSelect.Parent, (RibbonToggleButton)sender);
            if (sender is Ellipse || sender is Label)
                diagram.selectedState = diagram.getState(sender);
            if (sender is Path)
                diagram.selectedTransition = diagram.getTransition(sender);
            action = actions.select;
        }

        private void btnDelSelection_Click(object sender, RoutedEventArgs e)
        {
            if (diagram.selectedState != null)
            {
                fileStatus.isEdited = true;

                diagram.deleteState();
                btnFinal.IsChecked = btnInitial.IsChecked = false;
                grpStateOptions.IsEnabled = false;
            }
            else if (diagram.selectedTransition != null)
            {
                fileStatus.isEdited = true;

                diagram.deleteTransition();
                grpTransOptions.IsEnabled = false;
            }

            btnDelSelection.IsEnabled = false;
        }

        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            if (diagram.selectedState != null)
                if (txtRename.Text != "")
                {
                    fileStatus.isEdited = true;
                    ((Label)diagram.selectedState.figure.Tag).Content = txtRename.Text;
                }
        }

        State s1;// for storing the Starting state of a transition 

        public void setStateEventHandlers(State s)
        {
            s.onMouseUp(state_MouseLeftButtonUp);
            s.onMouseDown(state_MouseLeftButtonDown);
        }

        public void setTransitionEventHandlers(Transition t)
        {
            t.onMouseUp(transition_MouseLeftButtonUp);
            t.onMouseDown(transition_MouseLeftButtonDown);
            t.onMouseEnter(transition_MouseEnter);
            t.onMouseLeave(transition_MouseLeave);

        }

        private void updateUiOnStateSelectionChanged(bool isSelected)
        {
            if (isSelected == true)
            {
                updateUiOnTransitionSelectionChanged(false);
                grpStateOptions.IsEnabled = true;
                btnDelSelection.IsEnabled = true;
                btnNullClosure.IsEnabled = true;

                btnFinal.IsChecked = btnInitial.IsChecked = false; // deselect all

                switch (diagram.selectedState.type)
                {
                    case stateType.initial:
                        btnInitial.IsChecked = true;
                        break;
                    case stateType.final:
                        btnFinal.IsChecked = true;
                        break;
                    case stateType.both:
                        btnInitial.IsChecked = btnFinal.IsChecked = true;
                        break;
                }

            }
            else
            {
                btnNullClosure.IsEnabled = false;
                btnFinal.IsChecked = false;
                btnInitial.IsChecked = false;
                grpStateOptions.IsEnabled = false;
                btnDelSelection.IsEnabled = false;
            }
        }

        private void updateUiOnTransitionSelectionChanged(bool isSelected)
        {
            if (isSelected == true)
            {
                updateUiOnStateSelectionChanged(false);
                btnDelSelection.IsEnabled = true;
                grpTransOptions.IsEnabled = true;
            }
            else
            {
                btnDelSelection.IsEnabled = false;
                grpTransOptions.IsEnabled = false;
            }
        }

        void state_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (action == actions.addState)
                return;

            State currentState = diagram.getState(sender);

            if (currentState == null)
                return;

            if (action == actions.addTransitionStartingPt)
            {
                s1 = currentState;
                action = actions.addTransitionEndingPt;
            }
            else if (action == actions.addTransitionEndingPt)
            {
                State s2 = currentState; ;
                Transition t = diagram.addTransition(s1, s2);
                if (t != null)
                {
                    setTransitionEventHandlers(t);
                    diagram.mutexSelect(t);
                    updateUiOnTransitionSelectionChanged(true);

                    diagram.isDragging = true;
                    action = actions.moveTrans;
                    btnOk.IsEnabled = false;
                }
                else
                    action = actions.addTransitionStartingPt;

            }
            else
            {
                diagram.isDragging = false;
                action = getAction();
                //action = actions.select;
            }
        }

        void state_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (action == actions.select)// || action == actions.moveTrans)
            {
                State currentState = diagram.getState(sender);

                if (diagram.mutexSelect(currentState))
                {
                    action = actions.moveState;
                    diagram.isDragging = true;

                    updateUiOnStateSelectionChanged(true);

                }
                else
                {
                    updateUiOnStateSelectionChanged(false);
                }
            }

            e.Handled = true;
        }

        void transition_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            diagram.isDragging = false;
            e.Handled = true;

        }

        void transition_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (action == actions.select || action == actions.moveTrans)
            {
                Transition currentTrans = diagram.getTransition(sender);

                if (currentTrans == null)
                    return;

                if (diagram.mutexSelect(currentTrans))
                {
                    action = actions.moveTrans;
                    diagram.isDragging = true;
                    updateUiOnTransitionSelectionChanged(true);
                }
                else
                    diagram.isDragging = false;
            }


        }

        void transition_MouseLeave(object sender, MouseEventArgs e)
        {
            if (diagram.isDragging == false)
                canvasMain.Cursor = Cursors.Arrow;
        }

        void transition_MouseEnter(object sender, MouseEventArgs e)
        {
            if (action == actions.moveTrans || action == actions.select)
                canvasMain.Cursor = Cursors.ScrollAll;
        }

        public void newMachine(machines type, TransitionDiagram newDiagram = null)
        {
            if (newDiagram != null) // don't clear the canvas, i.e. we are only updating the interface on conversions etc
                ;
            else
            {
                if (saveOnNewOrExit() == MessageBoxResult.Cancel)
                    return;

                fileStatus.isEdited = false;
                loadedFilePath = "";

                grammar = null;
                pda = null;

                mode = modes.Machines;
                GrammarTab.Visibility = pdaTab.Visibility = Visibility.Collapsed;

                transitionDiagramTab.Visibility = Visibility.Visible;
                ExecuteTab.Visibility = Visibility.Visible;
                ribbonMain.Visibility = Visibility.Visible;
                transitionDiagramTab.IsSelected = true;

                clearAllTextBoxes();
                
            }

            //restore original settings

            btnFinal.IsChecked = btnInitial.IsChecked = false;
            btnAddState.IsChecked = btnAddTrans.IsChecked = btnSelect.IsChecked = false;
            btnNull.IsChecked = false;

            grpSymbols.IsEnabled = false;
            grpToolbox.IsEnabled = false;
            grpSelection.IsEnabled = false;

            btnStatistics.IsEnabled = false;

            grpStateOptions.IsEnabled = false;
            btnFinal.IsEnabled = true;// disabled in turing, moore, mealy

            grpTransOptions.IsEnabled = false;

            grpStringAcceptance.Visibility = Visibility.Collapsed;
            grpOutputProducer.Visibility = Visibility.Collapsed;

            btnNull.IsEnabled = false;


            //because they are changed in turing machine interface
            txtInput.Width = txtOutput.Width = 155; 

            lblInput.Content = "Input   "; //changed to input tape in turnig
            lblOutput.Content = "Output";//changed to output tape in turnig

            grpDfaOptions.Visibility = grpNfaOptions.Visibility = grpMooreMealyOptions.Visibility = Visibility.Collapsed;

            switch (type)
            {
                case machines.Dfa:
                    
                    machine = machines.Dfa;
                    if (newDiagram == null)
                        newDiagram = new DfaTransitionDiagram();

                    diagram = newDiagram;
                    
                    spInputSymbols.IsEnabled = true;
                    spOutputSymbols.IsEnabled = false;

                    grpDfaOptions.Visibility = Visibility.Visible;
                    grpStringAcceptance.Visibility = Visibility.Visible;
                    Application.Current.MainWindow.Title = "DFA";
                    break;
                
                case machines.Nfa:
                    machine = machines.Nfa;

                    if (newDiagram == null)
                        newDiagram = new NfaTransitionDiagram();

                    diagram = newDiagram;

                    btnNullClosure.IsEnabled = false;// to be enabled on state selection

                    spInputSymbols.IsEnabled = true;
                    spOutputSymbols.IsEnabled = false;
                    btnNull.IsEnabled = true;
                    

                    grpNfaOptions.Visibility = Visibility.Visible;
                    grpStringAcceptance.Visibility = Visibility.Visible;
                    Application.Current.MainWindow.Title = "NFA";
                    break;

                case machines.Moore:
                    machine = machines.Moore;

                    if (newDiagram == null)
                        newDiagram = new MooreTransitionDiagram();

                    diagram = newDiagram;

                    spInputSymbols.IsEnabled = true;
                    spOutputSymbols.IsEnabled = true;
                    btnFinal.IsEnabled = false;

                    grpMooreMealyOptions.Visibility = Visibility.Visible;
                    btnConvertToMoore.Visibility = Visibility.Collapsed;
                    btnConvertToMealy.Visibility = Visibility.Visible;
                    
                    grpMooreMealyOptions.Header = "Moore Options";

                    grpOutputProducer.Visibility = Visibility.Visible;
                    Application.Current.MainWindow.Title = "Moore Machine";
                    break;

                case machines.Mealy:
                    machine = machines.Mealy;

                    if (newDiagram == null)
                        newDiagram = new MealyTransitionDiagram();

                    diagram = newDiagram;

                    spInputSymbols.IsEnabled = true;
                    spOutputSymbols.IsEnabled = true;
                    btnFinal.IsEnabled = false;

                    grpMooreMealyOptions.Visibility = Visibility.Visible;
                    btnConvertToMoore.Visibility = Visibility.Visible;
                    btnConvertToMealy.Visibility = Visibility.Collapsed;
                    grpMooreMealyOptions.Header = "Mealy Options";

                    grpOutputProducer.Visibility = Visibility.Visible;
                    Application.Current.MainWindow.Title = "Mealy Machine";
                    break;

                case machines.Turing:
                    machine = machines.Turing;

                    if (newDiagram == null)
                        newDiagram = new TuringTransitionDiagram();

                    diagram = newDiagram;

                    lblInput.Content = "Input Tape   ";
                    lblOutput.Content = "Output Tape";
                
                    spInputSymbols.IsEnabled = true;
                    spOutputSymbols.IsEnabled = false;
                    btnFinal.IsEnabled = false;

                    grpOutputProducer.Visibility = Visibility.Visible;
                    txtInput.Width = txtOutput.Width = 500;
                    Application.Current.MainWindow.Title = "Turing Machine";
                    break;
                                   
                case machines.None:
                    return; // deselected above
            }

            
        
            //enable common options
            grpSymbols.IsEnabled = true;
            grpToolbox.IsEnabled = true;
            grpSelection.IsEnabled = true;
            btnStatistics.IsEnabled = true;
            btnAddTrans.IsEnabled = false;

            State.resetLabels();

            btnOk.IsEnabled = true;
            diagram.setCanvas(canvasMain);

            btnAddState.IsChecked = true;
            btnAddState_Click(btnAddState, null); // to set the mode as addState

        }

        private void btnOk_Click(object sender, RoutedEventArgs e) // i/o Symbols insertion
        {
            if ((txtInputSymbols.IsEnabled == true && txtInputSymbols.Text == "")
                || (txtOutputSymbols.IsEnabled == true && txtOutputSymbols.Text == ""))
            {
                MessageBox.Show("Enter Input/Output Symbols");
                return;// to stop acceptance of one text box even if the other is empty
            }

            if (txtInputSymbols.IsEnabled == true)
            {
                removeDuplicateSymbols(txtInputSymbols);

                diagram.noOfInputs = txtInputSymbols.Text.Split(';').Length;
                diagram.inputs = txtInputSymbols.Text.Split(';');
                spInputSymbols.IsEnabled = false;
                btnAddTrans.IsEnabled = true;
            }
            if (txtOutputSymbols.IsEnabled == true)
            {
                removeDuplicateSymbols(txtOutputSymbols);
                string[] outs = txtOutputSymbols.Text.Split(';');

                diagram.outputs = new List<string>(outs);
                spOutputSymbols.IsEnabled = false;

                btnAddTrans.IsEnabled = true;
            }

            btnOk.IsEnabled = false;
            btnNull.IsEnabled = false;
        }


        void blackenAllTransitions()
        {
            foreach (Transition t in diagram.transitions)
            {
                Path p = (Path)t.figure;
                p.Stroke = Brushes.Black;
                p.StrokeThickness = 1;
            }
        }

        void highlightTraversedPath(bool isAccepted)
        {
            SolidColorBrush color = Brushes.Red;
            if (isAccepted)
                color = Brushes.Green;

            
            if (diagram.transitionsOnTraversedPath == null)
                return;

            foreach (string str in diagram.transitionsOnTraversedPath)
            {
                int s1, s2;
                string input = "";
                    
                if (str[0] != ':')
                {
                    int i=0, j=0;//represent the position of 1st : and 2nd :
                        
                    int p=0;
                        
                    foreach (char c in str)
                    {
                        if (c == ':')
                        {
                            if (i == 0)
                                i = p;
                            else
                            {
                                j = p;
                                break;
                            }
                        }
                        p++;
                    }

                    s1 = int.Parse(str.Substring(0,i));
                    s2 = int.Parse(str.Substring(i + 1, j - i - 1));
                    input = str.Substring(j + 1);

                    foreach (Transition t in diagram.transitions)
                    {
                        if (t.sourceState.index == s1 && t.destState.index == s2 && t.input == input)
                        {
                            Path q = (Path)t.figure;
                            q.Stroke = color;
                            q.StrokeThickness = 3;
                        }
                    }
                }
            }
        }

        private void btnCheckAcceptance_Click(object sender, RoutedEventArgs e)
        {
            bool result;

            if(mode!=modes.Pda)
                blackenAllTransitions();

            if (txtStringToBeAccepted.Text == "")
            {
                txtStringToBeAccepted.Focus();
                return;
            }

            if (mode==modes.Pda)
            {
                if (pda.isErroneous())
                    return;
                result = pda.checkStringAcceptance(txtStringToBeAccepted.Text);
            }
            else
            {
                if (diagram.isErroneous())
                    return;
                result = diagram.checkStringAcceptance(txtStringToBeAccepted.Text);
            }

            if(mode!=modes.Pda)
                highlightTraversedPath(result);

            if (result == true)
            {
                lblAcceptanceResult.Foreground = Brushes.Green;
                lblAcceptanceResult.Content = "Accepted";
            }
            else
            {
                lblAcceptanceResult.Foreground = Brushes.Red;
                lblAcceptanceResult.Content = "Rejected";
            }

        }

        private void btnFinal_Click(object sender, RoutedEventArgs e)
        {
            fileStatus.isEdited = true;

            if (btnFinal.IsChecked == true)
            {
                diagram.selectedState.figure.Stroke = Brushes.Black;
                diagram.selectedState.figure.StrokeThickness = 2;

                diagram.finalStates.Add(diagram.selectedState);

                if (diagram.selectedState.type == stateType.initial)
                    diagram.selectedState.type = stateType.both;
                else
                    diagram.selectedState.type = stateType.final;
            }
            else
            {
                diagram.selectedState.figure.StrokeThickness = 0;

                diagram.finalStates.Remove(diagram.selectedState);
                if (diagram.selectedState.type == stateType.both)
                    diagram.selectedState.type = stateType.initial;
                else
                    diagram.selectedState.type = stateType.intermediate;
            }
        }

        private void btnInitial_Click(object sender, RoutedEventArgs e)
        {
            fileStatus.isEdited = true;

            if (btnInitial.IsChecked == true)
            {
                diagram.initialStates.Add(diagram.selectedState);
                diagram.selectedState.addStartingArrow(canvasMain);

                if (diagram.selectedState.type == stateType.final)
                    diagram.selectedState.type = stateType.both;
                else
                    diagram.selectedState.type = stateType.initial;
            }
            else
            {
                diagram.selectedState.removeStartingArrow(canvasMain);
                diagram.initialStates.Remove(diagram.selectedState);

                if (diagram.selectedState.type == stateType.both)
                    diagram.selectedState.type = stateType.final;
                else
                    diagram.selectedState.type = stateType.intermediate;
            }
        }

        private void txtStringToBeAccepted_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblAcceptanceResult.Content = "";
            if (chkExecuteAsIType.IsChecked == true)
            {
                btnCheckAcceptance_Click(btnCheckAcceptance, null);
            }
        }

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtOutput.Text = "";
            if (chkExecuteAsIType.IsChecked == true)
            {
                btnProduceOutput_Click(btnProduceOutput, null);
            }
        }

        private void btnProduceOutput_Click(object sender, RoutedEventArgs e)
        {
            if (!diagram.isErroneous())
            {
                blackenAllTransitions();
                if (txtInput.Text != "")
                {
                    txtOutput.Text = diagram.produceOutput(txtInput.Text);
                    highlightTraversedPath(true);
                }
            }
        }

        private void btnNullClosure_Click(object sender, RoutedEventArgs e)
        {
            if (!diagram.inputs.Contains("^"))
            {
                MessageBox.Show("The inputs don't contain ^ symbol");
                return;
            }

            List<State> nullClosure = ((NfaTransitionDiagram)diagram).nullClosure(diagram.selectedState);

            string msg = "{";
            foreach (State s in nullClosure)
                msg += ((Label)s.figure.Tag).Content.ToString() + ",";

            msg = msg.Trim(new char[] { ',' });
            msg += "}";

            MessageBox.Show(msg);
        }

        private void btnTransReverse_Click(object sender, RoutedEventArgs e)
        {
            if (diagram.selectedTransition != null)
            {
                fileStatus.isEdited = true;

                State s1 = diagram.selectedTransition.destState;
                State s2 = diagram.selectedTransition.sourceState;
                diagram.deleteTransition();

                Transition t = diagram.addTransition(s1, s2);
                
                if (t != null)
                {
                    setTransitionEventHandlers(t);
                    diagram.mutexSelect(t);
                    updateUiOnTransitionSelectionChanged(true);
                    diagram.isDragging = true;
                    action = actions.moveTrans;
                    btnOk.IsEnabled = false;
                }

            }

        }

        private void btnTransBreak_Click(object sender, RoutedEventArgs e)
        {
            if (diagram.selectedTransition != null)
            {
                fileStatus.isEdited = true;

                State s1 = diagram.selectedTransition.sourceState;
                State s2 = diagram.selectedTransition.destState;
                State s = diagram.addState(((QuadraticBezierSegment)((PathGeometry)diagram.selectedTransition.figure.Data).Figures[0].Segments[0]).Point1);

                diagram.deleteTransition();

                setStateEventHandlers(s);
                diagram.mutexSelect(s);
                updateUiOnStateSelectionChanged(true);

                Transition t = diagram.addTransition(s1, s);
                setTransitionEventHandlers(t);

                t = diagram.addTransition(s, s2);

                setTransitionEventHandlers(t);
            }
        }

        public void setInterfaceOnMachineLoad()
        {
            String str = "";
            for (int i = 0; i < diagram.noOfInputs; i++)
                str += diagram.inputs[i] + ";";

            txtInputSymbols.Text = str.Trim(';');
            
            btnOk.IsEnabled = false;
            btnNull.IsEnabled = false;

            if (txtInputSymbols.Text == "") //i.e. in case no input symbols were specified in the file
            {
                spInputSymbols.IsEnabled = true;
                btnOk.IsEnabled = true;
            }
            else
                spInputSymbols.IsEnabled = false;

            spOutputSymbols.IsEnabled = false;

            if (diagram.outputs != null)
            {
                String str2 = "";
                
                for (int i = 0; i < diagram.outputs.Count; i++)
                    str2 += diagram.outputs[i] + ";";
                
                txtOutputSymbols.Text = str2.Trim(';');

                if (txtOutputSymbols.Text == "")
                {
                    spOutputSymbols.IsEnabled = true;
                    btnOk.IsEnabled = true;
                }
            }

            if (machine == machines.Nfa)
                btnNull.IsEnabled = btnOk.IsEnabled;


            btnAddTrans.IsEnabled = true;
            

            grpTransOptions.IsEnabled = false;

            foreach (State s in diagram.states)
                setStateEventHandlers(s);

            foreach (Transition t in diagram.transitions)
                setTransitionEventHandlers(t);

        }

        private void btnNull_Click(object sender, RoutedEventArgs e)
        {
            //firstly remove all null symbols

            int i = txtInputSymbols.Text.IndexOf('^');

            while (i != -1)
            {
                txtInputSymbols.Text = txtInputSymbols.Text.Remove(i, 1);
                i = txtInputSymbols.Text.IndexOf('^');
            }

            removeDuplicateSymbols(txtInputSymbols);

            if (btnNull.IsChecked == true) // add one ^
                txtInputSymbols.Text = "^;" + txtInputSymbols.Text;
        }

        private void btnNfaToDfa_Click(object sender, RoutedEventArgs e)
        {
            if (diagram.isErroneous())
                return;

            canvasMain.Children.Clear(); // to supress the message: canvas not empty

            diagram = ((NfaTransitionDiagram)diagram).convertToDfa();
            newMachine(machines.Dfa, diagram);//update the interface according to DFA 
            setInterfaceOnMachineLoad();
            loadedFilePath = "";
        }

        private void btnRemoveNullMoves_Click(object sender, RoutedEventArgs e)
        {
            //save a temporary file of the original one and then create a button to view converted one (which is another temp file) and original one

            //string temp = "D:/" + "__temp_" + System.DateTime.Now.Ticks.ToString()+".nfa";

            //diagram.saveToFile(name: temp);

            diagram = ((NfaTransitionDiagram)diagram).removeNullMoves();
            setInterfaceOnMachineLoad();
            fileStatus.isEdited = true;
        }

        private void btnStatistics_Click(object sender, RoutedEventArgs e)
        {
            if (diagram != null)
            {
                string str = "";
                str += "No. of States: " + diagram.states.Count.ToString();
                str += "\nNo. of Transitions: " + diagram.transitions.Count.ToString();
                str += "\nInput Alpahabet: {" + txtInputSymbols.Text.Replace(';', ',') + "}";
                MessageBox.Show(str, "Machine Statistics");
            }
        }

        private void btnMinimizeDfa_Click(object sender, RoutedEventArgs e)
        {
            if (diagram.isErroneous())
                return;

            DfaTransitionDiagram diagTemp = ((DfaTransitionDiagram)diagram).minimizeStates();
            if (diagTemp != null)//is null on errors
            {
                diagram = diagTemp;
                setInterfaceOnMachineLoad();
                fileStatus.isEdited = true;
            }
        }

        private void btnDfaComplement_Click(object sender, RoutedEventArgs e)
        {
            foreach (State s in diagram.states)
            {
                switch (s.type)
                {
                    case stateType.initial:
                        s.type = stateType.both;
                        s.figure.Stroke = Brushes.Black;
                        s.figure.StrokeThickness = 2;
                        diagram.finalStates.Add(s);
                        break;
                    case stateType.final:
                        s.type = stateType.intermediate;
                        s.figure.StrokeThickness = 0;
                        diagram.finalStates.Remove(s);
                        break;
                    case stateType.intermediate:
                        s.type = stateType.final;
                        s.figure.Stroke = Brushes.Black;
                        s.figure.StrokeThickness = 2;
                        diagram.finalStates.Add(s);
                        break;
                    case stateType.both:
                        s.type = stateType.initial;
                        s.figure.StrokeThickness = 0;
                        diagram.finalStates.Remove(s);
                        break;
                }
            }

            fileStatus.isEdited = true;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (canvasMain.Children.Count == 0)
                return;
            else
            {
                if (MessageBox.Show("Are you sure to clear the Page", "Clear?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }

            grpStateOptions.IsEnabled = false;
            grpTransOptions.IsEnabled = false;
            btnInitial.IsChecked = btnFinal.IsChecked = false;

            TransitionDiagram newDiag = null;
            
            switch (machine)
            {
                case machines.Dfa:
                    newDiag = new DfaTransitionDiagram();
                    break;
                case machines.Nfa:
                    newDiag = new NfaTransitionDiagram();
                    break;
                case machines.Moore:
                    newDiag = new MooreTransitionDiagram();
                    break;
                case machines.Mealy:
                    newDiag = new MealyTransitionDiagram();
                    break;
                case machines.Turing:
                    newDiag = new TuringTransitionDiagram();
                    break;
            }

            if (newDiag != null)
            {
                newDiag.inputs = diagram.inputs;
                newDiag.noOfInputs = diagram.noOfInputs;
                newDiag.outputs = diagram.outputs;
                canvasMain.Children.Clear();
                newDiag.setCanvas(canvasMain);
                diagram = newDiag;
            }

        }

        private void menuDfa_Click(object sender, RoutedEventArgs e)
        {
            newMachine(machines.Dfa);
            e.Handled = true;
        }

        private void menuNfa_Click(object sender, RoutedEventArgs e)
        {
            newMachine(machines.Nfa);
            e.Handled = true;
        }

        private void menuMealy_Click(object sender, RoutedEventArgs e)
        {
            newMachine(machines.Mealy);
            e.Handled = true;
        }

        private void menuMoore_Click(object sender, RoutedEventArgs e)
        {
            newMachine(machines.Moore);
            e.Handled = true;
        }

        private void menuTuring_Click(object sender, RoutedEventArgs e)
        {
            newMachine(machines.Turing);
            e.Handled = true;
        }
    }
    

}

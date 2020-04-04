using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Media;

namespace Automata_Simulator
{
    class NfaTransitionDiagram : TransitionDiagram
    {
        List<State>[,] table = new List<State>[MAX, MAX];// [source state, input alphabet] -> dest states

        public override Transition addTransition(State s1, State s2, String point1=null)
        {
            // Tags Info:
            // mypath -> sp 
            // cb -> transition

            Transition transition = new nfaTransition();

            generateTransition(s1, s2, transition,point1);

            return transition;

        }

        public override Transition getTransition(object sender)
        {
            if (sender is Path)
            {
                Path p = (Path)sender;
                return (Transition)((StackPanel)p.Tag).Tag;
            }
            else if (sender is ComboBox)
            {
                return (Transition)((ComboBox)sender).Tag;
            }
            else
                return null;
        }

        public override void onInputChange(object sender, SelectionChangedEventArgs e)
        {
            fileStatus.isEdited = true;

            ComboBox cb = (ComboBox)sender;

            Transition t = (Transition)cb.Tag;
            t.input = cb.SelectedValue.ToString();

            int i = getIndexOfInput(cb.SelectedValue.ToString());

            if (e.RemovedItems.Count == 0)// first time selection
                ;
            else // selection changed
            {
                int j = getIndexOfInput(e.RemovedItems[0].ToString());
                e.RemovedItems.Clear();
                table[t.sourceState.index, j].Remove(t.destState);
            }

            if (table[t.sourceState.index, i] == null)
            {
                List<State> a = new List<State>();
                a.Add(t.destState);
                table[t.sourceState.index, i] = a;
            }
            else
                table[t.sourceState.index, i].Add(t.destState);

        }

        public bool isFinal(State s)
        {
            if (s == null)
                return false;

            foreach (State e in finalStates)
                if (e.index == s.index)
                    return true;

            return false;
        }

        public override bool isErroneous()
        {
            if (initialStates==null || initialStates.Count==0)
            {
                MessageBox.Show("Error: Set the Initial State");
                return true;
            }

            if (finalStates == null || finalStates.Count == 0)
            {
                MessageBox.Show("Error: Set the Final State");
                return true;
            }

            return base.isErroneous();

        }

        public override bool checkStringAcceptance(string str)
        {
            transitionsOnTraversedPath = null;
            
            if (getIndexOfInput("^") == -1) //without null moves
            {
                if (getIndexOfInput(str[0].ToString()) == -1)
                    return false;

                foreach (State initialState in this.initialStates)
                {
                    transitionsOnTraversedPath = new List<string>();
                    List<State> a = table[initialState.index, getIndexOfInput(str[0].ToString())];
                    if (a == null)
                        continue;

                    foreach (State s in a)
                        transitionsOnTraversedPath.Add(initialState.index.ToString() + ":" + s.index.ToString() + ":" + str[0].ToString());//source:dest:char

                    if (str.Length == 1)
                    {
                        foreach (State s in a)
                        {
                            if (isFinal(s))
                                return true;
                        }
                    }

                    int indexInsideString = 1; //index of the next character to be scanned

                    List<State> currentStates = a;

                    while (indexInsideString < str.Length)
                    {
                        List<State> temp = new List<State>();

                        foreach (State s in currentStates)
                        {
                            int i = getIndexOfInput(str[indexInsideString].ToString());
                            
                            if (i == -1)
                                return false;
                            
                            List<State> d = table[s.index, i];
                            if (d != null)
                            {
                                foreach (State item in d)
                                {
                                    transitionsOnTraversedPath.Add(s.index.ToString() + ":" + item.index.ToString()+":"+str[indexInsideString].ToString());
                                    temp.Add(item);
                                }
                            }
                        }

                        currentStates = temp;
                        indexInsideString++;
                    }

                    foreach (State s in currentStates)
                    {
                        if (isFinal(s))
                            return true;
                    }
                }

                return false;
            }
            else // with null moves
            {
                foreach (State initialState in this.initialStates)
                {
                    transitionsOnTraversedPath = new List<string>();
                    List<State> a = nullClosure(initialState,true);

                    if (str.Length == 0)
                    {
                        if (a == null)
                            continue;
                        else
                        {
                            foreach (State s in a)
                                if (isFinal(s))
                                    return true;

                            return false;
                        }
                    }

                    if (getIndexOfInput(str[0].ToString()) == -1)
                        return false;

                    int indexInsideString = 0; //index of the next character to be scanned

                    List<State> currentStates = a;

                    while (indexInsideString < str.Length)
                    {
                        List<State> temp = new List<State>();

                        foreach (State s in currentStates)
                        {
                            int i = getIndexOfInput(str[indexInsideString].ToString());
                            
                            if (i == -1)
                                return false;
                            
                            List<State> d = table[s.index, i];
                            
                            if (d != null)
                            {
                                foreach (State item in d)
                                {
                                    transitionsOnTraversedPath.Add(s.index.ToString() + ":" + item.index.ToString() + ":" + str[indexInsideString].ToString());

                                    foreach (State st in nullClosure(item,true))
                                    {
                                        //if (st.index == item.index)
                                        //  continue;
                                        if (!temp.Contains(st))
                                            temp.Add(st);
                                    }
                                    //temp.Add(item);
                                }
                            }
                        }

                        currentStates = temp;
                        indexInsideString++;
                    }

                    foreach (State s in currentStates)
                        if (isFinal(s))
                            return true;

                }

                return false;

            }

        }

        public List<State> nullClosure(State state, bool recordTraversedTransitions = false )
        {
            //arg info:
            //recordTraversedTransitions : used for the implementation of highlighting traversed edges
            
            List<State> returnValue = new List<State>();
            returnValue.Add(state);

            int i=getIndexOfInput("^");

            if (table[state.index, i] != null)
            {
                List<State> temp = table[state.index, i];
                List<State> temp2 = new List<State>();

                foreach (State s in temp)
                {
                    if(recordTraversedTransitions)
                        transitionsOnTraversedPath.Add(state.index.ToString() + ":" + s.index.ToString() + ":" + "^");
                    
                    returnValue.Add(s);
                }
                
                while (temp.Count!=0)
                {
                    foreach (State s in temp)
                    {
                        List<State> nextStates = table[s.index,i];

                        if (nextStates != null)
                        {
                            foreach (State st in nextStates)
                            {
                                if(recordTraversedTransitions)
                                    transitionsOnTraversedPath.Add(s.index.ToString() + ":" + st.index.ToString() + ":" + "^");

                                if (!(temp.Contains(st) || returnValue.Contains(st)))
                                {
                                    if (!temp2.Contains(st))
                                    {
                                        temp2.Add(st);
                                        returnValue.Add(st);
                                    }
                                }
                            }
                        }
                    }

                    temp = temp2;
                    temp2 = new List<State>();
                }
            }

            return returnValue;
            
        }

        public NfaTransitionDiagram removeNullMoves()
        {
            if (!this.inputs.Contains("^"))
                return this;

            bool conatainNullMoves = false;
            //if ^ is present in input alphabet but there are no null transitions
            foreach (Transition t in this.transitions)
            {
                if (t.input == "^")
                {
                    conatainNullMoves = true;
                    break;
                }
            }

            if (!conatainNullMoves)
            {
                MessageBox.Show("The diagram contains no ^ Moves");
                return this;
            }

            NfaTransitionDiagram newDiagram = new NfaTransitionDiagram();
            canvasMain.Children.Clear();
            newDiagram.setCanvas(canvasMain);

            string temp="";

            if (!this.inputs.Contains<string>("^"))
            {
                newDiagram.inputs = inputs;
            }
            else
            {
                foreach (string s in inputs)
                {
                    if (s != "^")
                        temp += s + ";";
                }
                newDiagram.inputs = temp.Trim(';').Split(';');
                newDiagram.noOfInputs = temp.Trim(';').Split(';').Length;
            }

            List<List<State>> newStates = new List<List<State>>();
            List<List<State>> tempp = new List<List<State>>();


            foreach (State initialState in initialStates)
                tempp.Add(nullClosure(initialState));

            newStates.Add(tempp[0]);

            for (int j = 1; j < tempp.Count; j++)
            {
                List<State> ls = tempp[j];
        
                foreach (State s in ls)
                {
                    if (!newStates[0].Contains(s))
                        newStates[0].Add(s);
                }
            }

            //add the initial state

            State iState = newDiagram.addState(State.getNewStatePosition());
            iState.addStartingArrow(canvasMain);
            newDiagram.initialStates.Add(iState);
            iState.type = stateType.initial;

            if (containsFinalState(newStates[0]))
            {
                newDiagram.finalStates.Add(iState);
                iState.type = stateType.both;
                iState.figure.Stroke = Brushes.Black;
                iState.figure.StrokeThickness = 2;
            }

            string tooltip = getToolTip(newStates[0]);
            
            iState.figure.ToolTip = tooltip;
            newDiagram.selectedState = iState;

            int i = -1;

            while (i < newStates.Count-1)
            {
                List<State> currentState = newStates[++i];

                newDiagram.selectedState= getStateByToolTip(newDiagram,getToolTip(currentState));

                foreach (string input in newDiagram.inputs)
                {
                    List<State> nextState = new List<State>();

                    List<State> next=new List<State>();
                        
                    foreach (State s in currentState)
                    {
                        if (table[s.index, getIndexOfInput(input)] != null)
                        {
                            foreach (State t in table[s.index, getIndexOfInput(input)])
                            {
                                if (!next.Contains(t))
                                    next.Add(t);
                            }
                        }

                    }

                    // now next contains the next state but no null closures are found yet 

                    if (next.Count == 0)
                        nextState = next;
                    else
                    {
                        foreach (State st in next)
                        {
                            foreach (State p in nullClosure(st))
                            {
                                if (!nextState.Contains(p))
                                    nextState.Add(p);
                            }
                        }
                    }

                    tooltip = getToolTip(nextState);

                    if (tooltip != "")
                    {
                        State x = getStateByToolTip(newDiagram, tooltip);

                        if (x == null) //nextState do not exists in the newStates or newDiagram.States
                        {
                            // a new state is found
                            State s = newDiagram.addState(State.getNewStatePosition());
                            s.figure.ToolTip = tooltip;

                            if (containsFinalState(nextState))
                            {
                                s.type = stateType.final;
                                newDiagram.finalStates.Add(s);
                                s.figure.Stroke = Brushes.Black;
                                s.figure.StrokeThickness = 2;
                            }

                            Point pt1 = new Point();

                            Random rnd = new Random();
                            pt1.X = (double)rnd.Next((int)canvasMain.ActualWidth);
                            pt1.X = (double)rnd.Next((int)canvasMain.ActualHeight);

                            Transition tr = newDiagram.addTransition(newDiagram.selectedState, s, pt1.ToString());
                            ((ComboBox)((StackPanel)tr.figure.Tag).Children[0]).SelectedItem = input;

                            newStates.Add(nextState);
                        }
                        else
                        { 
                            Transition tr = newDiagram.addTransition(newDiagram.selectedState, x);
                            if(tr!=null)
                                ((ComboBox)((StackPanel)tr.figure.Tag).Children[0]).SelectedItem = input;
                        }
                    }

                }

            }

            return newDiagram;
        }

        #region functions to be used only for removing null moves

        private String getToolTip(List<State> states)
        {
            string tooltip = "";
            foreach (State t in states)
            {
                tooltip += t.figure.ToolTip + " ";
            }

            return tooltip.Trim();
        
        }

        private State getStateByToolTip(TransitionDiagram newDiagram, string tip)
        {
            foreach (State  s in newDiagram.states)
            {
                if (s.figure.ToolTip.ToString() == tip)
                    return s;
            }

            return null;
        
        }

        private bool containsFinalState(List<State> states)
        {
            foreach (State s in states )
            {
                if (s.type == stateType.final || s.type==stateType.both)
                    return true;
            }
            return false;
        
        }

        #endregion

        public DfaTransitionDiagram convertToDfa()
        {
            NfaTransitionDiagram diagram = this.removeNullMoves();

            DfaTransitionDiagram newDiagram = new DfaTransitionDiagram();
            newDiagram.setCanvas(canvasMain);
            canvasMain.Children.Clear();

            newDiagram.inputs = diagram.inputs;
            newDiagram.noOfInputs = diagram.noOfInputs;

            State.resetPoints(); // because the some state positions have been consumed by removeNullMoves(), so reset them 

            //add the initial state
            State iState = newDiagram.addState(State.getNewStatePosition());
            iState.addStartingArrow(canvasMain);
            iState.type = diagram.initialStates[0].type;
            newDiagram.initialStates.Add(iState);
            
            if (iState.type == stateType.both)
            {
                newDiagram.finalStates.Add(iState);
                iState.figure.Stroke = Brushes.Black;
                iState.figure.StrokeThickness = 2;                
            }

            iState.figure.ToolTip = diagram.initialStates[0].figure.ToolTip;
            newDiagram.selectedState = iState;

            List<List<State>> newStates = new List<List<State>>();

            List<State> temp = new List<State>();
            temp.Add(iState);
            newStates.Add(temp);

            
            string tooltip;

            int i = -1;

            while (i < newStates.Count - 1)
            {
                List<State> currentState = newStates[++i];

                newDiagram.selectedState = getStateByToolTip(newDiagram, getToolTip(currentState));

                foreach (string input in newDiagram.inputs)
                {
                    List<State> nextState = new List<State>();

                    foreach (State s in currentState)
                    {
                        if (diagram.table[s.index, diagram.getIndexOfInput(input)] != null)
                        {
                            foreach (State t in diagram.table[s.index, diagram.getIndexOfInput(input)])
                            {
                                if (!nextState.Contains(t))
                                    nextState.Add(t);
                            }
                        }
                    }

                    tooltip = getToolTip(nextState);

                    if (tooltip != "")
                    {
                        State x = getStateByToolTip(newDiagram, tooltip);

                        if (x == null) //nextState do not exists in the newStates or newDiagram.States
                        {
                            // a new state is found
                            State s = newDiagram.addState(State.getNewStatePosition());
                            s.figure.ToolTip = tooltip;

                            if (containsFinalState(nextState))
                            {
                                s.type = stateType.final;
                                newDiagram.finalStates.Add(s);
                                s.figure.Stroke = Brushes.Black;
                                s.figure.StrokeThickness = 2;
                            }

                            Point pt1 = new Point();

                            Random rnd = new Random();
                            pt1.X = (double)rnd.Next((int)canvasMain.ActualWidth);
                            pt1.X = (double)rnd.Next((int)canvasMain.ActualHeight);

                            Transition tr = newDiagram.addTransition(newDiagram.selectedState, s, pt1.ToString());
                            ((ComboBox)((StackPanel)tr.figure.Tag).Children[0]).SelectedItem = input;

                            newStates.Add(nextState);
                        }
                        else
                        {
                            Transition tr = newDiagram.addTransition(newDiagram.selectedState, x);
                            if (tr != null)
                                ((ComboBox)((StackPanel)tr.figure.Tag).Children[0]).SelectedItem = input;
                        }
                    }

                }

            }

            return newDiagram;

        }

        public override string produceOutput(string input)
        {
            // not applicable
            return null;
        }

        public override void deleteState()
        {
            foreach (Transition t in selectedState.transitions)
            {
                int i = getIndexOfInput(t.input);
                if (i >= 0)// if nothing is selected i is -1
                    table[t.sourceState.index, i] = null;
            }

            base.deleteState();

        }

        public override void deleteTransition()
        {
            int i = getIndexOfInput(selectedTransition.input);
            if (i >= 0) // if nothing is selected i is -1
                table[selectedTransition.sourceState.index, i] = null;

            base.deleteTransition();
        }

        public override string saveToFile(XDocument xd = null, XElement root = null, string name="")
        {
            //Arg description:
            //Name :  to generate temp files automatically without requiring users to enter filename or to save the opened file

            if (name == "")
            {
                Microsoft.Win32.SaveFileDialog d = new Microsoft.Win32.SaveFileDialog();
                d.AddExtension = true;
                d.CheckPathExists = true;
                d.DefaultExt = "nfa";
                d.Filter = "NFA files (*.nfa)|*.nfa";
                d.OverwritePrompt = true;
                if (d.ShowDialog() != false) // cancel button is not pressed
                    name = d.FileName;
                else
                    return "";
            }

            xd = new XDocument();
            root = new XElement("Machine");
            root.Add(new XAttribute("type", "Nfa"));

            base.saveToFile(xd, root);
            xd.Save(name);
            return name;

        }

        public override void loadFromFile(XDocument doc)
        {
            if(doc.Root.Attribute("type").Value=="Nfa")
                base.loadFromFile(doc);
        }


    }
}

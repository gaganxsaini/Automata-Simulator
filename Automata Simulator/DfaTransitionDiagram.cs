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
    class DfaTransitionDiagram : TransitionDiagram //strict DFA
    {
        State[,] table = new State[MAX, MAX];// [source state, input alphabet] -> dest state

        public override Transition addTransition(State s1, State s2, String point1 = null)
        {
            // Tags Info:
            // mypath -> sp 
            // cb -> transition

            int count = 0;
            foreach (Transition t in s1.transitions)
            {
                if (t.sourceState == s1)
                    count++;
            }

            if (count < noOfInputs)
            {
                Transition transition = new nfaTransition();
                generateTransition(s1, s2, transition, point1);
                return transition;
            }
            else
            {
                MessageBox.Show("In DFA you can add only one transition corresponding to one input symbol");
                return null;
            }

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

        public override bool isErroneous()
        {
            if (initialStates == null || initialStates.Count == 0)
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

        public bool isFinal(State s)
        {
            if (s == null)
                return false;

            foreach (State e in finalStates)
                if (e.index == s.index)
                    return true;

            return false;
        }

        public override void onInputChange(object sender, SelectionChangedEventArgs e)
        {
            fileStatus.isEdited = true;

            ComboBox cb = (ComboBox)sender;

            // to supress calling of this event the second time after setting cb.selectedIndex=-1 when error occurs    
            if (cb.SelectedIndex == -1)
                return;

            Transition transition = (Transition)cb.Tag;

            bool isError = false;

            foreach (Transition t in transition.sourceState.transitions)
            {
                if (t.sourceState == transition.sourceState)
                {
                    if (t.input == cb.SelectedValue.ToString())
                    {
                        isError = true;
                        break;
                    }
                }
            }

            if (isError == true)
            {
                MessageBox.Show("There can't be more than one state having same input alphabet in a DFA");
                cb.SelectedIndex = -1;

                //when after selecting a correct input, some invalid one is chosen
                transition.input = null;
            }
            else
            {
                transition.input = cb.SelectedValue.ToString();

                int i = getIndexOfInput(cb.SelectedValue.ToString());

                if (e.RemovedItems.Count == 0)// first time selection
                    ;
                else // selection changed
                {
                    int j = getIndexOfInput(e.RemovedItems[0].ToString());
                    table[transition.sourceState.index, j] = null;
                }
             
                table[transition.sourceState.index, i] = transition.destState;
            }
        }

        public override bool checkStringAcceptance(string str)
        {
            transitionsOnTraversedPath = null;

            if (getIndexOfInput(str[0].ToString()) == -1)
                return false;

            foreach (State initialState in this.initialStates)
            {
                transitionsOnTraversedPath = new List<string>();
                
                State s = table[initialState.index, getIndexOfInput(str[0].ToString())];
                
                
                if (s == null)
                    continue;
                
                transitionsOnTraversedPath.Add(initialState.index.ToString() + ":" + s.index.ToString() + ":" + str[0].ToString());

                if (str.Length == 1)
                {
                    if (isFinal(s))
                        return true;
                }

                int indexInsideString = 1; //index of the next character to be scanned

                while (indexInsideString < str.Length)
                {
                    if (s == null)
                        break;
                    int i = getIndexOfInput(str[indexInsideString].ToString());
                    if (i == -1)
                        return false;

                    string traversedPath = s.index.ToString();

                    s = table[s.index, i];

                    if (s != null)
                        transitionsOnTraversedPath.Add(traversedPath + ":" + s.index.ToString() + ":" + str[indexInsideString].ToString());
                    
                    indexInsideString++;
                }

                if (isFinal(s))
                    return true;
            }

            return false;
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

        public DfaTransitionDiagram minimizeStates()
        {
            List<List<State>> pi = new List<List<State>>();
            
            //initialize pi
            pi.Add(this.finalStates);

            List<State> temp = new List<State>();
            //find non final states
            foreach (State s in this.states)
            {
                if (s.type == stateType.intermediate || s.type == stateType.initial)
                    temp.Add(s);
            }

            if (temp.Count != 0)
                pi.Add(temp);

            //now pi has two sets: finals and non-finals

            if (pi.Count == 1) // i.e. pi has only one set because all the states are either final or both (initial & final)
            {
                MessageBox.Show("The diagram is already minimized");
                return this;
            }
            
            DfaTransitionDiagram newDiagram = new DfaTransitionDiagram();
            newDiagram.inputs = this.inputs;
            newDiagram.noOfInputs = this.noOfInputs;
            newDiagram.setCanvas(canvasMain);
            

            List<List<State>> newPi = pi; // for the initial condition 
            List<List<State>> set;
            
            while (true)
            {
                pi = newPi;
                set = pi;
                List<List<State>> piTemp = null;
                foreach (String input in inputs)
                {
                    piTemp = new List<List<State>>();

                    foreach (List<State> ls in set)
                    {
                        if (ls.Count == 1)
                        {
                            //beacuse if a set contains only a single item then it will remain single
                            piTemp.Add(new List<State>());
                            piTemp.Last().Add(ls[0]);
                            continue;
                        }

                        List<int> abc = new List<int>();

                        foreach (State s in ls)
                        {
                            State stt=table[s.index, getIndexOfInput(input)];
                            if (stt == null)
                            {
                                MessageBox.Show("To minimize a DFA, For each State you must define transitions for all input symbols"); 
                                return null;
                            }
                            else
                                abc.Add(piTemp.Count + getSetIndex(pi, stt));
                        }

                        //pitemp.count is added because before that it was problematic for the iteration 
                        //with the next set on same input symbol due to same 0 1 etc in abc

                        for (int i = 0; i < abc.Count; i++)
                            piTemp.Add(new List<State>());

                        while (piTemp.Count <= abc.Max())
                        {
                            piTemp.Add(new List<State>());
                        }

                        int j = 0;
                        foreach (int i in abc)
                        {
                            piTemp[i].Add(ls[j]);
                            j++;
                        }


                        // remove empty lists
                        List<List<State>> t = new List<List<State>>(); // to bypass the collection modified error 
                        foreach (List<State> ls2 in piTemp)
                            if (ls2.Count == 0)
                                t.Add(ls2);

                        foreach (List<State> ls2 in t)
                            piTemp.Remove(ls2);

                    }

                    set = piTemp;
                }

                newPi = piTemp;

                if(isPiEqualsNewPi(pi,newPi))
                    break;
            }

            canvasMain.Children.Clear();

            // now create a new diagram by taking states from pi

            foreach (List<State> ls in pi)
            {
                State s =  newDiagram.addState(State.getNewStatePosition());
                s.figure.ToolTip = getToolTip(ls);
                if (containsInitialState(ls))
                {
                    s.addStartingArrow(canvasMain);
                    newDiagram.initialStates.Add(s);
                    s.type = stateType.initial;

                    if (containsFinalState(ls))
                    {
                        s.type = stateType.both;
                        newDiagram.finalStates.Add(s);
                        s.figure.Stroke = Brushes.Black;
                        s.figure.StrokeThickness = 2;
                    }
                    
                }
                else if (containsFinalState(ls))
                {
                    s.type = stateType.final;
                    newDiagram.finalStates.Add(s);
                    s.figure.Stroke = Brushes.Black;
                    s.figure.StrokeThickness = 2;
                }

            }

            foreach (List<State> ls in pi)
            {
                State source = ls[0];
                foreach (string input in newDiagram.inputs)
                {
                    State dest = table[ls[0].index, getIndexOfInput(input)];
                    if (dest != null)
                    {
                        source = getStateByToolTip(newDiagram, getToolTip(ls));

                        foreach (List<State> ls2 in pi)
                        {
                            if (ls2.Contains(dest))
                            {
                                dest = getStateByToolTip(newDiagram, getToolTip(ls2));
                                break;
                            }
                        }

                        if (source != null && dest != null)
                        {
                            Transition tr = newDiagram.addTransition(source, dest);
                            if (tr != null)
                                ((ComboBox)((StackPanel)tr.figure.Tag).Children[0]).SelectedItem = input;
                        }
                    }
                }
            }

            //code inserted later to patch the flaws in algorithm

            bool isDeletedSomeState = true;

            while (isDeletedSomeState)
            {
                isDeletedSomeState = false;
                
                bool isOrphanState;

                List<State> tmp = new List<State>(newDiagram.states);

                foreach (State s in tmp)
                {
                    isOrphanState = true;
                    
                    bool hasSource = false, hasDest = false;

                    foreach (Transition t in s.transitions)
                    {
                        if (s.type == stateType.initial || s.type == stateType.final || s.type == stateType.both)
                        {
                            isOrphanState = false;
                            break;
                        }

                        if (t.destState.index == s.index)
                            hasDest = true;

                        if (t.sourceState.index == s.index)
                            hasSource = true;

                        if (hasSource && hasDest)
                        {
                            isOrphanState = false;
                            break;
                        }
                    }

                    if (isOrphanState)
                    {
                        newDiagram.selectedState = s;
                        newDiagram.deleteState();
                        isDeletedSomeState = true;
                    }
                }
            }

            return newDiagram;
        }

        private bool containsFinalState(List<State> states)
        {
            foreach (State s in states)
            {
                if (s.type == stateType.final || s.type==stateType.both)
                    return true;
            }
            return false;

        }

        private bool containsInitialState(List<State> states)
        {
            foreach (State s in states)
            {
                if (s.type == stateType.initial || s.type==stateType.both)
                    return true;
            }
            return false;

        }

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


        bool isPiEqualsNewPi(List<List<State>> pi, List<List<State>> newPi)
        {
            int flag = 0;
            int g = 0;

            if (pi == newPi) //they contain same references
                return true; 

            if (pi.Count == newPi.Count)
            {
                foreach (List<State> ls in pi)
                {
                    flag = 0;
                    foreach (List<State> ls2 in newPi)
                    {
                        if (ls.Count == ls2.Count)
                        {
                            foreach (State s in ls2)
                            {
                                if (ls.Contains(s))
                                    flag++;
                                else
                                    break;
                            }

                            if (flag == ls2.Count)
                            {
                                g++;
                                break;
                            }
                        }
                    }
                }
                if (g == pi.Count)
                    return true;
            }
            return false;
        
        }

        int getSetIndex(List<List<State>> set ,State s)
        {
            foreach (List<State> ls in set)
            {
                if (ls.Contains(s))
                    return set.IndexOf(ls);
            }
            return -1;
        
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
                d.DefaultExt = "dfa";
                d.Filter = "DFA files (*.dfa)|*.dfa";
                d.OverwritePrompt = true;

                if (d.ShowDialog() != false) // cancel button is not pressed
                    name = d.FileName;
                else
                    return "";
            }

            xd = new XDocument();
            root = new XElement("Machine");
            root.Add(new XAttribute("type", "Dfa"));
        
            base.saveToFile(xd, root);
            xd.Save(name);
            return name;

        }

        public override void loadFromFile(XDocument doc)
        {
            if (doc.Root.Attribute("type").Value == "Dfa")
                base.loadFromFile(doc);
        }
    }
}



using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace Automata_Simulator
{
    class MooreTransitionDiagram:TransitionDiagram
    {
        State[,] table = new State[MAX, MAX];// [source state, input alphabet] -> dest states

        public override State addState(Point pos, int savedIndex = -1)
        {
            State s = base.addState(pos);

            Label lbl = (Label)s.figure.Tag;

            StackPanel sp = new StackPanel();
            //sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(new Label() { Content = lbl.Content.ToString(), Foreground = Brushes.White });
            ComboBox cb = new ComboBox();
            cb.FontSize = 12;

            cb.SelectionChanged += isEditedHandler;

            foreach (string o in outputs)
                cb.Items.Add(o);
            
            sp.Children.Add(cb);

            lbl.Content = sp;

            return s;
        }

        void isEditedHandler(object sender, SelectionChangedEventArgs e)
        {
            fileStatus.isEdited = true;
        }
        
        public override Transition addTransition(State s1, State s2, string point1 = null)
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
                Transition transition = new MooreTransition();
                generateTransition(s1, s2, transition, point1);
                return transition;
            }
            else
            {
                MessageBox.Show("In Moore Machine you can add only one transition corresponding to one input symbol");
                return null;
            }
        }

        public override string saveToFile(XDocument xd = null, XElement root = null, string name = "")
        {
            //Arg description:
            //Name :  to generate temp files automatically without requiring users to enter filename or to save the opened file

            
            if (name == "")
            {
                Microsoft.Win32.SaveFileDialog d = new Microsoft.Win32.SaveFileDialog();
                d.AddExtension = true;
                d.CheckPathExists = true;
                d.DefaultExt = "*.mo";
                d.Filter = "Moore Machine (*.mo)|*.mo";
                d.OverwritePrompt = true;

                if (d.ShowDialog() != false) // cancel button is not pressed
                    name = d.FileName;
                else
                    return "";
            }

            xd = new XDocument();
            root = new XElement("Machine");
            root.Add(new XAttribute("type", "Moore"));

            base.saveToFile(xd, root);
            xd.Save(name);
            return name;

        }

        public override void loadFromFile(XDocument doc)
        {
            if (doc.Root.Attribute("type").Value == "Moore")
                base.loadFromFile(doc);
        }

        public override Transition getTransition(object sender)
        {
            if (sender is Path)
            {
                Path p = (Path)sender;
                return (Transition)((StackPanel)p.Tag).Tag;
            }
            else if (sender is ComboBox)
                return (Transition)((ComboBox)sender).Tag;
            else
                return null;
        }

        public override void onInputChange(object sender, SelectionChangedEventArgs e)
        {
            fileStatus.isEdited = true;

            ComboBox cb1 =(ComboBox)sender;

            // to supress calling of this event the second time after setting cb.selectedIndex=-1 when error occurs    
            if (cb1.SelectedIndex == -1)
                return;

            Transition transition = (Transition)cb1.Tag;

            bool isError = false;

            foreach (Transition t in transition.sourceState.transitions)
            {
                if (t.sourceState == transition.sourceState)
                    if (t.input == cb1.SelectedValue.ToString())
                    {
                        isError = true;
                        break;
                    }
            }

            if (isError == true)
            {
                MessageBox.Show("There can't be more than one state having same input alphabet in a Moore machine");
                cb1.SelectedIndex = -1;
                transition.input = "";
            }
            else
            {
                transition.input = cb1.SelectedValue.ToString();

                int i = getIndexOfInput(cb1.SelectedValue.ToString());

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

        public override bool isErroneous()
        {
            if (initialStates == null || initialStates.Count == 0)
            {
                MessageBox.Show("Error: Set the Initial State");
                return true;
            }

            foreach (State s in states)
            {
                StackPanel sp = (StackPanel)((Label)s.figure.Tag).Content;
                if (((ComboBox)sp.Children[1]).SelectedIndex == -1)
                {
                    MessageBox.Show("Error: Set outputs on all the States");
                    return true;
                }

            }

            return base.isErroneous();

        }

        public override bool checkStringAcceptance(string str)
        {
            //not applicable
            return false;
        }

        public override string produceOutput(string input)
        {
            transitionsOnTraversedPath = new List<string>();

            int i = getIndexOfInput(input[0].ToString());

            int indexInsideString = 0;
            
            State currentState;
            
            if (initialStates.Count > 1)
            {
                if (this.selectedState == null)
                {
                    MessageBox.Show("As there are more than one initial states, Select the one you want to use for execution");
                    return "";
                }
                else
                {
                    if (this.selectedState.type == stateType.initial || this.selectedState.type == stateType.both)
                        currentState = this.selectedState;
                    else
                    {
                        MessageBox.Show("As there are more than one initial states, Select the one you want to use for execution");
                        return "";
                    }
                }
            }
            else
                currentState = initialStates[0];


            String outputString = ((ComboBox)((StackPanel)((Label)currentState.figure.Tag).Content).Children[1]).SelectedValue.ToString();

            State nextState = null;
            
            Transition transition = null;

            while (indexInsideString < input.Length)
            {
                i = getIndexOfInput(input[indexInsideString].ToString());

                if (i == -1)
                {
                    MessageBox.Show("Invalid input symbol \'" + input[indexInsideString].ToString()+ "\'");
                    return outputString;
                }
                
                nextState = table[currentState.index, i];
                
                if (nextState == null)
                {
                    MessageBox.Show("A move is not defined on some symbols");
                    return outputString;
                }

                transitionsOnTraversedPath.Add(currentState.index.ToString() + ":" + nextState.index.ToString() + ":" + input[indexInsideString].ToString());


                foreach (Transition t1 in currentState.transitions)
                {
                    if (t1.sourceState.index == currentState.index && t1.destState.index == nextState.index && t1.input == input[indexInsideString].ToString())
                    {
                        transition = t1;
                        break;
                    }
                }

                outputString += ((ComboBox)((StackPanel)((Label)nextState.figure.Tag).Content).Children[1]).SelectedValue.ToString();
                indexInsideString++;
                currentState = nextState;

            }

            return outputString;
            
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
            if (i >= 0)// if nothing is selected i is -1
                table[selectedTransition.sourceState.index, i] = null;

            base.deleteTransition();
        }
  
        
        public bool isError()
        {
            foreach (Transition t in transitions)
            {
                int index2 = ((ComboBox)((StackPanel)t.figure.Tag).Children[0]).SelectedIndex;
                if (index2 < 0)
                    return false;
            }
            foreach (State s in states)
            {
              int index1=((ComboBox)(((StackPanel)(((Label)((Ellipse)s.figure).Tag).Content)).Children[1])).SelectedIndex;
              if (index1 < 0)
                  return false;
            }

            return true;
        }
        public MealyTransitionDiagram convertToMealy()
        {
            if (isError() && transitions.Count==noOfInputs*states.Count)
            {
                MealyTransitionDiagram newDiagram = new MealyTransitionDiagram();
                newDiagram.inputs = this.inputs;
                newDiagram.outputs = this.outputs;
                newDiagram.noOfInputs = this.noOfInputs;
                newDiagram.setCanvas(canvasMain);
                List<int> initialStatesIndexes = new List<int>();
                foreach (State s in this.states)
                {
                    if (initialStates.Contains(s))
                        initialStatesIndexes.Add(s.index);
                }

                foreach (State s in this.states)
                {
                    foreach (string i in this.inputs)
                    {
                        if (table[s.index, getIndexOfInput(i)] == null)
                        {
                            MessageBox.Show("For each State you must define transitions for all input symbols");
                            return null;
                        }
                    }
                }

                canvasMain.Children.Clear();

                List<State> newStates = new List<State>();
                for (int i = 0; i < states.Count; i++)
                {
                    State s = newDiagram.addState(State.getNewStatePosition());
                    if (initialStatesIndexes.Contains(i))
                    {
                        newDiagram.initialStates.Add(s);
                        s.addStartingArrow(canvasMain);
                        s.type = stateType.initial;

                    }

                    newStates.Add(s);
                }

                foreach (State s in newStates)
                {
                    State source = s;
                    foreach (string input in newDiagram.inputs)
                    {
                        State dest = table[s.index, getIndexOfInput(input)];
                        State newDes = dest;
                        foreach (State ss in newStates)
                        {
                            if (ss.index == dest.index)
                            {
                                newDes = ss;
                                break;
                            }
                        }
                        if (newDes != null)
                        {
                            if (source != null && newDes != null)
                            {
                                string output = "";
                                foreach (State r in states)
                                {
                                    if (newDes.index == r.index)
                                    {
                                        output = ((ComboBox)(((StackPanel)(((Label)r.figure.Tag).Content)).Children[1])).SelectedItem.ToString();
                                        break;
                                    }
                                }

                                Transition tr = newDiagram.addTransition(source, newDes);
                                if (tr != null)
                                {
                                    int index1 = ((ComboBox)((StackPanel)tr.figure.Tag).Children[0]).Items.IndexOf(input);
                                    ((ComboBox)((StackPanel)tr.figure.Tag).Children[0]).SelectedIndex = index1;

                                    int index2 = ((ComboBox)((StackPanel)tr.figure.Tag).Children[2]).Items.IndexOf(output);
                                    ((ComboBox)((StackPanel)tr.figure.Tag).Children[2]).SelectedIndex = index2;
                                }
                            }
                        }
                    }
                }

                return newDiagram;
            }
            else
            {
                if (transitions.Count != noOfInputs * states.Count)
                {
                    MessageBox.Show("Error:There should be transition for each input symbol.");
                }
                else
                {
                    MessageBox.Show("Error:Set input on each transition/Set output on each state");
                }
                
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;
using System.Xml.Linq;

namespace Automata_Simulator
{
    class MealyTransitionDiagram:TransitionDiagram
    {
        State[,] table = new State[MAX, MAX];// [source state, input alphabet] -> dest states
        String[,] table2 = new String[MAX, MAX];// [source state, input alphabet] -> output
        State[,] table3 = new State[MAX, MAX];//[current state, destnion1,destination2,output]

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
                Transition transition = new MealyTransition();
                generateTransition(s1, s2, transition, point1);
                return transition;
            }
            else
            {
                MessageBox.Show("In Mealy Machine you can add only one transition corresponding to one input symbol");
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

        public override void onInputChange(object sender, SelectionChangedEventArgs e)
        {
            fileStatus.isEdited = true;

            ComboBox cb1 = (ComboBox)sender;
            ComboBox cb2 = (ComboBox)((StackPanel)cb1.Parent).Children[2];
            
            cb2.IsEnabled = true;



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
                MessageBox.Show("There can't be more than same input on single stste in Moore machine");
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

        public override string saveToFile(XDocument xd = null, XElement root = null, string name = "")
        {
            //Arg description:
            //Name :  to generate temp files automatically without requiring users to enter filename or to save the opened file

            if (name == "")
            {
                Microsoft.Win32.SaveFileDialog d = new Microsoft.Win32.SaveFileDialog();
                d.AddExtension = true;
                d.CheckPathExists = true;
                d.DefaultExt = "*.ml";
                d.Filter = "Mealy Machine (*.ml)|*.ml";
                d.OverwritePrompt = true;

                if (d.ShowDialog() != false) // cancel button is not pressed
                    name = d.FileName;
                else
                    return "";
            }

            xd = new XDocument();
            root = new XElement("Machine");
            root.Add(new XAttribute("type", "Mealy"));

            
                
            base.saveToFile(xd, root);
            xd.Save(name);
            return name;

        }

        public override void loadFromFile(XDocument doc)
        {
            if (doc.Root.Attribute("type").Value == "Mealy")
                base.loadFromFile(doc);
        }

        public override void onOutputChange(object sender, SelectionChangedEventArgs e)
        {
            fileStatus.isEdited = true;

            ComboBox cb2 = (ComboBox)sender;
            ComboBox cb1 = (ComboBox)((StackPanel)cb2.Parent).Children[0];

            Transition transition = (Transition)cb1.Tag;

            transition.input = cb1.SelectedValue.ToString();

            int i = getIndexOfInput(cb1.SelectedValue.ToString());

            if (e.RemovedItems.Count == 0)// first time selection
                ;
            else // selection changed
            {
                int j = getIndexOfInput(e.RemovedItems[0].ToString());
                table2[transition.sourceState.index, j] = null;
            }

            table2[transition.sourceState.index, i] = cb2.SelectedItem.ToString();

        }

        public override bool isErroneous()
        {
            if (initialStates == null || initialStates.Count == 0)
            {
                MessageBox.Show("Error: Set the Initial State");
                return true;
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

            String outputString = "";

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

            State nextState = null;

            Transition transition = null;

            while (indexInsideString < input.Length)
            {
                i = getIndexOfInput(input[indexInsideString].ToString());

                if (i == -1)
                {
                    MessageBox.Show("Invalid input symbol \'" + input[indexInsideString].ToString() + "\'");
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

                outputString += ((ComboBox)((StackPanel)transition.figure.Tag).Children[2]).SelectedItem; //first output
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
        public string getNameOfState(int i)
        {
            foreach (State s in states)
            {
                if (s.index == i)
                    return ((Label)((Ellipse)s.figure).Tag).Content.ToString();
            }
            return null;

        }
        List<State> newStates = new List<State>();
        int ptr = 0;
        public bool IsError()
        {
            foreach (Transition t in transitions)
            {
                int index1 = ((ComboBox)((StackPanel)t.figure.Tag).Children[0]).SelectedIndex;
                

                int index2 = ((ComboBox)((StackPanel)t.figure.Tag).Children[2]).SelectedIndex;
                
                if (index1 < 0 || index2 < 0)
                    return false;
            }
            return true;
        }
        public MooreTransitionDiagram convertToMoore()
        {
            if (IsError() && transitions.Count==noOfInputs*states.Count)
            {
                int flag = 0;

                if (initialStates.Count != 0)
                {
                    MooreTransitionDiagram newDiagram = new MooreTransitionDiagram();
                    newDiagram.inputs = this.inputs;
                    newDiagram.noOfInputs = this.noOfInputs;
                    newDiagram.outputs = this.outputs;

                    newDiagram.setCanvas(canvasMain);
                    canvasMain.Children.Clear();


                    List<int> initialStatesIndexes = new List<int>();
                    foreach (State s in this.states)
                    {
                        if (initialStates.Contains(s))
                            initialStatesIndexes.Add(s.index);
                    }


                    int newStatesCount = 0;
                    for (int i = 0; i < states.Count; i++)
                    {
                        if (table2[i, 0] == table2[i, 1])
                        {
                            State s = newDiagram.addState(State.getNewStatePosition());

                            ((Label)(((StackPanel)(((Label)s.figure.Tag).Content)).Children[0])).Content = getNameOfState(i) + outputs[0];
                            ((Label)(((StackPanel)(((Label)s.figure.Tag).Content)).Children[0])).ToolTip = getNameOfState(i) + outputs[0] + "," + getNameOfState(i) + outputs[1];
                            s.figure.ToolTip = getNameOfState(i) + outputs[0] + "," + getNameOfState(i) + outputs[1];


                            if (flag == 0)
                            {
                                ((ComboBox)(((StackPanel)(((Label)((Ellipse)s.figure).Tag).Content)).Children[1])).Items.Add("^");
                                int idx = ((ComboBox)(((StackPanel)(((Label)((Ellipse)s.figure).Tag).Content)).Children[1])).Items.IndexOf("^");
                                if (idx >= 0)
                                {
                                    ((ComboBox)(((StackPanel)(((Label)((Ellipse)s.figure).Tag).Content)).Children[1])).SelectedIndex = idx;
                                    flag++;
                                }
                            }
                            else
                            {
                                int index = ((ComboBox)(((StackPanel)(((Label)((Ellipse)s.figure).Tag).Content)).Children[1])).Items.IndexOf(table2[i, 0]);
                                if (index >= 0)
                                    ((ComboBox)(((StackPanel)(((Label)((Ellipse)s.figure).Tag).Content)).Children[1])).SelectedIndex = index;

                            }

                            if (initialStatesIndexes.Contains(i))
                            {
                                newDiagram.initialStates.Add(s);
                                s.addStartingArrow(canvasMain);
                                s.type = stateType.initial;

                            }

                            table3[ptr, 0] = s;
                            ptr++;
                            newStates.Add(s);
                            newStatesCount++;

                        }
                        else
                        {
                            State s1 = newDiagram.addState(State.getNewStatePosition());
                            ((Label)(((StackPanel)(((Label)s1.figure.Tag).Content)).Children[0])).Content = getNameOfState(i) + table2[i, 0];
                            ((Label)(((StackPanel)(((Label)s1.figure.Tag).Content)).Children[0])).ToolTip = getNameOfState(i) + table2[i, 0];
                            s1.figure.ToolTip = getNameOfState(i) + table2[i, 0];


                            if (flag == 0)
                            {

                                ((ComboBox)(((StackPanel)(((Label)((Ellipse)s1.figure).Tag).Content)).Children[1])).Items.Add("^");
                                int idx = ((ComboBox)(((StackPanel)(((Label)((Ellipse)s1.figure).Tag).Content)).Children[1])).Items.IndexOf("^");
                                if (idx >= 0)
                                {
                                    ((ComboBox)(((StackPanel)(((Label)((Ellipse)s1.figure).Tag).Content)).Children[1])).SelectedIndex = idx;

                                }
                            }
                            else
                            {
                                int index1 = ((ComboBox)(((StackPanel)(((Label)((Ellipse)s1.figure).Tag).Content)).Children[1])).Items.IndexOf(table2[i, 0]);
                                if (index1 >= 0)
                                    ((ComboBox)(((StackPanel)(((Label)((Ellipse)s1.figure).Tag).Content)).Children[1])).SelectedIndex = index1;

                            }

                            if (initialStatesIndexes.Contains(i))
                            {
                                newDiagram.initialStates.Add(s1);
                                s1.addStartingArrow(canvasMain);
                                s1.type = stateType.initial;
                            }

                            table3[ptr, 0] = s1;
                            newStates.Add(s1);
                            ptr++;
                            State s2 = newDiagram.addState(State.getNewStatePosition());
                            ((Label)(((StackPanel)(((Label)s2.figure.Tag).Content)).Children[0])).Content = getNameOfState(i) + table2[i, 1];
                            ((Label)(((StackPanel)(((Label)s2.figure.Tag).Content)).Children[0])).ToolTip = getNameOfState(i) + table2[i, 1];
                            s2.figure.ToolTip = getNameOfState(i) + table2[i, 1];

                            if (flag == 0)
                            {
                                ((ComboBox)(((StackPanel)(((Label)((Ellipse)s2.figure).Tag).Content)).Children[1])).Items.Add("^");
                                int idx = ((ComboBox)(((StackPanel)(((Label)((Ellipse)s1.figure).Tag).Content)).Children[1])).Items.IndexOf("^");
                                if (idx >= 0)
                                {
                                    ((ComboBox)(((StackPanel)(((Label)((Ellipse)s2.figure).Tag).Content)).Children[1])).SelectedIndex = idx;
                                    flag++;
                                }

                            }
                            else
                            {
                                int index2 = ((ComboBox)(((StackPanel)(((Label)((Ellipse)s2.figure).Tag).Content)).Children[1])).Items.IndexOf(table2[i, 1]);
                                if (index2 >= 0)
                                    ((ComboBox)(((StackPanel)(((Label)((Ellipse)s2.figure).Tag).Content)).Children[1])).SelectedIndex = index2;
                            }

                            if (initialStatesIndexes.Contains(i))
                            {

                                newDiagram.initialStates.Add(s2);
                                s2.addStartingArrow(canvasMain);
                                s2.type = stateType.initial;

                            }

                            newStates.Add(s2);
                            table3[ptr, 0] = s2;
                            ptr++;
                            newStatesCount += 2;
                        }
                    }

                    ptr = 0;
                    for (int i = 0; i < states.Count; i++)
                    {
                        if (table2[i, 0] == table2[i, 1])
                        {
                            State s = table[i, 0];
                            string name = getNameOfState(s.index) + table2[i, 0];
                            State ss = getStateByName(name);

                            table3[ptr, 1] = ss;

                            State s1 = table[i, 1];
                            string name1 = getNameOfState(s1.index) + table2[i, 1];
                            State ss1 = getStateByName(name1);


                            table3[ptr, 2] = ss1;
                            ptr++;

                        }
                        else
                        {
                            State s1 = table[i, 0];
                            string name1 = getNameOfState(s1.index) + table2[i, 0];
                            State ss1 = getStateByName(name1);


                            table3[ptr, 1] = ss1;
                            table3[ptr + 1, 1] = ss1;

                            State s2 = table[i, 1];
                            string name2 = getNameOfState(s2.index) + table2[i, 1];
                            State ss2 = getStateByName(name2);

                            table3[ptr, 2] = ss2;
                            table3[ptr + 1, 2] = ss2;
                            ptr += 2;
                        }
                    }


                    for (int i = 0; i < newStates.Count; i++)
                    {

                        for (int j = 1; j <= 2; j++)
                        {
                            Transition tr = newDiagram.addTransition(table3[i, 0], table3[i, j]);
                            if (tr != null)
                            {

                                int index2 = ((ComboBox)((StackPanel)tr.figure.Tag).Children[0]).Items.IndexOf(inputs[j - 1]);
                                if (index2 >= 0)
                                    ((ComboBox)((StackPanel)tr.figure.Tag).Children[0]).SelectedIndex = index2;
                            }
                        }
                    }
                    return newDiagram;

                }
                else
                {
                    MessageBox.Show("set staring state");
                    return null;
                }
            }
            else
            {
                if (transitions.Count == noOfInputs * states.Count)
                    MessageBox.Show("Error:There should be transition for each input symbol.");
                else
                    MessageBox.Show("Error:Set inputs/Outputs on all transitions");
               
                return null;
            }

        }
        public State getStateByName(string name)
        {
            foreach (State s in newStates)
            {
                string sname = ((Label)(((StackPanel)(((Label)s.figure.Tag).Content)).Children[0])).ToolTip.ToString();
               
                if (sname.IndexOf(',') >= 0)
                {
                    string[] twoNames = sname.Split(',');
                    if (name ==twoNames[0]||name==twoNames[1])
                        return s;
                }
                else
                {
                    if (sname == name)
                        return s;
                }
            }
            return null;
        }
    }
}
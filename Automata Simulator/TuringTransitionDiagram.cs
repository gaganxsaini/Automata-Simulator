using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;
using System.Xml.Linq;

namespace Automata_Simulator
{
    class TuringTransitionDiagram : TransitionDiagram
    {
        //in turing both input symbols and output symbols are same, so use this.inputs everywhere 
        State[,] table = new State[MAX, MAX];// [source state, input alphabet] -> dest State

        public override Transition addTransition(State s1, State s2, string point1)
        {
            int count = 0;  
            foreach (Transition t in s1.transitions)
            {
                if (t.sourceState == s1)
                    count++;
            }

            if (count < noOfInputs)
            {
                Transition transition = new turingTransition();
                generateTransition(s1, s2, transition,point1);
                return transition;
            }
            else
            {
                MessageBox.Show("You can add only one transition corresponding to one input symbol");
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

            ComboBox cb = (ComboBox)sender;

            Transition t = (Transition)cb.Tag;

            if (cb.SelectedItem != null) // because it was set null in the code on error
            {
                int i = getIndexOfInput(cb.SelectedValue.ToString());

                if (e.RemovedItems.Count == 0)// first time selection
                    ;
                else // selection changed
                {
                    int j = getIndexOfInput(e.RemovedItems[0].ToString());
                    e.RemovedItems.Clear();
                    table[t.sourceState.index, j] = null;
                    t.input = null;
                }

                if (table[t.sourceState.index, i] != null)
                {
                    MessageBox.Show("A transition is already specified on this input symbol");
                    cb.SelectedItem = null;
                }
                else
                {
                    table[t.sourceState.index, i] = t.destState;
                    t.input = cb.SelectedValue.ToString();
                }
            }


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

        public override bool checkStringAcceptance(string s)
        {
            //not applicable
            return false;
        }

        public override string produceOutput(string input)
        {
            transitionsOnTraversedPath = new System.Collections.Generic.List<string>();
            
            int i = 0; // at 0 is null

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

            while (i < input.Length)
            {
                int index = getIndexOfInput(input[i].ToString());
                
                if (index == -1)
                {
                    MessageBox.Show("Unrecognized symbol found on the Tape");
                    break;
                }

                nextState = table[currentState.index, index];
                
                if (nextState == null) // i.e. no transition from current state at given input
                    break;

                transitionsOnTraversedPath.Add(currentState.index.ToString() + ":" + nextState.index.ToString() + ":" + input[i].ToString());

                Transition transition = null;

                foreach (Transition t1 in currentState.transitions) //find the transition b/w current state and next state
                {
                    if (t1.sourceState.index == currentState.index && t1.destState.index == nextState.index && t1.input == input[i].ToString())
                    { 
                        transition = t1;
                        break;
                    }
                }

                if (transition != null)
                {
                    string output = ((ComboBox)((StackPanel)transition.figure.Tag).Children[2]).SelectedValue.ToString();

                    input = input.Substring(0, i) + output + input.Substring(i + 1, input.Length - (i + 1));

                    string direction = ((ComboBox)((StackPanel)transition.figure.Tag).Children[4]).SelectedValue.ToString();
                    if (direction == "L")
                        i--;
                    else if (direction == "R")
                        i++;
                    else
                        break;//halt
                }
                currentState = nextState;
            }

            return input;

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
                d.DefaultExt = "*.tm";
                d.Filter = "Turing Machine (*.tm)|*.tm";
                d.OverwritePrompt = true;

                if (d.ShowDialog() != false) // cancel button is not pressed
                    name = d.FileName;
                else
                    return "";
            }

            xd = new XDocument();
            root = new XElement("Machine");
            root.Add(new XAttribute("type", "Turing"));
                
            base.saveToFile(xd, root);
            xd.Save(name);
            return name;
        }

        public override void loadFromFile(XDocument doc)
        {
            if (doc.Root.Attribute("type").Value == "Turing")
                base.loadFromFile(doc);
        }
    }
}

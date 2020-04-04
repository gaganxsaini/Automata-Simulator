using System;
using System.Linq;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Collections.Generic;
using System.Xml.Linq;


namespace Automata_Simulator
{
    public abstract class TransitionDiagram
    {
        public TransitionDiagram()
        {
            State.resetLabels();
            State.resetPoints();
        }

        public List<string> transitionsOnTraversedPath = null;

        public State selectedState;
        public Transition selectedTransition;

        public List<State> initialStates = new List<State>();
        public List<State> finalStates = new List<State>();

        public List<State> states = new List<State>();
        public List<Transition> transitions = new List<Transition>();

        int newStateIndex = 0; // using states.count was causing problem when a state is deleted and after that a new state is created

        public string[] inputs = new string[MAX];
        public List<string> outputs = null;
        public int noOfInputs = 0;


        public bool isDragging = false; // whether dragging a state or transition

        public Canvas canvasMain;

        public void setCanvas(Canvas c)
        {
            canvasMain = c;
        }

        public const int MAX = 1000; // maximum number of states allowed 

        // To find the state whose index is known
        State[] indexToStateMapping = new State[MAX];
        
        public virtual State addState(Point pos, int savedIndex = -1)
        {
            //args info
            //savedIndex : load the index saved into file if machine is loaded from file, instead of generating new file

            //Tag Info:
            //Ellipse -> label -> index 

            if (states.Count == MAX || newStateIndex == MAX)// 2nd condition is because index in all arrays is <=100
            {
                MessageBox.Show("You can not have more than " + TransitionDiagram.MAX.ToString() + " states");
                return null;
            }
            else
            {
                int index;
                if (savedIndex != -1)
                    index = savedIndex;
                else
                    index = newStateIndex++;

                Ellipse el = new Ellipse();
                el.Width = el.Height = State.SIZE; 
                el.Fill = Brushes.Gray;
                el.SetValue(Canvas.LeftProperty, pos.X - el.Width / 2);
                el.SetValue(Canvas.TopProperty, pos.Y - el.Height / 2);

                Label lbl = new Label();
                lbl.FontSize = 10;
                lbl.Foreground = Brushes.White;

                string stateName = State.generateStateName();

                
                //to check uniqueness of state name
                //for the case when machine is loaded from a file
                while (true)
                {
                    bool isMatched = false;

                    foreach (State st in this.states)
                    {
                        if (((Label)st.figure.Tag).Content.ToString() == stateName)
                        {
                            stateName = State.generateStateName();
                            isMatched = true;
                            break;
                        }
                    }

                    if (!isMatched)
                        break;
                }
                
                el.ToolTip = lbl.Content = stateName;
                lbl.Tag = index;  // used to find the state when user clicks on the label instead of ellipse to select a state 

                el.Tag = lbl;

                canvasMain.Children.Add(el);
                canvasMain.Children.Add(lbl);

                lbl.SetValue(Canvas.LeftProperty, el.GetValue(Canvas.LeftProperty));
                lbl.SetValue(Canvas.TopProperty, el.GetValue(Canvas.TopProperty));

                el.SetValue(Canvas.ZIndexProperty, 10);
                lbl.SetValue(Canvas.ZIndexProperty, 11);

                State s = new State(index, el);
                indexToStateMapping[s.index] = s;
                states.Add(s);

                return s;
            }

        }

        public int getIndexOfInput(String s)
        {
            for (int i = 0; i < noOfInputs; i++)
                if (inputs[i] == s)
                    return i;

            return -1;

        }

        public int getIndexOfOutput(String s)
        {
            for (int i = 0; i <outputs.Count; i++)
                if (outputs[i] == s)
                    return i;

            return -1;

        }

        public virtual State getState(object sender)
        {
            if (sender is Label)
            {
                Label lbl = (Label)sender;
                return indexToStateMapping[Int16.Parse(lbl.Tag.ToString())];
            }
            else if (sender is Ellipse)
            {
                Ellipse el = (Ellipse)sender;
                return indexToStateMapping[Int16.Parse(((Label)el.Tag).Tag.ToString())];
            }
            else
                return null;
        }

        public void generateTransition(State s1, State s2, Transition transition, String point1)
		//pt1 to be used to hold the point1 of the curve in the form x,y  when a machine is read from a file
        {
            PathGeometry myPathGeometry = new PathGeometry();
            QuadraticBezierSegment curve = new QuadraticBezierSegment();
            PathFigure myPathFigure = new PathFigure();

            Point startingPt = new Point((double)s1.figure.GetValue(Canvas.LeftProperty) + State.SIZE / 2,
                (double)s1.figure.GetValue(Canvas.TopProperty) + State.SIZE / 2);

            Point endingPt = new Point((double)s2.figure.GetValue(Canvas.LeftProperty) + State.SIZE / 2,
                (double)s2.figure.GetValue(Canvas.TopProperty) + State.SIZE / 2);

            myPathFigure.StartPoint = startingPt;
            myPathFigure.Segments.Add(curve);
            myPathGeometry.Figures.Add(myPathFigure);

            // Create a path to draw a geometry with.

            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.StrokeThickness = 1;

            // specify the shape of the path using the path geometry.
            myPath.Data = myPathGeometry;


            curve.Point2 = endingPt;

            // find the height and width of the arc

            double distance = Math.Sqrt(Math.Pow(endingPt.X - startingPt.X, 2) + Math.Pow(endingPt.Y - startingPt.Y, 2));
            distance /= 2;
            distance += startingPt.X;

            Point pt1 = new Point();
            if (point1 != null)
            {
                string[] pts = point1.Split(',');
                pt1.X = double.Parse(pts[0]);
                pt1.Y = double.Parse(pts[1]);
            }
            else
            {
                pt1.X = distance;
                pt1.Y = 500;
            }

            curve.Point1 = pt1;

            Point mid = new Point((startingPt.X + endingPt.X) / 2, (startingPt.Y + endingPt.Y) / 2);

            Point mid2 = new Point((mid.X + pt1.X) / 2, (mid.Y + pt1.Y) / 2);


            StackPanel sp = transition.generateLabel(mid2, this);


            transition.figure = myPath;
            transition.sourceState = s1;
            transition.destState = s2;

            transition.sPoint = startingPt;
            transition.ePoint = endingPt;


            sp.Tag = transition;
            myPath.Tag = sp;

            canvasMain.Children.Add(myPath);
            canvasMain.Children.Add(sp);
            myPath.SetValue(Canvas.ZIndexProperty, -80); // to make sp appear over the curve

            Transition.addArrow(transition, canvasMain);

            if (s1 == s2) // add only once for the self loops
                s1.transitions.Add(transition);
            else
            {
                s1.transitions.Add(transition);
                s2.transitions.Add(transition);
            }
            transitions.Add(transition);

        }

        public abstract Transition addTransition(State s1, State s2, String point1 = null); 
        //point1 to be used when creating m/c dynamically (reading from file, or when computing result) 
        //point1 is taken as string because Point Object can not be assigned null as the default value    

        public abstract Transition getTransition(object sender);

        public abstract void onInputChange(object sender, SelectionChangedEventArgs e);

        public virtual void onOutputChange(object sender, SelectionChangedEventArgs e)
        { }


        public abstract bool checkStringAcceptance(string s);

        public abstract string produceOutput(string input);

        public virtual bool isErroneous() // override it
        {
            foreach (UIElement e in canvasMain.Children)
            {
                if (e is StackPanel)
                {
                    StackPanel sp = (StackPanel)e;
                    foreach (UIElement c in sp.Children)
                    {
                        if (c is ComboBox)
                        {
                            ComboBox cb = (ComboBox)c;
                            if (cb.SelectedIndex == -1)
                            {
                                ((Transition)cb.Tag).figure.Stroke = Brushes.Blue;
                                MessageBox.Show("Error: Set the symbols on all the Trasitions");
                                return true;
                            }
                        }

                    }

                }
            }

            return false;

        }

        public virtual void deleteState() //override it in the derived classes to delete table entries 
        {
            foreach (Transition t in selectedState.transitions)
            {
                if (t.sourceState != t.destState) //removing self loop edge here causes enumeration to be modified
                {
                    if (t.sourceState == selectedState)
                        t.destState.transitions.Remove(t);
                    else // t.destState == selectedState
                        t.sourceState.transitions.Remove(t);
                }

                transitions.Remove(t);

                canvasMain.Children.Remove((StackPanel)t.figure.Tag);
                canvasMain.Children.Remove(t.figure);
                canvasMain.Children.Remove(t.arrow);
            }

            canvasMain.Children.Remove(selectedState.figure);
            
            canvasMain.Children.Remove((Label)selectedState.figure.Tag);

            canvasMain.Children.Remove(selectedState.startingArrow);

            finalStates.Remove(selectedState);
            initialStates.Remove(selectedState);

            states.Remove(selectedState);

            selectedState = null;
        }

        public virtual void deleteTransition() //override it in the derived classes to delete table entries
        {
            selectedTransition.sourceState.transitions.Remove(selectedTransition);
            selectedTransition.destState.transitions.Remove(selectedTransition);
            transitions.Remove(selectedTransition);

            canvasMain.Children.Remove((StackPanel)selectedTransition.figure.Tag);
            canvasMain.Children.Remove(selectedTransition.arrow);
            canvasMain.Children.Remove(selectedTransition.figure);

            selectedTransition = null;

        }

        public bool mutexSelect(object toBeSelected) //returns true if anything gets selected
        {

            // if anything is already selected, deselect it 
            if (selectedTransition != null)
            {
                selectedTransition.figure.Stroke = Brushes.Black;

                //if clicked again on the selected one
                if (toBeSelected is Transition && selectedTransition == (Transition)toBeSelected)
                {
                    selectedTransition = null;
                    return false;
                }
                selectedTransition = null;

            }
            if (selectedState != null)
            {
                selectedState.figure.Fill = Brushes.Gray;

                if (toBeSelected is State && selectedState == (State)toBeSelected)
                {
                    selectedState = null;
                    return false;
                }
                selectedState = null;
            }


            if (toBeSelected is State)
            {
                selectedState = (State)toBeSelected;
                selectedState.figure.Fill = Brushes.Blue;
                return true;
            }
            else if (toBeSelected is Transition)
            {
                selectedTransition = (Transition)toBeSelected;
                selectedTransition.figure.Stroke = Brushes.Blue;
                return true;
            }

            return false;

        }

        public virtual string saveToFile(XDocument xd = null, XElement root = null, string name="") //override it
        {
            // name is used(in the derived classes) to generate temp files automatically without requiring users to enter filename
            // returns filename

            string str = "";
            foreach (String s in inputs)
                str = str + s + ";";

            str = str.TrimEnd(';');

            root.Add(new XAttribute("inputs", str));
            root.Add(new XAttribute("width", canvasMain.ActualWidth));
            root.Add(new XAttribute("height", canvasMain.ActualHeight));

            if (this is MooreTransitionDiagram || this is MealyTransitionDiagram)
            {
                string op = "";

                if (outputs != null)// outputs not set yer
                {
                    foreach (String s in outputs)
                        op = op + s + ";";

                    op = op.TrimEnd(';');
                }

                root.Add(new XAttribute("outputs", op));
            }


            XElement xStates = new XElement("States");

            xStates.Add(new XAttribute("count", states.Count));
            
            foreach (State s in states)
            {
                XElement x = new XElement("State");
                x.Add(new XAttribute("index", s.index));
                x.Add(new XAttribute("type", s.type.ToString()));

                if (this is MooreTransitionDiagram)
                {
                    x.Add(new XAttribute("label", ((Label)(((StackPanel)(((Label)s.figure.Tag).Content)).Children[0])).Content));
                    ComboBox cb = ((ComboBox)(((StackPanel)(((Label)((Ellipse)s.figure).Tag).Content)).Children[1]));
                    if (cb.SelectedIndex != -1)
                        x.Add(new XAttribute("output", cb.SelectedValue.ToString()));  
                }
                else
                    x.Add(new XAttribute("label", ((Label)s.figure.Tag).Content));
              
                x.Add(new XAttribute("x", s.figure.GetValue(Canvas.LeftProperty)));
                x.Add(new XAttribute("y", s.figure.GetValue(Canvas.TopProperty)));

                xStates.Add(x);
            }

            root.Add(xStates);

            XElement xTrans = new XElement("Transitions");
            xTrans.Add(new XAttribute("count", transitions.Count));
            foreach (Transition t in transitions)
            {
                XElement y = new XElement("Transition");

                if (t.input != null)
                    y.Add(new XAttribute("input", t.input));

                if (this is TuringTransitionDiagram)
                {
                    ComboBox cb= ((ComboBox)((StackPanel)t.figure.Tag).Children[2]);

                    if(cb.SelectedIndex != -1)
                        y.Add(new XAttribute("output", cb.SelectedValue.ToString()));

                    cb = ((ComboBox)((StackPanel)t.figure.Tag).Children[4]);
                    if(cb.SelectedIndex != -1)
                        y.Add(new XAttribute("direction", cb.SelectedValue.ToString()));
                }

                if (this is MealyTransitionDiagram)
                {
                    ComboBox cb = ((ComboBox)((StackPanel)t.figure.Tag).Children[2]);
                    if (cb.SelectedIndex!=-1)
                        y.Add(new XAttribute("output",cb.SelectedValue.ToString()));
                }

                y.Add(new XAttribute("sourceStateIndex", t.sourceState.index));
                y.Add(new XAttribute("destStateIndex", t.destState.index));
                y.Add(new XAttribute("point1", ((QuadraticBezierSegment)(((PathGeometry)t.figure.Data).Figures[0].Segments[0])).Point1));

                xTrans.Add(y);
            }

            root.Add(xTrans);

            xd.Add(root);

            return "";

        }

        public virtual void loadFromFile(XDocument doc) //override it
        {
            this.noOfInputs = doc.Root.Attribute("inputs").Value.Split(';').Length;
            this.inputs = doc.Root.Attribute("inputs").Value.Split(';');

            canvasMain.Height = Convert.ToDouble(doc.Root.Attribute("height").Value);
            canvasMain.Width = Convert.ToDouble(doc.Root.Attribute("width").Value);

			if (this is MealyTransitionDiagram || this is MooreTransitionDiagram)
            {
                string[] outs = doc.Root.Attribute("outputs").Value.Split(';');
                this.outputs = new List<string>(outs);
            }

            var query = from state in doc.Root.Element("States").Elements("State")
                        select state;
            
            foreach (XElement s in query)
            {
                int index = int.Parse(s.Attribute("index").Value);

                State a = this.addState(new Point(double.Parse(s.Attribute("x").Value), double.Parse(s.Attribute("y").Value)) , index);

                if (this is MooreTransitionDiagram)
                {
                    StackPanel sp = (StackPanel)(((Label)((Ellipse)a.figure).Tag).Content);
                    ((Label)sp.Children[0]).Content = s.Attribute("label").Value;
                }
                else
                    ((Label)a.figure.Tag).Content = s.Attribute("label").Value;

                switch (s.Attribute("type").Value)
                {
                    case "initial":
                        a.type = stateType.initial;
                        a.addStartingArrow(canvasMain);
                        this.initialStates.Add(a);
                        break;
                    case "final":
                        a.figure.Stroke = Brushes.Black;
                        a.figure.StrokeThickness = 2;
                        this.finalStates.Add(a);
                        a.type = stateType.final;
                        break;
                    case "both":
                        a.addStartingArrow(canvasMain);
                        this.initialStates.Add(a);
                        a.figure.Stroke = Brushes.Black;
                        a.figure.StrokeThickness = 2;
                        this.finalStates.Add(a);
                        a.type = stateType.both;
                        break;
                }

                if (this is MooreTransitionDiagram)
                {
                    if(s.Attribute("output")!=null)
                        ((ComboBox)(((StackPanel)(((Label)((Ellipse)a.figure).Tag).Content)).Children[1])).Text = s.Attribute("output").Value.ToString();
                }

                State.resetLabels(); //to enable renaming from start if a new state is added after load
            }

            query = from transition in doc.Root.Element("Transitions").Elements("Transition")
                    select transition;

            foreach (XElement xt in query)
            {
                State s1 = null, s2 = null;
                
                int i = int.Parse(xt.Attribute("sourceStateIndex").Value);

                foreach (State s in states)
                {
                    if (s.index == i)
                    {
                        s1 = s;
                        break;
                    }
                }

                i = int.Parse(xt.Attribute("destStateIndex").Value);

                foreach (State s in states)
                {
                    if (s.index == i)
                    {
                        s2 = s;
                        break;
                    }
                }

                Transition t = this.addTransition(s1, s2, xt.Attribute("point1").Value);
                
                if (xt.Attribute("input") != null)
                    ((ComboBox)((StackPanel)t.figure.Tag).Children[0]).SelectedIndex = getIndexOfInput(xt.Attribute("input").Value);

                if (this is TuringTransitionDiagram)
                {
                    if(xt.Attribute("output")!=null)
                        ((ComboBox)((StackPanel)t.figure.Tag).Children[2]).SelectedIndex = getIndexOfInput(xt.Attribute("output").Value);
                    
                    if(xt.Attribute("direction")!=null)
                        ((ComboBox)((StackPanel)t.figure.Tag).Children[4]).SelectedValue = xt.Attribute("direction").Value;
                }

                if (this is MealyTransitionDiagram)
                {
                    if(xt.Attribute("output")!=null)
                        ((ComboBox)((StackPanel)t.figure.Tag).Children[2]).SelectedIndex = getIndexOfOutput(xt.Attribute("output").Value);
                }
            }
        }
    }
}

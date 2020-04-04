using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Automata_Simulator
{

    public enum stateType
    {
        initial, final, intermediate, both // initial and final both
    }

    public class State : IEquatable<State> // IEquatable is implemented for the operation of List<State>.contains() 
    {

        #region Generate unique state label
        static int stateLabelNumber = 0;
        static char stateLabelAlphabet = 'A';
        public Image startingArrow = null;

        public static string generateStateName()
        {
            if (stateLabelNumber > 9)
            {
                stateLabelAlphabet++;
                stateLabelNumber = 0;
            }

            string s = stateLabelAlphabet.ToString();
            s += stateLabelNumber.ToString();
            stateLabelNumber++;

            if (stateLabelAlphabet == 'Z' && stateLabelNumber == 9)
            {
                stateLabelAlphabet = 'a';
                stateLabelNumber = 0;
            }
            
            if (stateLabelAlphabet == 'z' && stateLabelNumber == 9)
            {
                stateLabelAlphabet = 'A';
                stateLabelNumber = 0;
            }

            return s;

        }

        public static void resetLabels()
        { 
            stateLabelNumber = 0;
            stateLabelAlphabet = 'A';
        }

        #endregion

        public ArrayList transitions = new ArrayList();

        public const int SIZE = 35;

        public Ellipse figure;

        public int index; // used for indexing this state in various arrays

        public stateType type = stateType.intermediate;

        #region new

        static List<Point> positions = new List<Point>();

        static int nextPositionIndex = 0; //index of the point where the next state is to be placed

        static State()
        {
            positions.Add(new Point(42, 127)); 
            positions.Add(new Point(262,94));
            positions.Add(new Point(436,160)); 
            positions.Add(new Point(422,319)); 
            positions.Add(new Point(261,269)); 
            positions.Add(new Point(184,404)); 
            positions.Add(new Point(63,320)); 
            positions.Add(new Point(80,513)); 
            positions.Add(new Point(251,520)); 
            positions.Add(new Point(415,606)); 
            positions.Add(new Point(491,485)); 
            positions.Add(new Point(342,414)); 
            positions.Add(new Point(641,360)); 
            positions.Add(new Point(625,254)); 
            positions.Add(new Point(746,126)); 
            positions.Add(new Point(809,211)); 
            positions.Add(new Point(846,384)); 
            positions.Add(new Point(573,87)); 
            positions.Add(new Point(920,71)); 
            positions.Add(new Point(936,283)); 
            positions.Add(new Point(733,450)); 
            positions.Add(new Point(609,477)); 
            positions.Add(new Point(762,590)); 
            positions.Add(new Point(611,600));
            positions.Add(new Point(851,523));
            positions.Add(new Point(966,431)); 
            positions.Add(new Point(981,574)); 
            positions.Add(new Point(903,595));
            positions.Add(new Point(848,652));
            positions.Add(new Point(484,654));
            positions.Add(new Point(282,642)); 
            positions.Add(new Point(188,643));
            positions.Add(new Point(167,571)); 
            positions.Add(new Point(57,636)); 
            positions.Add(new Point(26,576));

            //load the points from file
            //XDocument doc = XDocument.Load("abc.config");
            //foreach (XElement e in doc.Element("Points").Elements("Point"))
            //{
            //    positions.Add(Point.Parse(e.Value));
            //}
        }

        public static void resetPoints() 
        {
            nextPositionIndex = 0;
        
        }

        public static Point getNewStatePosition()
        {
            if (nextPositionIndex < positions.Count)
                return positions[nextPositionIndex++];
            else
            {
                Random rnd = new Random();
                return new Point((double)rnd.Next(700), (double)rnd.Next(700));
            }
        }

        #endregion

        public State(int index, Ellipse figure)
        {
            this.index = index;
            this.figure = figure;
            this.startingArrow = null;

        }

        public void onMouseDown(MouseButtonEventHandler f)
        {
            figure.MouseLeftButtonDown += f;
            ((Label)figure.Tag).MouseLeftButtonDown += f;
        }

        public void onMouseUp(MouseButtonEventHandler f)
        {
            figure.MouseLeftButtonUp += f;
            ((Label)figure.Tag).MouseLeftButtonUp += f;

        }

        public void addStartingArrow(Canvas canvasMain)
        {
            startingArrow = new Image();

            BitmapImage bi3 = new BitmapImage();
            
            startingArrow.Source = ((Image)Application.Current.MainWindow.Resources["startArrow"]).Source;
            startingArrow.Stretch = Stretch.Fill;
            startingArrow.Width = startingArrow.Height = 20;

            double left = (double)figure.GetValue(Canvas.LeftProperty);
            double top = (double)figure.GetValue(Canvas.TopProperty);
            
            startingArrow.SetValue(Canvas.LeftProperty, left - 19);
            startingArrow.SetValue(Canvas.TopProperty, top + 8);

            canvasMain.Children.Add(startingArrow);
            
        }

        public void removeStartingArrow(Canvas canvasMain)
        {
            canvasMain.Children.Remove(startingArrow);
            startingArrow = null;
        }

        public bool Equals(State other)
        {
            if (this.index == other.index)
                return true;
            else
                return false;

        }
    }

}

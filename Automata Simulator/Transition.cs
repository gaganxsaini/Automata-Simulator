using System;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace Automata_Simulator
{

    public abstract class Transition
    {
        public Path figure;
        public State sourceState;
        public State destState;
        public string input;

        public Point sPoint, ePoint; //staring point and ending point of the transition
        public Image arrow;//image of arrow


        public void onMouseUp(MouseButtonEventHandler f)
        {
            this.figure.MouseLeftButtonUp += f;
        }

        public void onMouseDown(MouseButtonEventHandler f)
        {
            this.figure.MouseLeftButtonDown += f;
        }

        public void onMouseEnter(MouseEventHandler f)
        {
            this.figure.MouseEnter += f;
        }

        public void onMouseLeave(MouseEventHandler f)
        {
            this.figure.MouseLeave += f;
        }

        public static void addArrow(Transition t, Canvas canvas)
        {
            try
            {
                Point startingPt = t.sPoint;
                Point endingPoint = t.ePoint;
                Path path = t.figure;

                String x1, y1, x2, y2;
                ArrowImage ai = new ArrowImage();
                PathGeometry g = path.Data.GetFlattenedPathGeometry();
                PointCollection p;


                foreach (var f in g.Figures)
                {
                    foreach (var s in f.Segments)
                    {
                        if (s is PolyLineSegment)
                        {
                            p = ((PolyLineSegment)s).Points;
                            //   Point pt = p[(p.Count) / 2];
                            Point pt1 = p[p.Count - 1];//4
                            Point pt2 = p[p.Count - 3];//2

                            double slope = (pt2.Y - pt1.Y) / (pt2.X - pt1.X);
                            double dy = pt2.X - pt1.X;
                            x1 = pt1.X.ToString();
                            y1 = pt1.Y.ToString();
                            x2 = pt2.X.ToString();
                            y2 = pt2.Y.ToString();

                            double angle = ((Math.Atan(((Convert.ToDouble(y2) - Convert.ToDouble(y1)) / (Convert.ToDouble(x2) - Convert.ToDouble(x1))))) * (180.0)) / (Math.PI);

                            if (angle < 0)
                            {
                                if (Convert.ToDouble(x1) < Convert.ToDouble(x2) && Convert.ToDouble(y1) > Convert.ToDouble(y2))
                                {
                                    angle = 180 + angle;
                                }


                                double temp = -angle;
                                temp = 90 - temp;
                                temp = 90 + temp;

                                angle = temp;
                            }

                            if (Convert.ToDouble(x1) > Convert.ToDouble(x2) && Convert.ToDouble(y1) > Convert.ToDouble(y2))
                            {
                                angle = 180 + angle;
                            }

                            canvas.Children.Remove(t.arrow);
                            t.arrow = ai.setImage(pt2, angle, canvas);
                            canvas.Children.Add(t.arrow);

                        }
                    }
                }
            }
            catch(Exception)
            {}

        }

        public abstract StackPanel generateLabel(Point p, TransitionDiagram d);

    }

    class nfaTransition : Transition
    {
        public override StackPanel generateLabel(Point p, TransitionDiagram d)
        {
            StackPanel s = new StackPanel();
            ComboBox cb = new ComboBox();
            cb.ItemsSource = d.inputs;
            cb.Tag = this;
            cb.SelectionChanged += d.onInputChange;
            s.Children.Add(cb);

            s.SetValue(Canvas.LeftProperty, p.X);
            s.SetValue(Canvas.TopProperty, p.Y);

            return s;
        }

    }

    class turingTransition : Transition
    {
        public override StackPanel generateLabel(Point p, TransitionDiagram d)
        {
            StackPanel s = new StackPanel();
            ComboBox cb1 = new ComboBox(), cb2 = new ComboBox(), cb3 = new ComboBox();
            cb1.ItemsSource = d.inputs;
            cb2.ItemsSource = d.inputs;
            cb3.ItemsSource = new string[3] { "R", "L", "S" };

            cb1.Tag = cb2.Tag = cb3.Tag = this;
            cb1.SelectionChanged += d.onInputChange;
            cb2.SelectionChanged += isEdited;
            cb3.SelectionChanged += isEdited;

            Label lbl1 = new Label(), lbl2 = new Label();
            lbl1.Content = lbl2.Content = "/";

            s.Orientation = Orientation.Horizontal;
            s.Children.Add(cb1);
            s.Children.Add(lbl1);
            s.Children.Add(cb2);
            s.Children.Add(lbl2);
            s.Children.Add(cb3);

            s.SetValue(Canvas.LeftProperty, p.X);
            s.SetValue(Canvas.TopProperty, p.Y);

            return s;

        }

        public void isEdited(object sender, SelectionChangedEventArgs e)
        {
            fileStatus.isEdited = true;

        }
    }

    class MooreTransition : Transition
    {
        public override StackPanel generateLabel(Point p, TransitionDiagram d)
        {
            StackPanel s = new StackPanel();
            ComboBox cb = new ComboBox();

            //cb.ItemsSource = d.inputs;
            foreach (string str in d.inputs)
                cb.Items.Add(str);
            
            cb.Tag = this;
            cb.SelectionChanged += d.onInputChange;

            s.Orientation = Orientation.Horizontal;
            s.Children.Add(cb);

            s.SetValue(Canvas.LeftProperty, p.X);
            s.SetValue(Canvas.TopProperty, p.Y);

            return s;
        }

    }

    class MealyTransition : Transition
    {
        public override StackPanel generateLabel(Point p, TransitionDiagram d)
        {
            StackPanel s = new StackPanel();
            ComboBox cb1 = new ComboBox();
            ComboBox cb2 = new ComboBox();
            Label slash = new Label();

            //cb1.ItemsSource = d.inputs;
            foreach (string str in d.inputs)
                cb1.Items.Add(str);

            cb1.Tag = this;
            cb1.SelectionChanged += d.onInputChange;
            cb2.SelectionChanged += d.onOutputChange;
            // cb2.ItemsSource = d.outputs;
            foreach (string str in d.outputs)
                cb2.Items.Add(str);
            cb2.Tag = this;
            cb2.IsEnabled = false;

            slash.Content = "/";



            s.Orientation = Orientation.Horizontal;
            s.Children.Add(cb1);
            s.Children.Add(slash);
            s.Children.Add(cb2);


            s.SetValue(Canvas.LeftProperty, p.X);
            s.SetValue(Canvas.TopProperty, p.Y);

            return s;
        }

    }

    class ArrowImage
    {
        public Image setImage(Point pt2, double angle, Canvas c)
        {

            Image myImage3 = new Image();
            
            myImage3.Stretch = Stretch.Fill;
            myImage3.Source = ((Image)Application.Current.MainWindow.Resources["arrow"]).Source;
            
            //Point po = new Point(10, 80);

            myImage3.Width = myImage3.Height = 20;

            myImage3.SetValue(Canvas.LeftProperty, pt2.X - 10);
            myImage3.SetValue(Canvas.TopProperty, pt2.Y - 10);
            myImage3.RenderTransform = new RotateTransform(angle, 10, 10);


            return myImage3;

        }
    }

}

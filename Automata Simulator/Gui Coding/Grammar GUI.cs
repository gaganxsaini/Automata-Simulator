using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Windows.Controls.Ribbon;

namespace Automata_Simulator
{
    public partial class MainWindow : RibbonWindow
    {
        Grammar grammar;

        private void menuGrammar_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (saveOnNewOrExit() == MessageBoxResult.Cancel)
                return;

            fileStatus.isEdited = false;

            mode = modes.Grammar;

            pda = null;
            diagram = null;

            action = actions.none;

            Application.Current.MainWindow.Title = "Grammar";

            addNewRow();

            transitionDiagramTab.Visibility = ExecuteTab.Visibility = pdaTab.Visibility = Visibility.Collapsed;

            grpDfaOptions.Visibility = grpNfaOptions.Visibility = Visibility.Collapsed;
            GrammarTab.Visibility = Visibility.Visible;

            GrammarTab.IsSelected = true;

            ribbonMain.Visibility = Visibility.Visible;

            grammar = new Grammar();

            loadedFilePath = "";

        }

        private void addNewRow()
        {
            fileStatus.isEdited = true;

            if (canvasMain.Children.Count == 0)
            {
                //grammer = new Grammar();
                StackPanel s = new StackPanel();
                s.SetValue(Canvas.LeftProperty, (double)20);
                s.SetValue(Canvas.TopProperty, (double)20);
                canvasMain.Children.Add(s);
            }

            btnAddSeparator.IsEnabled = true;
            
            StackPanel sp = ((StackPanel)canvasMain.Children[0]);

            if (sp.ActualHeight > canvasMain.ActualHeight)
                canvasMain.Height = canvasMain.ActualHeight + 50;

            StackPanel sp2 = new StackPanel();
            TextBox tb1 = new TextBox() { Width = 40, Height = 23, MaxLength = 1 };
            tb1.TextChanged += new TextChangedEventHandler(tb1_TextChanged);
            sp2.Children.Add(tb1);

            Label lbl = new Label() { Content = "->", ToolTip = "Click to Select the production" };
            lbl.MouseLeftButtonDown += new MouseButtonEventHandler(mutexSelectProduction);
            sp2.Children.Add(lbl);

            TextBox tb2=new TextBox(){ Width = 140, Height = 23 };
            tb2.KeyDown += new KeyEventHandler(tb2_KeyDown); 
            sp2.Children.Add(tb2);

            sp2.Orientation = Orientation.Horizontal;
            sp.Children.Add(sp2);
        }

        void tb2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                StackPanel sp= (StackPanel)((TextBox)sender).Parent;
                StackPanel sp2= (StackPanel)sp.Parent;
                if (sp2.Children.IndexOf(sp) == sp2.Children.Count - 1)
                {
                    addNewRow();
                }
             
            }
        }

        void tb1_TextChanged(object sender, TextChangedEventArgs e)
        {
            fileStatus.isEdited = true;

            TextBox tb1 = (TextBox)sender;
            if (tb1.Text != "")
            {
                tb1.Text = Char.ToUpper(tb1.Text[0]).ToString();
                if (!Char.IsUpper(tb1.Text[0]))
                {
                    MessageBox.Show("You can use only uppercase letters (A-Z) as Non-Terminals");
                    tb1.Text = "";
                }
                else
                {
                    ((TextBox)((StackPanel)tb1.Parent).Children[2]).Focus();
                }
            }

        }

        void loadProductionsFromTextBoxes()
        {
            bool isAllOK = true;

            foreach (StackPanel sp in ((StackPanel)canvasMain.Children[0]).Children)
            {
                bool temp = grammar.addProduction(sp);
                if (isAllOK == true)
                    isAllOK = temp;
            }

            if (!isAllOK)
                MessageBox.Show("Some Productions contains Errors. These will not be added into the Grammar");
        }

        void mutexSelectProduction(object sender, MouseButtonEventArgs e)
        {
            //returns true if anything gets selected 

            Label lbl = (Label)sender;

            if (grammar.selectedProduction != null) // if a produciton is already selected
            {
                foreach (UIElement c in grammar.selectedProductionStackPanel.Children)
                {
                    if (c is TextBox)
                    {
                        TextBox t = (TextBox)c;

                        t.BorderThickness = new Thickness(1);
                        t.BorderBrush = txtInputSymbols.BorderBrush;
                    }
                    else if (c is Label)
                    {
                        Label l = (Label)c;
                        l.Foreground = Brushes.Black;
                    }
                }

                if (grammar.selectedProductionStackPanel == (StackPanel)lbl.Parent) // if selected production is clicked again
                {
                    grammar.selectedProduction = null;
                    btnDelProd.IsEnabled = false;
                    btnSetStartSymbol.IsEnabled = false;
                    return;
                }
            }

            Production p = new Production();
            StackPanel sp = (StackPanel)lbl.Parent;
            p.LHS = ((TextBox)sp.Children[0]).Text;
            p.RHS.Add(((TextBox)sp.Children[2]).Text);

            grammar.selectedProductionStackPanel = sp;

            grammar.selectedProduction = p;

            foreach (UIElement c in ((StackPanel)lbl.Parent).Children)
            {
                if (c is TextBox)
                {
                    TextBox t = (TextBox)c;

                    Brush b = t.BorderBrush;

                    t.BorderThickness = new Thickness(2);
                    t.BorderBrush = Brushes.Blue;
                }
                else if (c is Label)
                {
                    Label l = (Label)c;
                    l.Foreground = Brushes.Blue;
                }
            }

            btnDelProd.IsEnabled = true;
            btnSetStartSymbol.IsEnabled = true;

        }

        private void btnDelProd_Click(object sender, RoutedEventArgs e)
        {
            if (grammar.selectedProduction != null)
            {
                fileStatus.isEdited = true;

                StackPanel sp = (StackPanel)grammar.selectedProductionStackPanel.Parent;
                int i = sp.Children.IndexOf(grammar.selectedProductionStackPanel);
                sp.Children.Remove(grammar.selectedProductionStackPanel);

                grammar.removeSelectedProduction();

                if (i < sp.Children.Count)
                {
                    Label lbl = (Label)((StackPanel)sp.Children[i]).Children[1];
                    mutexSelectProduction(lbl, null);
                }
                else
                    btnDelProd.IsEnabled = false;

                if (sp.Children.Count == 0)
                {
                    btnDelProd.IsEnabled = false;
                    btnSetStartSymbol.IsEnabled = false;
                }

            }
        }

        private void btnRemoveLeftRecursion_Click(object sender, RoutedEventArgs e)
        {
            loadProductionsFromTextBoxes();
            if (grammar.startProduction != null)
            {
            grammar = grammar.removeLeftRecursion();
            if (grammar != null)
            {
                loadGrammerIntoTextBoxes(grammar);
                showTrueGrammar();
            }
              }
            else
            {
                MessageBox.Show("Please set Staring Symbol.");
               
            }
        }
        private void btnRemoveLeftFactoring_Click(object sender, RoutedEventArgs e)
        {
            fileStatus.isEdited = true;

            loadProductionsFromTextBoxes();
            if (grammar.IsGrammarLeftFactoring(grammar))
            {
                loadGrammerIntoTextBoxes(grammar=grammar.RemoveLeftFactoring());
                showTrueGrammar();
            }
            
        }
         private void showTrueGrammar()
        {
            String str = "";
            if (Grammar.trueGrammar != null)
            {
                foreach (Production p in Grammar.trueGrammar.productions)
                {
                    int l = 0;
                    str += p.LHS + "->";
                    foreach (string s in p.RHS)
                    {
                        if (l < p.RHS.Count - 1)
                        {
                            str += s + "|";

                        }
                        else
                        {
                            str += s;
                        }

                        l++;
                    }
                    str += "\n";
                }
                MessageBox.Show(str, "True Grammar", MessageBoxButton.OK, MessageBoxImage.Information);
                Grammar.trueGrammar.clearGrammar();
            }
            
            
        }
        static Grammar originalGrammar;
        
        private void btnRemoveUselessProductions_Click(object sender, RoutedEventArgs e)
        {
            loadProductionsFromTextBoxes();

            originalGrammar = grammar.createClone();
            originalGrammar.startProduction = grammar.startProduction;

            if (originalGrammar.startProduction != null)
            {
                fileStatus.isEdited = true;

                originalGrammar = grammar.eliminateDependency();
                List<string> usefullSymbols = new List<string>();
                usefullSymbols = originalGrammar.getUsefullSymbols();
                Grammar intermediateGrammar = grammar.removeUselessSymbols(usefullSymbols);
                intermediateGrammar.removeUselessProduction();


                loadGrammerIntoTextBoxes(intermediateGrammar);
                grammar = intermediateGrammar;   // for further processing
            }
            else
            {
                MessageBox.Show("Error:Set Starting Symbol.");
            }
        }

        private void btnComputeFirst_Click(object sender, RoutedEventArgs e)
        {
            if (grammar.startProduction != null)
            {
                fileStatus.isEdited = true;

                loadProductionsFromTextBoxes();
                Grammar g = grammar.eliminateDependency();
                g.ComputeFirst();

            }
            else
            {
                MessageBox.Show("Error:Set Starting Symbol.");
            }
        }

        private void btnComputeFollow_Click(object sender, RoutedEventArgs e)
        {
            loadProductionsFromTextBoxes();
            
            if (grammar.startProduction != null)
            {
                fileStatus.isEdited = true;

                grammar.eliminateDependency();
                grammar.mydata[0, 0] = grammar.startProduction.LHS;
                grammar.mydata[0, 1] = "$`";
                grammar.ComputeFollow();

                String str="";
                int j=0;
                foreach (Production p in grammar.productions)
                {
                    grammar.resultTable[j, 0] = p.LHS;
                    j++;
                }

                 for (int i = 0; i < Grammar.count; i++)
                {
                if (grammar.resultTable[i, 0]!="")
            	str += "FOLLOW[" + grammar.resultTable[i, 0] + "]=" + grammar.removeMultipleCharacter(grammar.resultTable[i, 1]) + "\n";

                 }
                    MessageBox.Show(str, "FOLLOW of Grammar", MessageBoxButton.OK, MessageBoxImage.Information);

                    Grammar.count = 0;
            }
            else
            {
                MessageBox.Show("Error:Set Staring Symbol.");
            }
        }


        private void loadGrammerIntoTextBoxes(Grammar g)
        {
            canvasMain.Children.Clear();
            foreach (Production p in g.productions)
            {
                addNewRow();

                StackPanel sp = (StackPanel)canvasMain.Children[0];
                StackPanel sp2 = (StackPanel)sp.Children[sp.Children.Count - 1];
                ((TextBox)sp2.Children[0]).Text = p.LHS;

                if (g.startProduction != null && p.LHS == g.startProduction.LHS)
                {
                    ((TextBox)sp2.Children[0]).Background = Brushes.LightGreen;
                    g.startProductionStackPanel = sp2;
                }
                
                string rhs = "";

                foreach (string s in p.RHS)
                    rhs += s + "|";

                ((TextBox)sp2.Children[2]).Text = rhs.Trim('|');
            }
        }

        private void btnAddSeparator_Click(object sender, RoutedEventArgs e)
        {
            foreach (StackPanel sp in ((StackPanel)canvasMain.Children[0]).Children)
            {
                TextBox tb2 = (TextBox)sp.Children[2];
                if (tb2.IsFocused)
                {
                    string s = tb2.Text.Trim('|');
                    s = "|" + s;
                    tb2.Text = s;
                }
            }
            
        }

        private void btnSetStartSymbol_Click(object sender, RoutedEventArgs e)
        {
            if (grammar.selectedProduction != null)
            {
                fileStatus.isEdited = true;

                if (grammar.startProduction != null)
                    ((TextBox)grammar.startProductionStackPanel.Children[0]).Background = Brushes.White;

                grammar.startProduction = grammar.selectedProduction;
                grammar.startProductionStackPanel = grammar.selectedProductionStackPanel;

                ((TextBox)grammar.selectedProductionStackPanel.Children[0]).Background = Brushes.LightGreen;
            }

        }

        private void btnRemoveUnitProductions_Click(object sender, RoutedEventArgs e)
        {
            loadProductionsFromTextBoxes();

            Grammar g = grammar.eliminateDependency();
            g.startProduction = grammar.startProduction;
            originalGrammar = grammar.createClone();
            originalGrammar.startProduction = g.startProduction;

            if (originalGrammar.startProduction != null)
            {
                fileStatus.isEdited = true;

                List<string> usefullSymbols = new List<string>();
                usefullSymbols = originalGrammar.getUsefullSymbols();
                Grammar intermediateGrammar = g.removeUselessSymbols(usefullSymbols);
                intermediateGrammar.removeUselessProduction();


                loadGrammerIntoTextBoxes(intermediateGrammar);
                grammar = intermediateGrammar;
            }
            else
            {
                MessageBox.Show("Error:Set Starting Symbol.");
            }
        }

        private void btnRemoveNullProductions_Click(object sender, RoutedEventArgs e)
        {
            loadProductionsFromTextBoxes();
            originalGrammar = grammar.createClone();
            originalGrammar.startProduction = grammar.startProduction;
 
            if(grammar.IsGrammarNullable())
            {
                if (originalGrammar.startProduction != null)
                {
                    fileStatus.isEdited = true;

                    originalGrammar = grammar.eliminateDependency();
                    List<string> nullableSymbols = new List<string>();
                    nullableSymbols = originalGrammar.getnullableSymbols();
                    Grammar newGrammar = originalGrammar.RemoveNullProductions(nullableSymbols);
                    loadGrammerIntoTextBoxes(newGrammar);

                    grammar = newGrammar;   // for further processing
                }
                else
                    MessageBox.Show("Error:Set starting Symbol");
            
            }
        }

    }
}
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Windows.Controls.Ribbon;
using System;

namespace Automata_Simulator
{

    public partial class MainWindow : RibbonWindow
    {

        enum pdaTypes
        { 
            DPDA, NDPDA, None
        }

        pdaTypes pdaType = pdaTypes.None;
        
        private void menuDPda_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (saveOnNewOrExit() == MessageBoxResult.Cancel)
                return;

            fileStatus.isEdited = false;
            loadedFilePath = "";
            
            mode=modes.Pda;
            pdaType = pdaTypes.DPDA;
            action = actions.none;

            pda = new DPda();

            Application.Current.MainWindow.Title = "Push Down Automata";

            ribbonMain.Visibility = Visibility.Visible;

            setInterfaceOnNewPda();
        }


        public void setInterfaceOnPdaLoad()
        {
            String str = "";
            foreach(string s in pda.inputs)
                str += s + ";";
            
            txtPdaInputSymbols.Text = str.Trim(';');
            spPdaInputSymbols.IsEnabled = false;

            btnPdaOk.IsEnabled = false;
        }

        void setInterfaceOnNewPda()
        {

            grammar = null;
            diagram = null;
            
            pda.setCanvas(canvasMain, btnPdaDelSelection, btnPdaInitial, btnPdaFinal);

            txtPdaInputSymbols.Text = "";
            transitionDiagramTab.Visibility = GrammarTab.Visibility = Visibility.Collapsed;
            grpDfaOptions.Visibility = grpNfaOptions.Visibility = Visibility.Collapsed;
            
            pdaTab.Visibility = Visibility.Visible;
            ExecuteTab.Visibility=Visibility.Visible;
            pdaTab.IsSelected = true;

            spPdaInputSymbols.IsEnabled = true;

            grpStringAcceptance.Visibility = Visibility.Visible;
            grpOutputProducer.Visibility = Visibility.Collapsed;

            grpPdaSymbols.IsEnabled = true;
            grpPdaSelection.IsEnabled = true;
            btnPdaOk.IsEnabled = true;

            grpPdaStateOptions.IsEnabled = true;
            btnPdaFinal.IsEnabled = false;
            btnPdaInitial.IsEnabled = true;
        }

        private void btnPdaOk_Click(object sender, RoutedEventArgs e)
        {
            if ((txtPdaInputSymbols.IsEnabled == true && txtPdaInputSymbols.Text == ""))
            {
                MessageBox.Show("Enter Input Symbols");
                return;
            }

            removeDuplicateSymbols(txtPdaInputSymbols);
            pda.inputs = txtPdaInputSymbols.Text.Split(';').ToList();
            spPdaInputSymbols.IsEnabled = false;

            pda.stackSymbols = pda.inputs.ToList();
            if (!pda.inputs.Contains("^"))
                pda.stackSymbols.Add("^");//stack's last symbol (empty stack)

            pda.addNewRow();

            btnPdaOk.IsEnabled = false;
        }

        private void btnPdaClear_Click(object sender, RoutedEventArgs e)
        {
            if (canvasMain.Children.Count == 0)
                return;
            else
            {
                if (MessageBox.Show("Are you sure to clear the Page", "Clear?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }

            grpPdaStateOptions.IsEnabled = false;
            btnPdaInitial.IsChecked = btnPdaFinal.IsChecked = false;

            Pda newPda = null;

            switch (pdaType)
            {
                case pdaTypes.DPDA:
                    newPda = new DPda();
                    break;
                case pdaTypes.NDPDA:
                    newPda = new NDPda();
                    break;
            }

            if (newPda != null)
            {
                newPda.inputs = pda.inputs;
                newPda.stackSymbols = pda.stackSymbols;

                canvasMain.Children.Clear();
                newPda.setCanvas(canvasMain, btnDelSelection, btnInitial, btnFinal);
                pda = newPda;
                pda.addNewRow();
            }

        }

        private void btnPdaInitial_Click(object sender, RoutedEventArgs e)
        {
            fileStatus.isEdited = true;

            if (btnPdaInitial.IsChecked == true)
            {
                if (pda.selectedRow != null)
                {
                    if (pda.initialState != null)
                        pda.initialState.Background = Brushes.White;
                    pda.initialState = pda.selectedRow.txtCurrentState;
                    pda.initialState.Background = Brushes.LightGreen;
                }
            }
            else
            {
                if (pda.selectedRow != null)
                {
                    pda.initialState.Background = Brushes.White;
                    pda.initialState = null;
                }

            }
        }

        private void btnPdaFinal_Click(object sender, RoutedEventArgs e)
        {
            fileStatus.isEdited = true;

            if (btnPdaFinal.IsChecked == true)
            {
                if (pda.selectedRow != null)
                {
                    if (pda.finalState != null)
                        pda.finalState.Background = Brushes.White;
                    pda.finalState = (TextBox)pda.selectedRow.sp.Children[5];
                    pda.finalState.Background = Brushes.LightPink;
                }

            }
            else
            {
                if (pda.selectedRow != null)
                {
                    pda.finalState.Background = Brushes.White;
                    pda.finalState = null;
                }
            }

        }
    }
}
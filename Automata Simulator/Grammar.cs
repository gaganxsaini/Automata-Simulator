using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Xml.Linq;


namespace Automata_Simulator
{
    class Grammar
    {
        public static Canvas canvasMain = null;

        public List<Production> productions = new List<Production>();
        public Production selectedProduction;
        public StackPanel selectedProductionStackPanel = null;

        public Production startProduction; 
        // the RHS of SelectedProdcution is not formatted i.e. it contains complete text of tb2 e.g. a|b|c

        public StackPanel startProductionStackPanel=null;

        public List<String> nonTerminals = new List<string>();// have the non terminals that are already used in the productions 

        public static void setCanvas(Canvas c)
        {
            canvasMain = c;
        }

        public bool addProduction(StackPanel sp)
        {
            TextBox tb1 = (TextBox)sp.Children[0]; // tb1 can only have one character
            Label lbl = (Label)sp.Children[1];
            TextBox tb2 = (TextBox)sp.Children[2];

            if (tb2.Text == "" || tb1.Text == "" || tb1.Text.Length>1)
                return false;

            Production p = null;

            foreach (Production d in productions)
            {
                if (d.LHS.ToString() == tb1.Text)
                {
                    p = d;
                    break;
                }
            }

            if (p == null)//no prodcution with the same LHS exist
            {
                p = new Production();
                p.LHS = tb1.Text;
                productions.Add(p);
            }

            if (tb2.Text.Contains('|'))
            {
                string s = tb2.Text.Trim('|');
                string[] str = s.Split('|');
                foreach (string item in str)
                {
                    if (!p.RHS.Contains<String>(item))
                        p.RHS.Add(item);
                }
            }
            else
            {
                if (!p.RHS.Contains<String>(tb2.Text))
                    p.RHS.Add(tb2.Text);
            }

            if (!nonTerminals.Contains(p.LHS))
                nonTerminals.Add(p.LHS);

            foreach (string s in p.RHS)
            {
                foreach (char c in s)
                {
                    if (!char.IsLower(c))
                    {
                        if (!nonTerminals.Contains(c.ToString()))
                            nonTerminals.Add(c.ToString());
                    }
                }
                
            }

            return true;
        
        }

        public void removeSelectedProduction()
        {
            if (this.selectedProduction == this.startProduction)
                this.startProduction = null;

            if (selectedProduction.LHS == "")
                return;

            Production p = null;
            foreach (Production d in productions)
            {
                if (d.LHS == selectedProduction.LHS)
                {
                    p = d;
                    break;
                }
            }

            if (p == null)
                return;

            if (selectedProduction.RHS[0].Contains('|'))
            {
                string s=selectedProduction.RHS[0].Trim('|');
                string[] str = selectedProduction.RHS[0].Split('|');
                foreach (String item in str)
                    p.RHS.Remove(item);
            }
            else
                p.RHS.Remove(selectedProduction.RHS[0]);

            if (p.RHS.Count == 0)
                productions.Remove(p);

            selectedProduction = null;
            
        }

        public string getUniqueNonTermainal()
        {
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (!nonTerminals.Contains(c.ToString()))
                    return c.ToString();
            }

            return null;
        }

        void abc()
        {
            string s = getUniqueNonTermainal();
            if (s == null)
                System.Windows.MessageBox.Show("Error");
            else
                System.Windows.MessageBox.Show(s);

        }

        public static Grammar trueGrammar = new Grammar();
        
        public Grammar removeLeftRecursion()
        {
            Grammar g = new Grammar();
           
            foreach (Production p in productions)
            {
                if (isLeftRecursive(p))
                {
                
                    Production Prod = new Production();   //resulted productions
                    Production dashProd = new Production(); //resulted dash production
                    Production tProd = new Production();
                    Production tDashPro = new Production();

                    Prod.LHS = p.LHS;
                    tProd.LHS = p.LHS;
                    if (p.LHS == startProduction.LHS)
                    {
                        g.startProduction = Prod;
                    }

                    dashProd.LHS = getUniqueNonTermainal();
                    tDashPro.LHS = p.LHS + "'";

                    if (!nonTerminals.Contains(dashProd.LHS))
                        nonTerminals.Add(dashProd.LHS);

                    foreach (string s in p.RHS)
                    {
                        if (p.LHS == s[0].ToString())
                        {
                            if (s.Substring(1).Length != 0)
                            {
                                dashProd.RHS.Add(s.Substring(1) + dashProd.LHS);
                                tDashPro.RHS.Add(s.Substring(1) + tDashPro.LHS);
                            }
                        }
                        else
                        {
                            Prod.RHS.Add(s + dashProd.LHS);
                            tProd.RHS.Add(s + tDashPro.LHS);
                        }
                    }
                    dashProd.RHS.Add("^");
                    tDashPro.RHS.Add("^");

                    g.productions.Add(Prod);
                    g.productions.Add(dashProd);
                   
                    trueGrammar.productions.Add(tProd);
                    trueGrammar.productions.Add(tDashPro);

                }
                else
                {
                    g.productions.Add(p);
                   
                    trueGrammar.productions.Add(p);
                }
            }
            
            return g;
        } 
        
        public bool IsGrammarNullable()
        {
            foreach (Production p in productions)
            {
                foreach (string s in p.RHS)
                {
                    if (s == "^")
                        return true;
                }
            }

            return false;
        }

        public bool IsProductionLeftFactoring(Production p)
        {
            foreach (string s in p.RHS)
            {
                foreach (string ss in p.RHS)
              {
                  if (s != ss)
                  {
                      if (s[0] == ss[0])
                          return true;
							break;
                  }
                
                }
            }
            return false;

        }
        public List<string> NonLeftFactoringRHS(List<string> RHS,Production p)
        {
            List<string> ls = new List<string>();
           
            foreach (string s in p.RHS)
            {
                int flag = 0;
                foreach (string ss in RHS)
                {
                    if (s.IndexOf(ss, 0) >= 0)
                        flag++;
                }
                if (flag == 0)
                {
                    if (!ls.Contains(s))
                        ls.Add(s);
                }

            }
            return ls;
        }
        public bool IsGrammarLeftFactoring(Grammar g)
        {
            foreach (Production p in g.productions)
            {
                if (IsProductionLeftFactoring(p))
                    return true;
            }
            return false;
        }
        public Grammar RemoveLeftFactoring()
        {
            trueGrammar.clearGrammar();
            Grammar g = new Grammar();
            foreach (string t in nonTerminals)
            {
                if (!g.nonTerminals.Contains(t))
                    g.nonTerminals.Add(t);
            }


            foreach (Production p in productions)
            {
                if (IsProductionLeftFactoring(p))
                {
                    List<string> CommonStr = ExtractCommon(p.RHS);
                    List<string> data = ProceesIt(CommonStr);
                    List<string> sameRhs = NonLeftFactoringRHS(data, p);
                    Production  newProduction=new Production();
                                newProduction.LHS=p.LHS;
                    Production tnewProduction = new Production();
                               tnewProduction.LHS = p.LHS;
                    
                    if (p.LHS == this.startProduction.LHS)
                        g.startProduction = newProduction;

                    int noOfDash = 1;
                    foreach (string d in data)
                    {   
                        Production newDashProduction = new Production();
                        Production tnewDashProduction = new Production();
                        newDashProduction.LHS = g.getUniqueNonTermainal();
                        tnewDashProduction.LHS = tnewProduction.LHS+noOfDash;
                        g.nonTerminals.Add(newDashProduction.LHS);
                        newProduction.RHS.Add(d + newDashProduction.LHS);
                        tnewProduction.RHS.Add(d + tnewDashProduction.LHS);

                        foreach (string dd in p.RHS)
                        {
                            int index = dd.IndexOf(d, 0);
                            
                            if (index >= 0)
                                index = index + d.Length;
                
                            if (index >= 0)
                            {
                                string temp = dd.Substring(index);
                                if (temp == "")
                                {if(!newDashProduction.RHS.Contains("^"))
                                    newDashProduction.RHS.Add("^");
                                    tnewDashProduction.RHS.Add("^");

                                }
                                else
                                {
                                    if (!newDashProduction.RHS.Contains(temp))
                                    {
                                        newDashProduction.RHS.Add(temp);
                                        tnewDashProduction.RHS.Add(temp);
                                    }
                                }
                                
                            }
                        }

                        if (!g.productions.Contains(newDashProduction))
                        {
                            g.productions.Add(newDashProduction);
                            trueGrammar.productions.Add(tnewDashProduction);
                        }
                        noOfDash++;
                    }
                    foreach (string s in sameRhs)
                    {
                        if (!newProduction.RHS.Contains(s))
                        {    newProduction.RHS.Add(s);
                             tnewProduction.RHS.Add(s);
                        }
                    }
                    g.productions.Add(newProduction);
                    trueGrammar.productions.Add(tnewProduction);

                }
                else
                {
                    if (p.LHS == this.startProduction.LHS)
                        g.startProduction =p;

                    g.productions.Add(p);
                    trueGrammar.productions.Add(p);
                }
              
            }
            
            if (!IsGrammarLeftFactoring(g))
                return g;
            else
                return g.RemoveLeftFactoring();
            
        }

        public List<string> ProceesIt(List<string> list)
        {
            List<string> ls = new List<string>();
            foreach (string s in list)
            {
                int flag = 0;
                foreach (string t in list)
                {
                    if (s != t)
                    {
                        if (s.IndexOf(t,0) >= 0)
                            flag = 1;
                    }
                }
             
                if (flag == 0)
                    ls.Add(s);
            }
            return ls;
        }
        public List<string>  ExtractCommon(List<string> RHS)
        {
            List<string> ls = new List<string>();
            foreach (string s in RHS)
            {
                foreach (string ss in RHS)
                {
                    if (s[0] == ss[0])
                    {
                        if (s != ss)
                        {
                            string temp = StringDiff(s, ss);
                            if (temp != null)
                            {
                                if (!ls.Contains(temp))
                                    ls.Add(temp);
                            }
                        }
                    }
                }

            }

            return ls;
        }
        public string StringDiff(string str1,string str2)
        {
            int min;
            if (str1.Length <= str2.Length)
            {
                min = str1.Length;

            }
            else
            {
                min = str2.Length;

            }
            int index = -1;
            for (int i = 0; i < min; i++)  // check for equal to
            {
                if (str1[i] != str2[i] && Convert.ToUInt16(str1[i]) != Convert.ToUInt16(str2[i]))
                {
                    index = i;
                    break;
                }
            }
            if (index==-1)
            {
                if (min == 0)
                    return str1;
                else
                    return str1.Substring(0, min); 
            }
            else
                return str1.Substring(0, index);

        }
        
        public void clearGrammar()
        {
            this.productions.Clear();
        }

        public bool isLeftRecursive(Production production)
        {
            foreach (string s in production.RHS)
            {
                if (production.LHS == s[0].ToString())
                    return true;
            }
            return false;
        }


        #region Remove Useless Productions
       
        public List<string> returnRHS(string LHS)
        {
            foreach (Production p in productions)
            {
                if (p.LHS == LHS)
                {
                    return p.RHS;
                    break;
                }
            }
            return null;
        }
        public bool isTerminalString(string str)
        {
            
            foreach (char c in str)
            {
                if (Convert.ToInt16(c) >= 65 && Convert.ToInt16(c) <= 90)
                {
                    return false;
                    break;

                }
            }
            return true;

        }
        public bool isCombinationOfTerminalAndNonTerminal(string str,List<string> UsefullSymbols)
        {
            int flag=0;
            foreach(char c in str)
            {
                if(Convert.ToInt16(c) >= 65 && Convert.ToInt16(c) <= 90)
                {
                    if (!UsefullSymbols.Contains(c.ToString()))
                    {
                        flag++;
                    }
                
                }
            
            }
            if (flag == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        
        }
        public Grammar createClone()
        {
            Grammar gg = new Grammar();
            foreach (Production p in productions)
            {
                Production pp = new Production();
                foreach (string s in p.RHS)
                {
                    pp.RHS.Add(s);
                }
                pp.LHS = p.LHS;
                gg.productions.Add(pp);

            }
            return gg;
        }
        public Grammar removeUselessSymbols(List<string> usefullSymbols)
        {
            Grammar newGrammar = new Grammar();
            newGrammar.startProduction=this.startProduction;
            foreach (Production p in productions)
            {
                Production newProduction = new Production();

                if (p.LHS == this.startProduction.LHS || usefullSymbols.Contains(p.LHS))
                {
                    newProduction.LHS = p.LHS;
                    foreach (string s in p.RHS)
                    {
                        if (isCombinationOfTerminalAndNonTerminal(s, usefullSymbols) | (usefullSymbols.Contains(p.LHS) && isTerminalString(s)))
                        {   if(!newProduction.RHS.Contains(s))
                            newProduction.RHS.Add(s);

                        }

                    }
                    newGrammar.productions.Add(newProduction);
           
                }
                
            }
            return newGrammar;
        }
        public string returnLHS(string str)
        {
            foreach (Production p in productions)
            {
                if (p != startProduction)
                {
                    foreach (string s in p.RHS)
                    {
                        if (s == str)
                        {
                            return p.LHS;
                            break;
                        }
                    }
                }
            }
            return null;
        }
        public Production returnProduction(string str)
        {
            foreach (Production p in productions)
            {
                if (p.LHS==str)
                    return p;
            }
            return null;

        }
        public List<string> getUsefullSymbols()
        {

           
            // finding all usefull symbols
            List<string> usefullSymbols = new List<string>();
           
            for (int i = 0;i < productions.Count; i++)
            {

                foreach (Production p in productions)
                {
                    foreach (string s in p.RHS)
                    {
                        if (isTerminalString(s))  // is the string s is of form   aaa,abbb,ababab means only terminals
                        {

                            if (!usefullSymbols.Contains(p.LHS))
                            {
                                usefullSymbols.Add(p.LHS);
                            }
                        }
                        if (isCombinationOfTerminalAndNonTerminal(s, usefullSymbols))  //Is the string is of form aAb,AB, etc. means combination of terminals and NON Terminals in the usefull symbols
                        {

                            if (!usefullSymbols.Contains(p.LHS))
                            {
                                usefullSymbols.Add(p.LHS);
                            }
                        }

                    }
                }

            }
            return usefullSymbols;
        }
        public void removeUselessProduction()
        {
            List<Production> uselessProductions = new List<Production>();

            for (int i = 0; i < productions.Count; i++)
            {
                foreach (Production p in this.productions)
                {
                    if (p.LHS != null)
                    {

                        int status = 0;
                        foreach (Production pp in this.productions)
                        {

                            if (pp.LHS != p.LHS)
                            {
                                foreach (string s in pp.RHS)
                                {
                                    foreach (char c in s)
                                    {
                                        if (p.LHS == c.ToString())
                                        {
                                            status++;
                                            break;
                                        }
                                    }

                                }

                            }

                        }
                        if (status == 0)
                        {
                            if (p.LHS != this.startProduction.LHS)
                                if (!uselessProductions.Contains(p))
                                    uselessProductions.Add(p);
                        }
                    }

                }

                foreach (Production p in uselessProductions)
                    this.productions.Remove(p);
            }

        }
        #endregion

        public List<string> getnullableSymbols()
        {
            List<string> nullableSymbols = new List<string>();
            for (int i = 0; i < productions.Count; i++)
            {

                foreach (Production p in productions)
                {
                    foreach (string s in p.RHS)
                    {
                        if (s=="^")   //if string is null
                            if (!nullableSymbols.Contains(p.LHS))
                                nullableSymbols.Add(p.LHS);

                        if (isNullableDueToSubstitution(s, nullableSymbols))  //S->AB and A<B are nullable ,then s is also nullable
                            if (!nullableSymbols.Contains(p.LHS))
                                nullableSymbols.Add(p.LHS);
                    }
                }

            }
            return nullableSymbols;

        }

        private bool isNullableDueToSubstitution(string str, List<string> nullableSymbols)
        {
            int flag = 0;
            foreach (char c in str)
            {
                if (Convert.ToInt16(c) >= 65 && Convert.ToInt16(c) <= 90)
                    if (!nullableSymbols.Contains(c.ToString()))
                        flag++;
            }
            if (flag == 0)
                return true;
            else
                return false;
            
        }

        List<string> nullTermainals = new List<string>();// contains Non terminals which derives null

        public Grammar RemoveNullProductions(List<string> nullableSymbols)
        {
            Grammar gg = new Grammar();
            Grammar newGrammar = new Grammar();
            
            foreach (Production p in productions)
            {
                Production p1 = new Production();

                foreach (string s in p.RHS)
                {
                    List<string> list = new List<string>();
                    
                    foreach(string n in nullableSymbols)
                    {
                        foreach(string t in (List<string>)removeNull(s,n))
                        { 
                            if(!list.Contains(t))
                                list.Add(t);
                        }
                    }
                            
                    if (s == "^"|list.Count!=0)
                    {
                        if (list.Count != 0)
                        {
                            foreach (string temp in list)
                            {
                                if (!p1.RHS.Contains(temp)&&temp!="^"&&temp!="")
                                {
                                    p1.LHS = p.LHS;
                                    p1.RHS.Add(temp);
                                }
                            }
                        }
                       // nullTermainals.Add(p.LHS);
                    
                    }
                       
                    else
                    {
                        if (!p1.RHS.Contains(s)&&s!="")
                        {
                            p1.LHS = p.LHS;
                            p1.RHS.Add(s);
                        }
                    }
                }
                
                if(!gg.productions.Contains(p1)&&p1.LHS!=null)
                    gg.productions.Add(p1);
            }

            //till this just created new grammar after removing null productions

            foreach (Production p in gg.productions)
            {
                Production pp = new Production();
                pp.LHS = p.LHS;
                foreach (string s in p.RHS)
                {
                    if (!pp.RHS.Contains(s) && pp.LHS != s && s != "^")
                        pp.RHS.Add(s);
                    foreach (string ss in nullableSymbols)
                    {
                        List<string> list = new List<string>();
                        list = removeNull(s, ss);
                        if (list != null)
                        {
                            foreach (string b in list)
                            {
                                if (!pp.RHS.Contains(b) && pp.LHS != b&&b!="")
                                    pp.RHS.Add(b);
                            }
                        }
                    }
                }

                if(!newGrammar.productions.Contains(pp)&&pp.LHS!=null)
                    newGrammar.productions.Add(pp);
            }
           
            return newGrammar;
        }

        public List<string> removeNull(String str, String nullSymbol)
        {
            int index1 = 0;
            int index2 = 0;
            List<string> newRHS = new List<string>();
            int count = findCount(str, nullSymbol);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    List<string> RHS = new List<string>();
                    index2 = str.IndexOf(nullSymbol, index1);
                    String temp = str.Substring(0, index2) + str.Substring(index2 + 1);
                    if (!newRHS.Contains(temp))
                        newRHS.Add(temp);
                    if (findCount(temp, nullSymbol) > 0)
                        RHS = removeNull(temp, nullSymbol);
                    if (RHS != null)
                    {
                        foreach (string s in RHS)
                        {
                            if (!newRHS.Contains(s)&&s!="")
                                newRHS.Add(s);
                        }
                    }
                    index1 = index2 + 1;

                }
                return newRHS;
            }
            else
            {               
                newRHS.Add(str);
                return newRHS;
                 
            }
        }

        public int findCount(string str, string element)
        {
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str.Substring(i, 1) == element)
                    count++;
            }
            return count;
        }

        public string[,] resultTable = new string[100, 2]; //final result table
        public string[,] addTable = new string[100, 2]; //follow produxtion ->on the productions
        public string[,] mydata = new string[100, 2];
        //int specialCounter = 0;
        int cc = 1;

        public void ComputeFirst()
        {
            string str = "";
            foreach (Production p in productions)
            {
                str += "FIRST[" + p.LHS + "]="+removeMultipleCharacter(findFirst(p))+"\n";
               
            }
            MessageBox.Show(str, "FIRST of Grammar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static int count = 0;

        public void ComputeFollow()
        {

            foreach (Production p in productions)
            {
                foreach (string s in p.RHS)
                {
                    string str1 = FollowRule1(s);
                    if (str1 != "")
                    {
                        string[] oo = str1.Split('|');
                        for (int k = 0; k < oo.Length; k++)
                        {
                            if (oo[k] != "")
                            {
                                int index = oo[k].IndexOf("?");

                                mydata[cc, 0] = oo[k].Substring(0, 1);
                                mydata[cc, 1] = oo[k].Substring(index + 1);

                                cc++;
                            }
                        }
                    }

                }
                FollowRule2(p.LHS);
                count++;
            }

            for (int i = 0; i < cc; i++)
            {
                if (addTable[i, 0] != null)
                {
                    string data = addTable[i, 0];

                    for (int j = 0; j < data.Length; j++)
                    {
                        if (data.Substring(j, 1) != null)
                        {
                            string part = data.Substring(j, 1);

                            for (int k = 0; k < cc; k++)
                            {
                                if (mydata[k, 0] == part)
                                    resultTable[i, 1] += mydata[k, 1] + "`";
                            }
                        }

                    }
                }

            }

            for (int i = 0; i < cc; i++)
            {
                int index = findIndex(mydata[i, 0]);
                resultTable[index, 1] += mydata[i, 1] + "`";

            }
        }

        public string FollowRule1(string element)
        {
            string output = "";

            string rule = element;

            for (int k = 0; k < rule.Length; k++)
            {
                string character = rule.Substring(k, 1);
                if (Convert.ToInt16(Convert.ToChar(character)) >= 65 && Convert.ToInt16(Convert.ToChar(character)) <= 90 && character != "ε") // A->alpha,B,beta   if first character is terminal  and beta not null
                {
                    if (k != rule.Length - 1)
                    {

                        string first = removeMultipleCharacter(firstRule(rule.Substring(k + 1)));

                        int indx = first.IndexOf("^");
                        if (indx < 0)
                            output += character + "?" + first + "|";
                        else
                        {
                            string temp = first.Remove(indx, 1);
                            int i = findIndex(character);
                            resultTable[i, 0] = character;
                            resultTable[i, 1] = temp;
                            int ptr = findIndex(character);
                            if (ptr >= 0)
                                addTable[ptr, 0] += element;
                            
                            output += character + "?" + temp;
                        }
                    }
                }
            }

            return output;

        }

        public void FollowRule2(string element)
        {

            foreach (Production p in productions)
            {
                foreach (string s in p.RHS)
                {
                    string lastCharacter = s.Substring(s.Length - 1, 1);
                    if (Convert.ToInt16(Convert.ToChar(lastCharacter)) >= 65 && Convert.ToInt16(Convert.ToChar(lastCharacter)) <= 90) // A->alpha,B,beta   if first character is terminal  and beta not null
                    {
                        int idx = findIndex(lastCharacter);
                        addTable[idx, 0] += element;
                    }
                }
            }

        }

        public int findIndex(string element)
        {
            int index = -1;
            int c = 0;
            foreach (Production p in productions)
            {
                if (p.LHS == element)
                {
                    index = c;
                    break;
                }
                c++;
            }
            return index;
        }

        Production parentProduction = null;

        public string findFirst(Production p)
        {
            String output = "";
            if (p != null)
            {
                parentProduction = p;
               
                foreach (string s in p.RHS)
                {
                    output += firstRule(s) + "`";
                }
            }
            return output;
        }

       public string removeMultipleCharacter(string str)
        {
            if (str != null)
            {
                if (str.IndexOf('`') >= 0)
                {
                    string output = "";
                    string[] temp = str.Split('`');

                    for (int j = 0; j < temp.Length; j++)
                    {
                        int s = 0;

                        for (int k = j + 1; k < temp.Length; k++)
                        {
                            if (temp[j] == temp[k])
                            {
                                s++;
                                temp[k] = "";
                            }
                        }
                        if (j == temp.Length - 1)
                        {
                            if (temp[j] != "")
                                output += temp[j];
                        }
                        else
                        {
                            if (temp[j] != "")
                                output += temp[j] + ",";
                        }

                    }
                    return output;
                }
                else
                    return str;
            }
            else
                return null;
        }

        private List<string> RemoveUnitPro(Production p)
        {
            List<string> newRHS = new List<string>();
            foreach (string s in p.RHS)
            {
                if (isTerminalString(s))
                    newRHS.Add(s);
                else
                {
                    if (nonTerminals.Contains(s))
                    {
                        Production pro = returnProduction(s);
                        List<string> RHS = RemoveUnitPro(pro);

                        foreach (string ss in RHS)
                            newRHS.Add(ss);
                    }
                    else
                        newRHS.Add(s);
                }
            }

            return newRHS;
        }

        public Grammar eliminateDependency()
        {
            Grammar g = new Grammar();
            foreach (Production p in productions)
            {
                Production newProduction = new Production();
                newProduction.LHS = p.LHS;
                newProduction.RHS =(List<string>)RemoveUnitPro(p);
                g.productions.Add(newProduction);

            }
            return g;
        }

        public string firstRule(String rule)
        {
            if (rule != "")
            {
                string firstCharacter = rule[0].ToString();

                if (firstCharacter == "^")
                    return "^";
                else
                {
                    if (Convert.ToInt16(Convert.ToChar(firstCharacter)) < 65 || Convert.ToInt16(Convert.ToChar(firstCharacter)) > 90) //if first character is terminal
                        return firstCharacter;
                    else
                    {
                        Production TerminalCharacter = null; //if there is terminal character in R.H.S find First of that Non Terminal
                        foreach (Production p in productions)
                        {
                            if (p.LHS == firstCharacter)
                            {
                                TerminalCharacter = p;
                                TerminalCharacter.LHS = p.LHS;
                                TerminalCharacter.RHS = p.RHS;
                                break;
                            }

                        }

                        string output = "";
                        if (TerminalCharacter != parentProduction)
                            output = findFirst(TerminalCharacter);
                        else
                        {
                            if (TerminalCharacter.RHS.Contains("^"))
                                output = firstRule(rule.Substring(1));
                        }

                        if (output.IndexOf("^") >= 0)
                        {
                            output = output.Remove(output.IndexOf("^"), 1);
                            return (output + firstRule(rule.Substring(1)) + "`");
                        }
                        else
                            return output;
                    }
                }

            }
            else
                return "";

        }

        public string saveToFile(XDocument xd = null, XElement root = null, string name = "")//override it
        {
            //Arg description:
            //Name :  to generate temp files automatically without requiring users to enter filename or to save the opened file

            if (name == "")
            {
                Microsoft.Win32.SaveFileDialog d = new Microsoft.Win32.SaveFileDialog();
                d.AddExtension = true;
                d.CheckPathExists = true;
                d.DefaultExt = "gmr";
                d.Filter = "Grammar files (*.gmr)|*.gmr";
                d.OverwritePrompt = true;
                if (d.ShowDialog() != false) // cancel button is not pressed
                    name = d.FileName;
                else
                    return "";
            }

            xd = new XDocument();
            root = new XElement("Grammar");
            root.Add(new XAttribute("type", "Cfg"));
            root.Add(new XAttribute("width", canvasMain.ActualWidth));
            root.Add(new XAttribute("height", canvasMain.ActualHeight));

            
            XElement xProductions = new XElement("Productions");

            xProductions.Add(new XAttribute("Count", productions.Count));
            foreach (Production p in productions)
            {
                XElement x = new XElement("Production");
                x.Add(new XAttribute("LHS", p.LHS));
                if (this.startProduction != null)
                {
                    if (p.LHS == this.startProduction.LHS)
                        x.Add(new XAttribute("IsStarting", true));
                    else
                        x.Add(new XAttribute("IsStarting", false));
                }
                else
                    x.Add(new XAttribute("IsStarting", false));

                int counter = 0; ;
                string rhs = "";
                foreach (string s in p.RHS)
                {
                    if (counter < p.RHS.Count - 1)
                    {
                        rhs += s + "|";
                        counter++;
                    }
                    else
                        rhs += s;
                }
                x.Add(new XAttribute("RHS", rhs));

                xProductions.Add(x);
            }
            root.Add(xProductions);
            xd.Add(root);

            xd.Save(name);
            return name;

        }

        public virtual void loadFromFile(XDocument doc) //override it
        {
            var query = from production in doc.Root.Element("Productions").Elements("Production")
                        select production;

            canvasMain.Height = Convert.ToDouble(doc.Root.Attribute("height").Value);
            canvasMain.Width = Convert.ToDouble(doc.Root.Attribute("width").Value);

            foreach (XElement s in query)
            {
                Production p = new Production();
                TextBox tb1 = new TextBox();
                tb1.Text = s.Attribute("LHS").Value.ToString();
                p.LHS = tb1.Text;

                string isStartingProduction = s.Attribute("IsStarting").Value.ToString();

                if (isStartingProduction=="true")
                    this.startProduction = p;
            
                Label lbl = new Label();
                lbl.Content = "->";

                TextBox tb2 = new TextBox();
                tb2.Text = s.Attribute("RHS").Value.ToString();
                String[] str = tb2.Text.Split('|');
                foreach (string data in str)
                    p.RHS.Add(data);

              //  loadedGM.productions.Add(p);

                StackPanel sp = new StackPanel();
                sp.Children.Add(tb1);
                sp.Children.Add(lbl);
                sp.Children.Add(tb2);

                this.addProduction(sp);
            }

        }
    }


    class Production
    {
        public string LHS;
        public List<string> RHS=new List<string>();
    }

}
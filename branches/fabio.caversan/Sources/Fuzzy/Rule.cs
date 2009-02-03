// AForge Fuzzy Library
// AForge.NET framework
//
// Copyright � Andrew Kirillov, 2005-2008 
// andrew.kirillov@gmail.com 
//
// Copyright � Fabio L. Caversan, 2008-2009
// fabio.caversan@gmail.com
//

namespace AForge.Fuzzy
{
    using System;
    using System.Collections.Generic;

    // TODO: document the class properly
    /// <summary>
    /// The Fuzzy Rule. This class will receive a rule expression and converts it to a RPN (Reverse Polish Notation)
    /// so the firing strengh of the rule can be easly calculated. 
    /// </summary>
    public class Rule
    {
        // name of the rule 
        private string name;
        // the original expression 
        private string rule;
        // the parsed RPN (reverse polish notation) expression
        private List<object> rpnTokenList;
        // the database with the linguistic variables
        private Database database;

        public Rule( Database FuzzyDatabase, string Name, string Rule )
        {
            // the list with the RPN expression
            rpnTokenList  = new List<object>( );
            
            // setting attributes
            this.name     = Name;
            this.rule     = Rule;
            this.database = FuzzyDatabase;
            
            // parsing the rule to obtain RPN of the expression
            ParseRule( );
        }

        /// <summary>
        /// Converts the RPN fuzzy expression into a string representation.
        /// </summary>
        /// 
        /// <returns>String representation of the RPN fuzzy expression.</returns>
        /// 
        public string GetRPNExpression( )
        {
            string result = "";
            foreach ( object o in rpnTokenList )
            {
                // if its a fuzzy clause we can call clause's ToString()
                if ( o.GetType( ) == typeof( Clause ) )
                {
                    Clause c = o as Clause;
                    result += c.ToString( );
                }
                else
                    result += o.ToString( );
                result += ", ";
            }
            result += "#";
            result = result.Replace( ", #", "" );
            return result;
        }
        
        /// <summary>
        /// Defines the priority of the fuzzy operators.
        /// </summary>
        /// 
        /// <param name="Operator">A fuzzy operator or openning parenthesis.</param>
        /// 
        /// <returns>A number indicating the priority of the operator, and zero for openning parenthesis.</returns>
        /// 
        private int Priority( string Operator )
        {
            switch ( Operator )
            {
                case "OR": return 1;
                case "AND": return 2;
            }
            return 0;
        }

        /// <summary>
        /// Converts the Fuzzy Rule to RPN (Reverse Polish Notation). For debug proposes, the string representation of the 
        /// RPN expression can be acessed by calling GetRPNExpression() method.
        /// </summary>
        private void ParseRule(  )
        {
            // tokens like IF and THEN will be searched always in upper case
            string upRule = rule.ToUpper( );
            
            // the rule must start with IF, and must have a THEN somewhere
            if ( ! upRule.StartsWith( "IF" ) )
                throw new ArgumentException( "A Fuzzy Rule must start with an IF statement." );
            if ( upRule.IndexOf( "THEN" ) < 0 )
                throw new ArgumentException( "Missing the consequent (THEN) statement." );

            // building a list with all the expression (rule) string tokens
            string spacedRule = rule.Replace( "(", " ( " ).Replace( ")", " ) " );
            string [] tokensList = spacedRule.Split( ' ' );

            // stack to convert to RPN
            Stack<string> s = new Stack<string>( );
            // storing the last token
            string lastToken = "IF";
            // linguistic var read, used to build clause
            LinguisticVariable lingVar = null;

            // verifying each token
            for ( int i = 0; i < tokensList.Length; i++ )
            {
                // removing spaces
                string token = tokensList[i].Trim( );
                // getting upper case
                string upToken = token.ToUpper( );

                // ignoring these tokens
                if ( upToken == "" || upToken == "IF" ) continue;

                // ending token
                if ( upToken == "THEN" ) break;

                // if we got a linguistic variable, an IS statement and a label is needed
                if ( lastToken == "VAR" )
                {
                    if ( upToken == "IS" )
                        lastToken = upToken;
                    else
                        throw new ArgumentException( "An IS statement is expected after a linguistic variable." );
                }
                // if we got an IS statement, a label must follow it
                else if ( lastToken == "IS" )
                {
                    try
                    {
                        FuzzySet fs = lingVar.GetLabel( token );
                        Clause c = new Clause( lingVar, fs );
                        rpnTokenList.Add( c );
                        lastToken = "LAB";
                    }
                    catch ( KeyNotFoundException )
                    {
                        throw new ArgumentException( "Linguistic label "+token+" was not found on the variable "+lingVar.Name+"." );
                    }
                }
                // not VAR and not IS statement 
                else
                {
                    // openning new scope
                    if ( upToken == "(" )
                    {
                        s.Push( upToken );
                        lastToken = upToken;
                    }
                    // operators
                    else if ( upToken == "AND" || upToken == "OR" )
                    {
                        // pop all the higher priority operators until the stack is empty 
                        while ( ( s.Count > 0 ) && ( Priority( s.Peek( ) ) > Priority( upToken ) ) )
                            rpnTokenList.Add( s.Pop() );

                        // pushing the operator    
                        s.Push( upToken );
                        lastToken = upToken;
                    }
                    // closing the scope
                    else if ( upToken == ")" )
                    {
                        // if there is nothing on the stack, an oppening parenthesis is missing.
                        if (s.Count == 0)
                            throw new ArgumentException( "Openning parenthesis missing." );
                        // pop the tokens and copy to output until openning is found 
                        while ( s.Peek() != "(" )
                        {
                            rpnTokenList.Add( s.Pop() );
                            if ( s.Count == 0 )
                                throw new ArgumentException( "Openning parenthesis missing." );
                        }
                        s.Pop( );

                        // saving last token...
                        lastToken = upToken;
                    }
                    // finally, the token is a variable
                    else
                    {
                        // find the variable
                        try
                        {
                            lingVar = database.GetVariable( token );
                            lastToken = "VAR";
                        }
                        catch ( KeyNotFoundException )
                        {
                            throw new ArgumentException( "Linguistic variable "+token+" was not found on the database." );
                        }
                    }

                }
            }

            // popping all operators left in stack
            while ( s.Count > 0 )
                rpnTokenList.Add( s.Pop( ) );

        }
    }
}


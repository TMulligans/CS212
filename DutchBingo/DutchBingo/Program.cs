﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Bingo
{
    class Program
    {
        private static RelationshipGraph rg;

        // Read RelationshipGraph whose filename is passed in as a parameter.
        // Build a RelationshipGraph in RelationshipGraph rg
        private static void ReadRelationshipGraph(string filename)
        {
            rg = new RelationshipGraph();                           // create a new RelationshipGraph object

            string name = "";                                       // name of person currently being read
            int numPeople = 0;
            string[] values;
            Console.Write("Reading file " + filename + "\n");
            try
            {
                string input = System.IO.File.ReadAllText(filename);// read file
                input = input.Replace("\r", ";");                   // get rid of nasty carriage returns 
                input = input.Replace("\n", ";");                   // get rid of nasty new lines
                string[] inputItems = Regex.Split(input, @";\s*");  // parse out the relationships (separated by ;)
                foreach (string item in inputItems)
                {
                    if (item.Length > 2)                            // don't bother with empty relationships
                    {
                        values = Regex.Split(item, @"\s*:\s*");     // parse out relationship:name
                        if (values[0] == "name")                    // name:[personname] indicates start of new person
                        {
                            name = values[1];                       // remember name for future relationships
                            rg.AddNode(name);                       // create the node
                            numPeople++;
                        }
                        else
                        {
                            rg.AddEdge(name, values[1], values[0]); // add relationship (name1, name2, relationship)

                            // handle symmetric relationships -- add the other way
                            if (values[0] == "hasSpouse" || values[0] == "hasFriend")
                                rg.AddEdge(values[1], name, values[0]);

                            // for parent relationships add child as well
                            else if (values[0] == "hasParent")
                                rg.AddEdge(values[1], name, "hasChild");
                            else if (values[0] == "hasChild")
                                rg.AddEdge(values[1], name, "hasParent");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("Unable to read file {0}: {1}\n", filename, e.ToString());
            }
            Console.WriteLine(numPeople + " people read");
        }

        // Show the relationships a person is involved in
        private static void ShowPerson(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
                Console.Write(n.ToString());
            else
                Console.WriteLine("{0} not found", name);
        }

        // Show a person's friends
        private static void ShowFriends(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
            {
                Console.Write("{0}'s friends: ", name);
                List<GraphEdge> friendEdges = n.GetEdges("hasFriend");
                foreach (GraphEdge e in friendEdges) {
                    Console.Write("{0} ", e.To());
                }
                Console.WriteLine();
            }
            else
                Console.WriteLine("{0} not found", name);
        }

        //show orphans
        private static void ShowOphans() {
            Console.Write(" \nThose who are Orphans: \n \n");
            foreach (GraphNode n in rg.nodes) {
                List<GraphEdge> parentsEdge = n.GetEdges("hasParent");
                if (parentsEdge.Count == 0) {
                    Console.Write(n.Name);
                    Console.WriteLine();
                }
            }
        }


        private static void bingo(string beginning, string ending) {
            GraphNode start = rg.GetNode(beginning);
            GraphNode end = rg.GetNode(ending);
            foreach (GraphNode e in rg.nodes) {
                if (e == start)
                {
                    e.setDistance(0);
                }
                else {
                    e.setDistance(2147483647);
                }
                
            }

        }

        private static void descendants(string name, int level) {
            GraphNode n = rg.GetNode(name);
            List<GraphEdge> childEdge = n.GetEdges("hasChild");
            if (level == 0) {
                Console.Write("The Descendants of ");
                Console.Write(name);
                Console.Write("\n\n");
                Console.WriteLine();
            } else if (level == 1) {
                Console.Write("Child --> ");
                Console.Write(name);
               // Console.Write("\n");
                Console.WriteLine();
            } else if (level == 2) {
                Console.Write("Grandchild --> ");
                Console.Write(name);
               // Console.Write("\n");
                Console.WriteLine();
            } else {
                for (int i = 2; i < level; i++) {
                    Console.Write("Great ");
                }
                Console.Write("Grandchild --> ");
                Console.Write(name);
              //  Console.Write("\n");
                Console.WriteLine();
            }
            foreach(GraphEdge e in childEdge){
                descendants(e.To(), level +1);

            }
        }


        // accept, parse, and execute user commands
        private static void CommandLoop()
        {
            string command = "";
            string[] commandWords;
            Console.Write("Welcome to Mulligan's Dutch Bingo Bananza!\n");

            while (command != "exit")
            {
                Console.Write("\nEnter a command: ");
                command = Console.ReadLine();
                commandWords = Regex.Split(command, @"\s+");        // split input into array of words
                command = commandWords[0];

                if (command == "exit")
                    ;                                               // do nothing

                // read a relationship graph from a file
                else if (command == "read" && commandWords.Length > 1)
                    ReadRelationshipGraph(commandWords[1]);

                // show information for one person
                else if (command == "show" && commandWords.Length > 1)
                    ShowPerson(commandWords[1]);

                else if (command == "friends" && commandWords.Length > 1)
                    ShowFriends(commandWords[1]);

                // dump command prints out the graph
                else if (command == "dump")
                    rg.Dump();

                else if (command == "orphans") {
                    ShowOphans();
                }

                else if (command == "bingo" && commandWords.Length > 1) {
                    bingo(commandWords[1], commandWords[2]);
                }
                else if (command == "descendants" && commandWords.Length > 1) {
                    descendants(commandWords[1], 0);
                }

                // illegal command
                else
                    Console.Write("\nLegal commands: read [filename], dump, show [personname],\n  friends [person name], orphans, descendants [person name], exit\n");
            }
        }

        static void Main(string[] args)
        {
            CommandLoop();
        }
    }
}

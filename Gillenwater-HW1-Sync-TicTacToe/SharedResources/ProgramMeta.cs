using System;

namespace SharedResources
{
    public static class ProgramMeta
    {
        public const int PORT_NUMBER = 11000;

        private const string AUTHOR = "Jake Gillenwater";
        private const string DEPARTMENT = "ETSU Department of Computing";
        private const string COURSE = "CSCI-5150-940";
        private const string ASSIGNMENT = "Homework 1 : Synchronous Tic-Tac-Toe";

        public static string GetProgramHeaderInfo() {
            return string.Join(Environment.NewLine, AUTHOR, DEPARTMENT, COURSE, ASSIGNMENT);
        }
    }
}

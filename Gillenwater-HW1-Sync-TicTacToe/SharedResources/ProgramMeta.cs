using System;

namespace SharedResources
{
    /// <summary>
    /// Stores constants relevant to both client and server projects.
    /// Mostly status code references and program meta data.
    /// </summary>
    public static class ProgramMeta
    {

        // Extra data for generating program header info
        private const string AUTHOR = "Jake Gillenwater";
        private const string DEPARTMENT = "ETSU Department of Computing";
        private const string COURSE = "CSCI-5150-940";
        private const string ASSIGNMENT = "Midterm : Part 2: gRPC Tic-Tac-Toe";

        /// <summary>
        /// Combines program meta data into a string that can be printed directed to the console to display the program's meta data.
        /// </summary>
        /// <returns>String of header info</returns>
        public static string GetProgramHeaderInfo() {
            return string.Join(Environment.NewLine, AUTHOR, DEPARTMENT, COURSE, ASSIGNMENT, "=========================");
        }
    }
}

﻿using System;

namespace SharedResources
{
    /// <summary>
    /// Stores constants relevant to both client and server projects.
    /// Mostly status code references and program meta data.
    /// </summary>
    public static class ProgramMeta
    {
        // Common reference to the program's operating port number
        // This ensures that the port number is always the same on the 
        // server and the client.
        public const int PORT_NUMBER = 11000;

        // HTTP status code reference: https://www.restapitutorial.com/httpstatuscodes.html
        public const string INPUT_ACCEPTED = "202"; // HTTP Accepted
        public const string GAMEBOARD_INCOMING = "100"; // HTTP Continue
        public const string PLAYER_MOVE = "402"; // HTTP Payment Required
        public const string GAME_RESULTS_INCOMING = "205"; // HTTP Reset Content
        public const string PROCEDE_WITH_DELIVERY = "200"; // HTTP Ok

        // Extra data for generating program header info
        private const string AUTHOR = "Jake Gillenwater";
        private const string DEPARTMENT = "ETSU Department of Computing";
        private const string COURSE = "CSCI-5150-940";
        private const string ASSIGNMENT = "Homework 1 : Synchronous Tic-Tac-Toe";

        /// <summary>
        /// Combines program meta data into a string that can be printed directed to the console to display the program's meta data.
        /// </summary>
        /// <returns>String of header info</returns>
        public static string GetProgramHeaderInfo() {
            return string.Join(Environment.NewLine, AUTHOR, DEPARTMENT, COURSE, ASSIGNMENT, "=========================");
        }
    }
}

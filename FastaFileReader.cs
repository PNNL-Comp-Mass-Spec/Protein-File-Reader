// This class can be used to open a Fasta file and return each protein present
//
// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
// Program started in January 2004
//
// E-mail: matthew.monroe@pnl.gov or proteomics@pnl.gov
// Website: https://panomics.pnl.gov/ or https://omics.pnl.gov
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace ProteinFileReader
{
    /// <summary>
    /// Class for reading *.fasta files
    /// </summary>
    public class FastaFileReader : ProteinFileReaderBaseClass
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks></remarks>
        public FastaFileReader()
        {
            InitializeLocalVariables();
        }

        /// <summary>
        /// Constructor that accepts a FASTA file path
        /// </summary>
        /// <param name="fastaFilePath"></param>
        public FastaFileReader(string fastaFilePath)
        {
            InitializeLocalVariables();
            OpenFile(fastaFilePath);
        }

        #region "Constants and Enums"

        /// <summary>
        /// Each protein description line in the Fasta file should start with a > symbol
        /// </summary>
        private const char PROTEIN_LINE_START_CHAR = '>';
        private const char PROTEIN_LINE_ACCESSION_TERMINATOR = ' ';

        #endregion

        #region "Classwide Variables"

        private char mProteinLineStartChar;
        private char mProteinLineAccessionEndChar;
        private udtProteinEntryType mNextEntry;

        #endregion

        #region "Interface Functions"

        /// <summary>
        /// Header line (protein name and description), without the start character (&lt;)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string HeaderLine
        {
            get { return GetHeaderLine(false); }
        }

        /// <summary>
        /// Character that precedes each header line (the line with the protein name and description)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public char ProteinLineStartChar
        {
            get { return mProteinLineStartChar; }
            set
            {
                if (value != 0)
                {
                    mProteinLineStartChar = value;
                }
            }
        }

        /// <summary>
        /// The character that follows the protein name; always a space
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public char ProteinLineAccessionEndChar
        {
            get { return mProteinLineAccessionEndChar; }
            set
            {
                if (value != 0)
                {
                    mProteinLineAccessionEndChar = value;
                }
            }
        }

        #endregion

        private string ExtractDescriptionFromHeader(string strHeaderLine)
        {
            var intCharLoc = 0;
            var strDescription = strHeaderLine;

            try
            {
                if (strHeaderLine.StartsWith(mProteinLineStartChar.ToString()))
                {
                    // Remove the > character from the start of the line
                    strHeaderLine = strHeaderLine.TrimStart(mProteinLineStartChar).Trim();
                }

                intCharLoc = strHeaderLine.IndexOf(mProteinLineAccessionEndChar);
                if (intCharLoc > 0)
                {
                    strDescription = strHeaderLine.Substring(intCharLoc + 1).Trim();
                }
                else
                {
                    strDescription = strHeaderLine;
                }
            }
            catch (Exception)
            {
                // Ignore any errors
            }

            return strDescription;
        }

        private string ExtractAccessionNameFromHeader(string strHeaderLine)
        {
            // Note: strHeaderLine should not start with the > character; it should have already been removed when the file was read
            // Look for mProteinLineAccessionEndChar in strHeaderLine

            var intCharLoc = 0;
            var strAccessionName = strHeaderLine;

            try
            {
                if (strHeaderLine.StartsWith(mProteinLineStartChar.ToString()))
                {
                    // Remove the > character from the start of the line
                    strHeaderLine = strHeaderLine.TrimStart(mProteinLineStartChar).Trim();
                }

                intCharLoc = strHeaderLine.IndexOf(mProteinLineAccessionEndChar);
                if (intCharLoc > 0)
                {
                    strAccessionName = strHeaderLine.Substring(0, intCharLoc).Trim();
                }
                else
                {
                    strAccessionName = strHeaderLine;
                }
            }
            catch (Exception)
            {
                // Ignore any errors
            }

            return strAccessionName;
        }

        /// <summary>
        /// Header line (protein name and description)
        /// </summary>
        /// <param name="includeStartChar">If true then include the start character (&lt;)</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string GetHeaderLine(bool includeStartChar)
        {
            try
            {
                if (!includeStartChar && mCurrentEntry.HeaderLine.StartsWith(mProteinLineStartChar.ToString()))
                {
                    // Remove the > character from the start of the line
                    return mCurrentEntry.HeaderLine.TrimStart(mProteinLineStartChar).Trim();
                }
                return mCurrentEntry.HeaderLine;
            }
            catch (Exception)
            {
                return mCurrentEntry.HeaderLine;
            }
        }

        private void InitializeLocalVariables()
        {
            mProteinLineStartChar = PROTEIN_LINE_START_CHAR;
            mProteinLineAccessionEndChar = PROTEIN_LINE_ACCESSION_TERMINATOR;

            mNextEntry.HeaderLine = string.Empty;
            mNextEntry.Name = string.Empty;
            mNextEntry.Description = string.Empty;
            mNextEntry.Sequence = string.Empty;
        }

        /// <summary>
        /// Reads the next entry in a Fasta file
        /// </summary>
        /// <returns>True if an entry is found, otherwise false</returns>
        /// <remarks></remarks>
        public override bool ReadNextProteinEntry()
        {
            string strLineIn = null;
            var blnProteinEntryFound = false;

            mCurrentEntry.HeaderLine = string.Empty;
            mCurrentEntry.Name = string.Empty;
            mCurrentEntry.Description = string.Empty;
            mCurrentEntry.Sequence = string.Empty;

            blnProteinEntryFound = false;
            mFileLineSkipCount = 0;
            // This is always 0 for Fasta files

            if (mProteinFileInputStream != null)
            {
                try
                {
                    while (!blnProteinEntryFound && !mProteinFileInputStream.EndOfStream)
                    {
                        if (!string.IsNullOrWhiteSpace(mNextEntry.HeaderLine))
                        {
                            strLineIn = mNextEntry.HeaderLine;
                        }
                        else
                        {
                            strLineIn = mProteinFileInputStream.ReadLine();
                            if (!string.IsNullOrWhiteSpace(strLineIn))
                            {
                                mFileBytesRead += strLineIn.Length + 2;
                                mFileLinesRead += 1;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(strLineIn))
                        {
                            strLineIn = strLineIn.Trim();

                            // See if strLineIn starts with the protein header start character
                            if (strLineIn.StartsWith(mProteinLineStartChar.ToString()))
                            {
                                mCurrentEntry.HeaderLine = strLineIn;
                                mCurrentEntry.Name = ExtractAccessionNameFromHeader(strLineIn);
                                mCurrentEntry.Description = ExtractDescriptionFromHeader(strLineIn);
                                mCurrentEntry.Sequence = string.Empty;
                                blnProteinEntryFound = true;

                                // Now continue reading until the next protein header start character is found
                                while (!mProteinFileInputStream.EndOfStream)
                                {
                                    strLineIn = mProteinFileInputStream.ReadLine();

                                    if (!string.IsNullOrWhiteSpace(strLineIn))
                                    {
                                        mFileBytesRead += strLineIn.Length + 2;
                                        mFileLinesRead += 1;

                                        strLineIn = strLineIn.Trim();

                                        if (strLineIn.StartsWith(mProteinLineStartChar.ToString()))
                                        {
                                            // Found the next protein entry
                                            // Store in mNextEntry and jump out of the loop
                                            mNextEntry.HeaderLine = strLineIn;
                                            break;
                                        }
                                        mCurrentEntry.Sequence += strLineIn;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Error reading the input file
                    // Ignore any errors
                    blnProteinEntryFound = false;
                }
            }

            return blnProteinEntryFound;
        }
    }
}

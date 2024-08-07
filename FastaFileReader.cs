﻿// This class can be used to open a FASTA file and return each protein present
//
// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
// Program started in January 2004
//
// E-mail: matthew.monroe@pnl.gov or proteomics@pnl.gov
// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://www.pnnl.gov/integrative-omics
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace ProteinFileReader
{
    /// <summary>
    /// Class for reading FASTA files
    /// </summary>
    public sealed class FastaFileReader : ProteinFileReaderBaseClass
    {
        // Ignore Spelling: Fasta

        /// <summary>
        /// Constructor
        /// </summary>
        public FastaFileReader()
        {
            mProteinResidues = new StringBuilder();
        }

        /// <summary>
        /// Constructor that accepts a FASTA file path
        /// </summary>
        /// <param name="fastaFilePath"></param>
        public FastaFileReader(string fastaFilePath)
        {
            mProteinResidues = new StringBuilder();
            OpenFile(fastaFilePath);
        }

        #region "Constants and Enums"

        /// <summary>
        /// Each protein description line in the FASTA file should start with a > symbol
        /// </summary>
        public const char PROTEIN_LINE_START_CHAR = '>';

        /// <summary>
        /// Character that denotes the end of the protein name
        /// </summary>
        public const char PROTEIN_LINE_ACCESSION_TERMINATOR = ' ';

        #endregion

        #region "Class wide Variables"

        /// <summary>
        /// Residues for the current entry
        /// </summary>
        /// <remarks>
        /// Copied to mCurrentEntry.Sequence once all residues have been read
        /// </remarks>
        private readonly StringBuilder mProteinResidues;

        private string mCachedHeaderLine = string.Empty;

        #endregion

        #region "Interface Functions"

        /// <summary>
        /// Header line (protein name and description), without the start character (&lt;)
        /// </summary>
        public override string HeaderLine => GetHeaderLine(false);

        /// <summary>
        /// Character that indicates the start of a data line with protein name (accession) and optionally description
        /// </summary>
        /// <remarks>
        /// Should be a '>'
        /// </remarks>
        public char ProteinLineStartChar { get; } = PROTEIN_LINE_START_CHAR;

        /// <summary>
        /// The character that follows the protein name
        /// </summary>
        /// <remarks>
        /// Should be a space, but we also match a non-breaking space (Ux00A0) and a tab
        /// \xfffd (the Unicode replacement character) is included to match a non-breaking space when reading an input file that does not specify an encoding
        /// </remarks>
        public char[] ProteinLineAccessionEndChars { get; } = { PROTEIN_LINE_ACCESSION_TERMINATOR, '\x00A0', '\xfffd', '\t' };

        #endregion

        private string ExtractDescriptionFromHeader(string headerLine)
        {
            var description = headerLine;

            try
            {
                if (headerLine.StartsWith(ProteinLineStartChar.ToString()))
                {
                    // Remove the > character from the start of the line
                    headerLine = headerLine.TrimStart(ProteinLineStartChar).Trim();
                }

                var charIndex = headerLine.IndexOfAny(ProteinLineAccessionEndChars);

                if (charIndex > 0)
                {
                    description = headerLine.Substring(charIndex + 1).Trim();
                }
                else
                {
                    description = headerLine;
                }
            }
            catch (Exception)
            {
                // Ignore any errors
            }

            return description;
        }

        /// <summary>
        /// Extract the accession name from the header line by looking for mProteinLineAccessionEndChar
        /// </summary>
        /// <remarks>
        /// HeaderLine should not start with the > character; it should have already been removed when the file was read
        /// </remarks>
        /// <param name="headerLine"></param>
        /// <returns>Accession (protein) name</returns>
        private string ExtractAccessionNameFromHeader(string headerLine)
        {
            try
            {
                if (headerLine.StartsWith(ProteinLineStartChar.ToString()))
                {
                    // Remove the > character from the start of the line
                    headerLine = headerLine.TrimStart(ProteinLineStartChar).Trim();
                }

                var charIndex = headerLine.IndexOfAny(ProteinLineAccessionEndChars);

                if (charIndex > 0)
                {
                    var proteinName = headerLine.Substring(0, charIndex).Trim();

                    if (headerLine[charIndex] == ' ')
                    {
                        return proteinName;
                    }

                    string characterName;

                    switch (headerLine[charIndex])
                    {
                        case '\t':
                            characterName = "a tab character";
                            break;

                        case '\x00A0':
                            characterName = "a non-breaking space";
                            break;

                        default:
                            characterName = "an unrecognized character (possibly a non-breaking space)";
                            break;
                    }

                    Console.WriteLine("Warning: Protein '{0}' has {1} after its name", proteinName, characterName);
                    return proteinName;
                }

                return headerLine;
            }
            catch (Exception)
            {
                // Ignore any errors; return the full line
                return headerLine;
            }
        }

        /// <summary>
        /// Header line (protein name and description)
        /// </summary>
        /// <param name="includeStartChar">If true then include the start character (&lt;)</param>
        public string GetHeaderLine(bool includeStartChar)
        {
            try
            {
                if (!includeStartChar && mCurrentEntry.HeaderLine.StartsWith(ProteinLineStartChar.ToString()))
                {
                    // Remove the > character from the start of the line
                    return mCurrentEntry.HeaderLine.TrimStart(ProteinLineStartChar).Trim();
                }
                return mCurrentEntry.HeaderLine;
            }
            catch (Exception)
            {
                return mCurrentEntry.HeaderLine;
            }
        }

        /// <summary>
        /// Reads the next entry in a FASTA file
        /// </summary>
        /// <returns>True if an entry is found, otherwise false</returns>
        public override bool ReadNextProteinEntry()
        {
            mCurrentEntry.Clear();

            var proteinEntryFound = false;

            // This is always 0 for FASTA files
            mFileLineSkipCount = 0;

            if (mProteinFileInputStream == null)
                return false;

            try
            {
                var proteinLineStartChar = ProteinLineStartChar.ToString();

                while (!proteinEntryFound && !mProteinFileInputStream.EndOfStream)
                {
                    string lineIn;

                    if (!string.IsNullOrWhiteSpace(mCachedHeaderLine))
                    {
                        lineIn = string.Copy(mCachedHeaderLine);
                        mCachedHeaderLine = string.Empty;
                    }
                    else
                    {
                        lineIn = mProteinFileInputStream.ReadLine();
                        mFileLinesRead++;

                        if (lineIn == null)
                        {
                            mFileBytesRead += 2;
                        }
                        else
                        {
                            mFileBytesRead += lineIn.Length + 2;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(lineIn))
                        continue;

                    var dataLine = lineIn.Trim();

                    // See if lineIn starts with the protein header start character
                    if (!dataLine.StartsWith(ProteinLineStartChar.ToString()))
                        continue;

                    mCurrentEntry.HeaderLine = dataLine;
                    mCurrentEntry.Name = ExtractAccessionNameFromHeader(dataLine);
                    mCurrentEntry.Description = ExtractDescriptionFromHeader(dataLine);
                    mCurrentEntry.Sequence = string.Empty;

                    mProteinResidues.Clear();

                    proteinEntryFound = true;

                    // Now continue reading until the next protein header start character is found
                    while (!mProteinFileInputStream.EndOfStream)
                    {
                        var lineIn2 = mProteinFileInputStream.ReadLine();
                        mFileLinesRead++;

                        if (lineIn2 == null)
                        {
                            mFileBytesRead += 2;
                            continue;
                        }

                        mFileBytesRead += lineIn2.Length + 2;

                        if (lineIn2.TrimStart().StartsWith(proteinLineStartChar))
                        {
                            // Found the next protein entry
                            // Store in mCachedHeaderLine and jump out of the loop
                            mCachedHeaderLine = string.Copy(lineIn2);
                            break;
                        }

                        if (DiscardProteinResidues)
                            continue;

                        // lineIn2 has additional residues for the current protein
                        mProteinResidues.Append(lineIn2.Trim());
                    }

                    mCurrentEntry.Sequence = mProteinResidues.ToString();
                }

                if (!proteinEntryFound)
                {
                    AdjustBytesReadForEOF();
                }
            }
            catch (Exception ex)
            {
                // Error reading the input file
                proteinEntryFound = false;

                Console.WriteLine("Exception in FastaFileReader.ReadNextProteinEntry: " + ex.Message);
            }

            return proteinEntryFound;
        }
    }
}

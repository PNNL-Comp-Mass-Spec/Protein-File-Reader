// This class can be used to open a Fasta file or delimited text file
//  with protein names and sequences and return each protein present
//
// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
// Program started in October 2004
//
// E-mail: matthew.monroe@pnl.gov or proteomics@pnl.gov
// Website: https://panomics.pnl.gov/ or https://omics.pnl.gov
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.IO;

[assembly: CLSCompliant(true)]

namespace ProteinFileReader
{
    /// <summary>
    /// Abstract base class for reading protein files
    /// </summary>
    public abstract class ProteinFileReaderBaseClass : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks></remarks>
        protected ProteinFileReaderBaseClass()
        {
            mCurrentEntry = new ProteinInfo("");
        }

        #region "Classwide Variables"

        /// <summary>
        /// Current entry being read/evaluated
        /// </summary>
        protected readonly ProteinInfo mCurrentEntry;

        /// <summary>
        /// Stream for reading the protein file
        /// </summary>
        protected StreamReader mProteinFileInputStream;

        private bool mFileOpen;

        /// <summary>
        /// Bytes read from the file
        /// </summary>
        protected long mFileBytesRead;

        /// <summary>
        /// Lines read from the file
        /// </summary>
        protected int mFileLinesRead;

        /// <summary>
        /// Lines skipped in the file
        /// </summary>
        protected int mFileLineSkipCount;

        #endregion

        #region "Interface Functions"

        /// <summary>
        /// Unique entry ID
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>Only used for delimited text files of peptides, with a eDelimitedFileFormatCode format that has a UniqueID column</remarks>
        public int EntryUniqueID => mCurrentEntry.UniqueID;

        /// <summary>
        /// Number of lines read
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int LinesRead => mFileLinesRead;

        /// <summary>
        /// Number of lines skipped due to having an invalid format
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int LineSkipCount => mFileLineSkipCount;

        /// <summary>
        /// Peptide discriminant score
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public float PeptideDiscriminantScore => mCurrentEntry.DiscriminantScore;

        /// <summary>
        /// Peptide mass
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double PeptideMass => mCurrentEntry.Mass;

        /// <summary>
        /// Peptide normalized elution time (NET)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public float PeptideNET => mCurrentEntry.NET;

        /// <summary>
        /// Standard deviation of peptide normalized elution time (NET)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public float PeptideNETStDev => mCurrentEntry.NETStDev;

        /// <summary>
        /// Protein name
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProteinName => mCurrentEntry.Name;

        /// <summary>
        /// Protein description
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProteinDescription => mCurrentEntry.Description;

        /// <summary>
        /// Protein sequence
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProteinSequence => mCurrentEntry.Sequence;

        /// <summary>
        /// Protein Name or Protein Name and Description
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>If file format is eDelimitedFileFormatCode.SequenceOnly, returns the protein sequence</remarks>
        public virtual string HeaderLine => mCurrentEntry.HeaderLine;

        #endregion

        /// <summary>
        /// Close the data file
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool CloseFile()
        {
            var blnSuccess = false;

            try
            {
                if (mProteinFileInputStream != null)
                {
                    mProteinFileInputStream.Close();
                }
                mFileOpen = false;
                mFileBytesRead = 0;
                mFileLinesRead = 0;
                blnSuccess = true;
            }
            catch (Exception)
            {
                blnSuccess = false;
            }

            return blnSuccess;
        }
        }

        /// <summary>
        /// Open the text file
        /// </summary>
        /// <param name="strInputFilePath"></param>
        /// <returns>True if success, false if a problem</returns>
        /// <remarks></remarks>
        public virtual bool OpenFile(string strInputFilePath)
        {
            var blnSuccess = false;

            try
            {
                if (CloseFile())
                {
                    mProteinFileInputStream = new StreamReader(new FileStream(strInputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                    blnSuccess = true;
                }
            }
            catch (IOException)
            {
                try
                {
                    // Try again, this time allowing for read/write access
                    mProteinFileInputStream = new StreamReader(new FileStream(strInputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    blnSuccess = true;
                }
                catch (Exception)
                {
                    blnSuccess = false;
                }
            }
            catch (Exception)
            {
                blnSuccess = false;
            }

            if (blnSuccess)
            {
                mFileOpen = true;
                mFileBytesRead = 0;
                mFileLinesRead = 0;
                mFileLineSkipCount = 0;
            }

            return blnSuccess;
        }

        /// <summary>
        /// Percent of the file that has been read
        /// </summary>
        /// <returns>Value between 0 and 100</returns>
        /// <remarks></remarks>
        public float PercentFileProcessed()
        {
            if (mFileOpen)
            {
                if (mProteinFileInputStream.BaseStream.Length > 0)
                {
                    return Convert.ToSingle(Math.Round((double)mFileBytesRead / mProteinFileInputStream.BaseStream.Length * 100, 2));
                }
                return 0;
            }
            return 0;
        }

        /// <summary>
        /// Look for the next protein entry
        /// </summary>
        /// <returns>True if an entry is found, otherwise false</returns>
        /// <remarks></remarks>
        public abstract bool ReadNextProteinEntry();

        /// <summary>
        /// Properly close open file handles
        /// </summary>
        public void Dispose()
        {
            if (mFileOpen)
                CloseFile();
        }
    }
}

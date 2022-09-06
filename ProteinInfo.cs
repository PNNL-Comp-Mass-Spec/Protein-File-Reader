
namespace ProteinFileReader
{
    /// <summary>
    /// This class is used both for Proteins and for Peptides
    /// Only Peptides from delimited text files use the Mass and NET fields
    /// </summary>
    public class ProteinInfo
    {
        /// <summary>
        /// For FASTA files, the header line, including the protein header start character; for Delimited files, the entire line
        /// </summary>
        public string HeaderLine { get; set; }

        /// <summary>
        /// Aka the accession name of the protein
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Protein description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Protein sequence
        /// </summary>
        public string Sequence { get; set; }

        /// <summary>
        /// For delimited text files listing peptides, the UniqueID of the peptide sequence
        /// </summary>
        public int UniqueID { get; set; }

        /// <summary>
        /// Protein mass
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// Protein Normalized Elution Time
        /// </summary>
        public float NET { get; set; }

        /// <summary>
        /// Standard deviation of the Normalized Elution Time
        /// </summary>
        public float NETStDev { get; set; }

        /// <summary>
        /// Discriminant score
        /// </summary>
        public float DiscriminantScore { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="proteinName"></param>
        public ProteinInfo(string proteinName)
        {
            Name = proteinName;
        }

        /// <summary>
        /// Clear all of the fields
        /// </summary>
        public void Clear()
        {
            HeaderLine = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            Sequence = string.Empty;
            UniqueID = 0;
            Mass = 0;
            NET = 0;
            NETStDev = 0;
            DiscriminantScore = 0;
        }

        /// <summary>
        /// Display the protein name
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }
}

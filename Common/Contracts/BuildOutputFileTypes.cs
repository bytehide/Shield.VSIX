namespace ShieldVSExtension.Common.Contracts
{
    public class BuildOutputFileTypes
    {
        /// <summary>
        /// Represents localized resource DLLs in an output group.
        /// </summary>
        public bool LocalizedResourceDlls { get; set; } = true;

        /// <summary>
        /// XML-serializer assemblies.
        /// </summary>
        public bool XmlSerializer { get; set; }

        /// <summary>
        /// Represents content files in an output group.
        /// </summary>
        public bool ContentFiles { get; set; }

        /// <summary>
        /// Represents built files in an output group.
        /// </summary>
        public bool Built { get; set; } = true;

        /// <summary>
        /// Represents source code files in an output group.
        /// </summary>
        public bool SourceFiles { get; set; }

        /// <summary>
        /// Represents a list of symbols in an output group.
        /// </summary>
        public bool Symbols { get; set; } = true;

        /// <summary>
        /// Represents documentation files in an output group.
        /// </summary>
        public bool Documentation { get; set; } = true;

        /// <summary>
        /// Checks whether all properties is <c>false</c>.
        /// </summary>
        public bool IsEmpty => !(LocalizedResourceDlls
                                 || XmlSerializer
                                 || ContentFiles
                                 || Built
                                 || SourceFiles
                                 || Symbols
                                 || Documentation);
    }
}

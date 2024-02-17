using System;
using System.Diagnostics;

namespace EmbyKinopoiskRu.Configuration
{
    /// <summary>
    /// Class for a Kinopoisk collection
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CollectionItem
    {
        /// <summary>
        /// Category of the collection
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Id of the collection
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Is the collection shoud be created flag
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// Name of the collection
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is CollectionItem a && string.Equals(Id, a.Id, StringComparison.InvariantCulture);
        }
        
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        private string DebuggerDisplay => $"{Id} - {Name} - {IsEnable}";

    }
}

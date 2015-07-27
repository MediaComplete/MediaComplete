using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store;
using LQuery = Lucene.Net.Search.Query;
using Version = Lucene.Net.Util.Version;
using System.Linq;
using System.Text;
using Lucene.Net.Search;
using System.Collections.Generic;

namespace MSOE.MediaComplete.Search.Lucene
{
    /// <summary>
    /// A Lucene-based search index.
    /// 
    /// <see cref="https://lucenenet.apache.org/"/>
    /// </summary>
    internal class LuceneIndex : IIndex
    {
        private Analyzer analyzer = new StandardAnalyzer(Version.LUCENE_30);
        private IndexWriter.MaxFieldLength maxLength = IndexWriter.MaxFieldLength.LIMITED;

        private readonly static object indexLock = new object();

        /// <summary>
        /// The id field of the search index. Used to match <see cref="Entry.Key"/>.
        /// </summary>
        public const string ID_FLD_NAME = "id";

        /// <summary>
        /// Gets or sets the index location. This may not be used in all implementations, and 
        /// will almost definitely not be thread-safe.
        /// </summary>
        /// <value>
        /// The index location.
        /// </value>
        public string IndexLocation
        {
            get
            {
                return _indexLocation;
            }
            set
            {
                _indexLocation = value;
            }
        }
        private string _indexLocation;

        /// <summary>
        /// Clears the entire index. Useful for when the entire index needs to rebuilt.
        /// </summary>
        public void Clear()
        {
            lock(indexLock)
            {
                using (var writer = new IndexWriter(FSDirectory.Open(IndexLocation), analyzer, maxLength))
                {
                    writer.DeleteAll();
                    writer.Commit();
                }
            }
        }

        /// <summary>
        /// Searches this index with the specified query.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <returns>A <see cref="Results"/> object</returns>
        public Results Search(Query query)
        {
            StringBuilder strQuery = new StringBuilder();
            bool first = true;
            foreach(var kvp in query.FieldQueries)
            {
                if (!first)
                {
                    strQuery.Append(" AND ");
                }
                strQuery.Append(kvp.Key);
                strQuery.Append(":\"");
                strQuery.Append(QueryParser.Escape(kvp.Value));
                strQuery.Append("\"");
                first = false;
            }

            var parser = new MultiFieldQueryParser(Version.LUCENE_30,
                query.FieldQueries.Keys.ToArray(), analyzer);
            LQuery q = parser.Parse(strQuery.ToString());
            IEnumerable<Document> hits = null;
            int count = 0;

            lock (indexLock)
            {
                using (var searcher = new IndexSearcher(FSDirectory.Open(IndexLocation), true))
                {
                    var topDocs = searcher.Search(q.Weight(searcher), null, int.MaxValue);
                    hits = topDocs.ScoreDocs.Select(d => searcher.Doc(d.Doc));
                    count = topDocs.TotalHits;
                }
            }

            return new Results()
            {
                Count = count,
                Hits = hits.Select(h => new Entry() {
                    Fields = h.GetFields().ToDictionary(f => f.Name, f => f.StringValue),
                    Key = h.GetField(ID_FLD_NAME).StringValue
                })
            };
        }

        /// <summary>
        /// Updates the index entries - either adds or updates them based on the key.
        /// </summary>
        /// <param name="entries">The entries.</param>
        public void UpdateEntries(params Entry[] entries)
        {
            var documents = new List<Document>();
            foreach (var entry in entries)
            {
                var doc = new Document();
                foreach (string fld in entry.Fields.Keys)
                {
                    doc.Add(new Field(fld, entry.Fields[fld], Field.Store.NO, Field.Index.ANALYZED));
                }
                doc.Add(new Field(ID_FLD_NAME, entry.Key, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
                documents.Add(doc);
            }

            lock (indexLock)
            {
                using (var writer = new IndexWriter(FSDirectory.Open(IndexLocation), analyzer, maxLength))
                {
                    foreach (var doc in documents)
                    {
                        writer.UpdateDocument(new Term(ID_FLD_NAME, doc.Get(ID_FLD_NAME)), doc);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the entries from the search index, by ID.
        /// Since the ID is the only thing used here, the other fields need not be filled out.
        /// </summary>
        /// <param name="entries">The entries.</param>
        public void RemoveEntries(params Entry[] entries)
        {
            IEnumerable<string> ids = entries.Select(e => e.Key);
            lock (indexLock)
            {
                using (var writer = new IndexWriter(FSDirectory.Open(IndexLocation), analyzer, maxLength))
                {
                    foreach (var id in ids)
                    {
                        writer.DeleteDocuments(new Term(ID_FLD_NAME, id));
                    }
                }
            }
        }
    }
}

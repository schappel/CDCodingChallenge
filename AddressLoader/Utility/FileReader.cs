using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AddressLoader.Utility
{
    public class FileReader<T> : IDisposable
    {
        private string filename;
        private StreamReader reader;

        public FileReader(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ApplicationException("Failed to load " + filename);
            this.filename = filename;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                reader.Dispose();

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion

        private void EnsureFileIsOpen()
        {
            if (reader == null) reader = new StreamReader(filename);
        }

        public IEnumerable<T> Take( int rows )
        {
            EnsureFileIsOpen();

            var result = new List<T>(rows);

            for( int i=0; i < rows; i++)
            {
                if (reader.EndOfStream) return result;  // shortcut to the end of the file
                result.Add(JsonConvert.DeserializeObject<T>(reader.ReadLine()));
            }
            return result;
        }

        public FileReader<T> Skip(int rows)
        {
            EnsureFileIsOpen();
            for (int i = 0; i < rows; i++)
            {
                reader.ReadLine();
            }
            return this;
        }

        public bool EndOfFile
        {
            get
            {
                if (reader == null) return false;
                return reader.EndOfStream;
            }
        }
    }
}

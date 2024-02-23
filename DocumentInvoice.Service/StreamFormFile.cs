using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentInvoice.Service
{
    public class StreamFormFile : IFormFile
    {
        private readonly Stream _stream;
        private readonly string _fileName;
        private readonly string _contentType;

        public StreamFormFile(Stream stream, string fileName, string contentType = null)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            _contentType = contentType;
        }

        public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{_fileName}\"";

        public string ContentType => _contentType ?? "application/octet-stream";

        public IHeaderDictionary Headers => new HeaderDictionary();

        public long Length => _stream.Length;

        public string Name => _fileName;

        public string FileName => _fileName;

        public void CopyTo(Stream target)
        {
            _stream.CopyTo(target);
        }

        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            await _stream.CopyToAsync(target, (int)_stream.Length, cancellationToken);
        }

        public Stream OpenReadStream()
        {
            // Since the stream might be read more than once, we need to reset its position.
            _stream.Position = 0;
            return _stream;
        }
    }
}

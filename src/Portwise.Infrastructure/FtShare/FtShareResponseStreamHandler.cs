using System.Net;

namespace Portwise.Infrastructure.FtShare;

/// <summary>
/// Classifies MCP response stream failures so the complete MCP exchange can be retried.
/// </summary>
internal sealed class FtShareResponseStreamHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (response.Content is null)
        {
            return response;
        }

        var originalContent = response.Content;
        try
        {
            var responseStream = await originalContent
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            response.Content = new FtShareResponseStreamContent(
                originalContent,
                new FtShareResponseStream(responseStream));
            return response;
        }
        catch (HttpRequestException exception)
        {
            response.Dispose();
            throw new FtShareResponseStreamException(
                "The FTShare MCP response stream could not be opened.",
                exception);
        }
        catch (IOException exception)
        {
            response.Dispose();
            throw new FtShareResponseStreamException(
                "The FTShare MCP response stream could not be opened.",
                exception);
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }
}

internal sealed class FtShareResponseStreamException(
    string message,
    Exception innerException) : Exception(message, innerException);

internal sealed class FtShareResponseStream(Stream innerStream) : Stream
{
    public override bool CanRead => innerStream.CanRead;

    public override bool CanSeek => innerStream.CanSeek;

    public override bool CanWrite => innerStream.CanWrite;

    public override long Length => innerStream.Length;

    public override long Position
    {
        get => innerStream.Position;
        set => innerStream.Position = value;
    }

    public override void Flush() => innerStream.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken)
        => innerStream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count)
    {
        try
        {
            return innerStream.Read(buffer, offset, count);
        }
        catch (HttpRequestException exception)
        {
            throw CreateReadException(exception);
        }
        catch (IOException exception)
        {
            throw CreateReadException(exception);
        }
    }

    public override int Read(Span<byte> buffer)
    {
        try
        {
            return innerStream.Read(buffer);
        }
        catch (HttpRequestException exception)
        {
            throw CreateReadException(exception);
        }
        catch (IOException exception)
        {
            throw CreateReadException(exception);
        }
    }

    public override async Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
    {
        try
        {
            return await innerStream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            throw CreateReadException(exception);
        }
        catch (IOException exception)
        {
            throw CreateReadException(exception);
        }
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await innerStream.ReadAsync(buffer, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            throw CreateReadException(exception);
        }
        catch (IOException exception)
        {
            throw CreateReadException(exception);
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
        => innerStream.Seek(offset, origin);

    public override void SetLength(long value) => innerStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
        => innerStream.Write(buffer, offset, count);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            innerStream.Dispose();
        }

        base.Dispose(disposing);
    }

    private static FtShareResponseStreamException CreateReadException(Exception exception)
        => new("The FTShare MCP response stream was interrupted.", exception);
}

internal sealed class FtShareResponseStreamContent : HttpContent
{
    private readonly HttpContent innerContent;
    private readonly Stream responseStream;
    private readonly long? contentLength;

    public FtShareResponseStreamContent(HttpContent innerContent, Stream responseStream)
    {
        this.innerContent = innerContent;
        this.responseStream = responseStream;
        contentLength = innerContent.Headers.ContentLength;
        foreach (var header in innerContent.Headers)
        {
            if (!string.Equals(header.Key, "Content-Length", StringComparison.OrdinalIgnoreCase))
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (contentLength is { } length)
        {
            Headers.ContentLength = length;
        }
    }

    protected override Task SerializeToStreamAsync(
        Stream stream,
        TransportContext? context)
        => SerializeToStreamAsync(stream, context, CancellationToken.None);

    protected override Task SerializeToStreamAsync(
        Stream stream,
        TransportContext? context,
        CancellationToken cancellationToken)
        => responseStream.CopyToAsync(stream, cancellationToken);

    protected override bool TryComputeLength(out long length)
    {
        if (contentLength is long knownLength)
        {
            length = knownLength;
            return true;
        }

        length = 0;
        return false;
    }

    protected override Task<Stream> CreateContentReadStreamAsync()
        => Task.FromResult(responseStream);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            responseStream.Dispose();
            innerContent.Dispose();
        }

        base.Dispose(disposing);
    }
}

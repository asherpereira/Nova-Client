namespace Nova.Client.Core.Transfers;
public interface IFileTransferService { Task<FileTransferResult> UploadAsync(Stream content,string fileName,IProgress<long>? progress=null,CancellationToken cancellationToken=default); Task DownloadAsync(Uri source,Stream destination,IProgress<long>? progress=null,CancellationToken cancellationToken=default); Task CancelAsync(string transferId,CancellationToken cancellationToken=default); }
public sealed record FileTransferResult(string TransferId,Uri? RemoteUri,long BytesTransferred,bool Verified);

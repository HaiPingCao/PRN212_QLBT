using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.IO;
using System.Text.Json;

namespace QLBT.Client.Services
{
    internal static class FileReceiver
    {
        /// <summary>
        /// Sends a request, receives sequential FILE_CHUNK_DOWN messages,
        /// assembles the file, and saves it to saveFolder.
        /// Returns the saved file path, null on failure.
        /// </summary>
        public static string? Receive(TcpClient client, Message requestMsg, string saveFolder)
        {
            client.SendMessage(requestMsg);

            var chunks = new Dictionary<int, byte[]>();
            string? fileName = null;

            while (true)
            {
                var response = client.WaitForNext(timeoutMs: 10000);
                if (response == null) return null;

                if (!response.Success) return null;

                var inner = (response.Data as JsonElement?)
                    ?.Deserialize<Message>(JsonHandle.JsonOpts);
                if (inner == null) return null;

                if (inner.Type == Command.FILE_CHUNK_DOWN)
                {
                    var chunkData = (inner.Data as JsonElement?)
                        ?.Deserialize<FileChunkDownData>(JsonHandle.JsonOpts);
                    if (chunkData == null) return null;

                    chunks[chunkData.ChunkIndex] = Convert.FromBase64String(chunkData.Base64Data);
                }
                else if (inner.Type == Command.FILE_END_DOWN)
                {
                    var endData = (inner.Data as JsonElement?)
                        ?.Deserialize<FileEndDownData>(JsonHandle.JsonOpts);
                    if (endData == null) return null;

                    fileName = endData.FileName;
                    break;
                }
            }

            if (fileName == null) return null;

            var allBytes = chunks
                .OrderBy(kv => kv.Key)
                .SelectMany(kv => kv.Value)
                .ToArray();

            Directory.CreateDirectory(saveFolder);
            var savePath = Path.Combine(saveFolder, fileName);
            File.WriteAllBytes(savePath, allBytes);

            return savePath;
        }
    }
}
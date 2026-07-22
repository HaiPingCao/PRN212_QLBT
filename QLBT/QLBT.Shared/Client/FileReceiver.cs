using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.Text.Json;

namespace QLBT.Shared.Client
{
    /// <summary>Dung chung cho Teacher/Student client de nhan file duoc server gui dang FileChunkDown/FileEndDown.</summary>
    public static class FileReceiver
    {
        /// <summary>
        /// Sends a request, receives sequential FileChunkDown messages,
        /// assembles the file, and saves it to saveFolder. Returns the saved file path, null on failure.
        /// </summary>
        public static string? Receive(TcpClient client, Message requestMsg, string saveFolder)
        {
            client.SendMessage(requestMsg);

            var chunks = new Dictionary<int, byte[]>();
            string? fileName = null;

            while (true)
            {
                var response = client.WaitForNext(timeoutMs: 10000);
                if (response == null || !response.Success) return null;

                var inner = (response.Data as JsonElement?)?.Deserialize<Message>(JsonHandle.JsonOpts);
                if (inner == null) return null;

                if (inner.Type == Command.FileChunkDown)
                {
                    var chunkData = (inner.Data as JsonElement?)?.Deserialize<FileChunkDownData>(JsonHandle.JsonOpts);
                    if (chunkData == null) return null;

                    chunks[chunkData.ChunkIndex] = Convert.FromBase64String(chunkData.Base64Data);
                }
                else if (inner.Type == Command.FileEndDown)
                {
                    var endData = (inner.Data as JsonElement?)?.Deserialize<FileEndDownData>(JsonHandle.JsonOpts);
                    if (endData == null) return null;

                    fileName = endData.FileName;
                    break;
                }
            }

            if (fileName == null) return null;

            var allBytes = chunks.OrderBy(kv => kv.Key).SelectMany(kv => kv.Value).ToArray();

            Directory.CreateDirectory(saveFolder);
            var savePath = Path.Combine(saveFolder, fileName);
            File.WriteAllBytes(savePath, allBytes);

            return savePath;
        }
    }
}

using QLBT.Shared.Client;
using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.Text.Json;

namespace QLBT.Client.Teacher.Services
{
    /// <summary>Giao vien chi xem lop cua minh va danh sach sinh vien trong lop - viec quan ly lop
    /// (them/sua/xoa lop, them/xoa sinh vien) nam o man hinh admin cuc bo cua QLBT.Server.</summary>
    public class ClassClientService
    {
        private readonly TcpClient _client;
        private readonly AuthClientService _auth;

        public ClassClientService(TcpClient client, AuthClientService auth)
        {
            _client = client;
            _auth = auth;
        }

        public List<ClassDto> GetClassList()
        {
            var response = _client.SendAndWait(new Message { Type = Command.GetClassList, Token = _auth.Token ?? "" });
            if (response == null || !response.Success) return new List<ClassDto>();
            return (response.Data as JsonElement?)?.Deserialize<List<ClassDto>>(JsonHandle.JsonOpts) ?? new List<ClassDto>();
        }

        public List<ClassEnrollmentDto> GetClassStudents(int classId)
        {
            var msg = new Message
            {
                Type = Command.GetClassStudents,
                Token = _auth.Token ?? "",
                Data = new GetClassStudentsRequest { ClassId = classId }
            };
            var response = _client.SendAndWait(msg);
            if (response == null || !response.Success) return new List<ClassEnrollmentDto>();
            return (response.Data as JsonElement?)?.Deserialize<List<ClassEnrollmentDto>>(JsonHandle.JsonOpts) ?? new List<ClassEnrollmentDto>();
        }
    }
}

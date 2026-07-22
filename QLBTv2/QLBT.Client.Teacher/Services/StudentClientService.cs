using QLBT.Shared.Client;
using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.Text.Json;

namespace QLBT.Client.Teacher.Services
{
    public class StudentClientService
    {
        private readonly TcpClient _client;
        private readonly AuthClientService _auth;

        public StudentClientService(TcpClient client, AuthClientService auth)
        {
            _client = client;
            _auth = auth;
        }

        public List<StudentDto> GetAll()
        {
            var response = _client.SendAndWait(new Message { Type = Command.GetStudentList, Token = _auth.Token ?? "" });
            if (response == null || !response.Success) return new List<StudentDto>();
            return (response.Data as JsonElement?)?.Deserialize<List<StudentDto>>(JsonHandle.JsonOpts) ?? new List<StudentDto>();
        }

        /// <summary>Tra ve (thanh cong, du lieu sinh vien neu thanh cong, thong bao loi neu that bai).</summary>
        public (bool Ok, StudentDto? Student, string Error) Create(CreateStudentRequest input)
        {
            var msg = new Message { Type = Command.CreateStudent, Token = _auth.Token ?? "", Data = input };
            var response = _client.SendAndWait(msg);
            if (response == null) return (false, null, "Không thể kết nối tới máy chủ.");
            if (!response.Success) return (false, null, response.Error);

            var student = (response.Data as JsonElement?)?.Deserialize<StudentDto>(JsonHandle.JsonOpts);
            return (true, student, "");
        }

        public (bool Ok, StudentDto? Student, string Error) Update(UpdateStudentRequest input)
        {
            var msg = new Message { Type = Command.UpdateStudent, Token = _auth.Token ?? "", Data = input };
            var response = _client.SendAndWait(msg);
            if (response == null) return (false, null, "Không thể kết nối tới máy chủ.");
            if (!response.Success) return (false, null, response.Error);

            var student = (response.Data as JsonElement?)?.Deserialize<StudentDto>(JsonHandle.JsonOpts);
            return (true, student, "");
        }
    }
}

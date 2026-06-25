using QLBT.Client.Services;
using QLBT.Shared.Transport;

var tcpClient = new TcpClient("127.0.0.1", 9000);
tcpClient.Connect();

var auth = new AuthClientService(tcpClient);
var assignment = new AssignmentClientService(tcpClient, auth);
var submission = new SubmissionClientService(tcpClient, auth);

// Test login
var ok = auth.Login("SV001", "1");
Console.WriteLine(ok ? $"Logged in: {auth.FullName}" : "Login failed");

// Test lấy danh sách bài tập
var list = assignment.GetAssignmentList();
list?.ForEach(bt => Console.WriteLine($"{bt.Id} - {bt.Title}"));
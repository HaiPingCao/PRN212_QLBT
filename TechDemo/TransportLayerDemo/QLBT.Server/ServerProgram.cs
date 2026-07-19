using QLBT.Server.Handlers;
using QLBT.Server.Models;
using QLBT.Server.Services;
using QLBT.Shared.Transport;

class ServerProgram
{
    static void Main(string[] args)
    {
        Prn212PQlbtContext db = new Prn212PQlbtContext();
        var authService = new AuthService(db);
        var assignmentService = new AssignmentService(db);
        var submissionService = new SubmissionService(db, assignmentService)
        {
            RootFolder = @"C:\QLBT_Submissions" // GV sẽ set qua UI sau
        };
        var tcpServer = new TcpServer("0.0.0.0", 9000);

        var handler = new MessageHandler(tcpServer, authService, assignmentService, submissionService);

        tcpServer.Start();

        Console.WriteLine("Nhan Enter de dung server...");
        Console.ReadLine();

        tcpServer.Stop();
    }
}
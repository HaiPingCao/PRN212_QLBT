using System;
using System.Collections.Generic;
using System.Text;

using QLBT.Server.Models;

namespace QLBT.Server.Services
{
    public interface IStudentService
    {
        List<StudentItem> GetAll();

        StudentItem Add(CreateStudentInput input);

        StudentItem? Update(UpdateStudentInput input);

        bool Delete(string mssv);

        List<LopOption> GetAllClasses();
    }
}

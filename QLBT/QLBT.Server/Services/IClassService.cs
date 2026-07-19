using System;
using System.Collections.Generic;
using System.Text;

using QLBT.Server.Models;

namespace QLBT.Server.Services
{
    public interface IClassService
    {
        List<ClassItem> GetAll();

        ClassItem Add(CreateClassInput input);

        ClassItem? Update(UpdateClassInput input);

        bool Delete(int id);
    }
}

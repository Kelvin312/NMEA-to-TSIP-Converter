using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class FileForm:ChartBaseForm
    {
        public FileForm(string filePath):base(Path.GetFileNameWithoutExtension(filePath))
        {
           
        }
    }
}
